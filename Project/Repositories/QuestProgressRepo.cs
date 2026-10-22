using CloudLogic.RepoModels.Achievements;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudLogic.Repositories
{
    /// <summary>
    /// THE Repo will use same model as achievemtn repot 
    /// </summary>
    public class QuestProgressRepo : BaseFireStoreService
    {
        public QuestProgressRepo(ILogger logger) : base("QuestsPerUser", logger)
        {
        }

        public static string BuildPlayerDocId(string playerId) => playerId;

        public static string BuildProgressId(string playerId, string questKey)
        {
            return $"{playerId}_{questKey}";
        }

        private static string GetToday()
        { 
            return DateTime.UtcNow.ToString("yyyy-MM-dd"); 
        }

        private async Task<T> RunPlayerDocTransaction<T>(
            string playerId,
            Func<QuestsProgress, Timestamp, T> mutateAndReturn)
        {
            var docId = BuildPlayerDocId(playerId);
            var today = GetToday();

            T result = default;

            await RunOurTransactionAsync(async tran =>
            {
                var now = Timestamp.FromDateTime(DateTime.UtcNow);
                var current = await GetRecordById<QuestsProgress>(docId, tran) ?? new QuestsProgress
                {
                    Id = docId,
                    PlayerId = playerId,
                    Stats = new Dictionary<string, long>(),
                    CompletedAchievements = new Dictionary<string, bool>(),
                    DayKey = today,
                    UpdatedAt = now
                };

                current.PlayerId = playerId;
                current.Stats ??= new Dictionary<string, long>();
                current.CompletedAchievements ??= new Dictionary<string, bool>();

                // Lazy daily reset: new UTC day => wipe progress before mutating.
                if (current.DayKey != today)
                {
                    current.Stats = new Dictionary<string, long>();
                    current.CompletedAchievements = new Dictionary<string, bool>();
                    current.ExplicitUpserts = new List<string> { nameof(current.Stats), nameof(current.CompletedAchievements) };
                    current.DayKey = today;
                }

                current.UpdatedAt = now;

                result = mutateAndReturn(current, now);
                await UpsertRecord(current, tran);
            });

            return result;
        }

        public async Task<QuestsProgress> GetProgressForPlayer(string playerId)
        {
            var single = await GetPlayerProgressDoc(playerId);

            return single ?? new QuestsProgress();
        }

        public async Task<QuestsProgress> GetPlayerProgressDoc(string playerId)
        {
            if (string.IsNullOrWhiteSpace(playerId))
                return null;

            var id = BuildPlayerDocId(playerId);
            var today = GetToday();
            var existing = await GetRecordById<QuestsProgress>(id);

            // Surface a reset view on a new day even if nothing has mutated yet.
            if (existing != null)
            {
                if (existing.DayKey != today)
                {
                    existing.Stats = new Dictionary<string, long>();
                    existing.CompletedAchievements = new Dictionary<string, bool>();
                    existing.DayKey = today;
                }

                return existing;
            }

            var created = new QuestsProgress
            {
                Id = id,
                PlayerId = playerId,
                Stats = new Dictionary<string, long>(),
                CompletedAchievements = new Dictionary<string, bool>(),
                DayKey = today,
                UpdatedAt = Timestamp.FromDateTime(DateTime.UtcNow)
            };

            await CreateUniqueRecord(created);

            return await GetRecordById<QuestsProgress>(id) ?? created;
        }

        public async Task<Dictionary<string, long>> SetStatIfMoreAtomic(string playerId, 
            string statKey,
            long newValue)
        {
            if (string.IsNullOrWhiteSpace(playerId) || string.IsNullOrWhiteSpace(statKey))
                return null;

            return await RunPlayerDocTransaction(playerId, (current, _) =>
            {
                current.Stats.TryGetValue(statKey, out var cur);

                var next = newValue > cur ? newValue : cur;
                current.Stats[statKey] = next;
                return current.Stats;
            });
        }

        public async Task<QuestsProgress> IncrementStatAtomic(string playerId, string statKey, long delta)
        {
            if (string.IsNullOrWhiteSpace(playerId) || string.IsNullOrWhiteSpace(statKey))
                return null;

            return await RunPlayerDocTransaction(playerId, (current, _) =>
            {
                current.Stats.TryGetValue(statKey, out var cur);
                var next = cur + delta;
                if (next < 0) next = 0;
                current.Stats[statKey] = next;
                return current;
            });
        }

        public async Task<Dictionary<string, long>> IncrementStatsAtomic(string playerId, Dictionary<string, long> additions)
        {
            if (string.IsNullOrWhiteSpace(playerId) || additions == null || additions.Count == 0)
                return new Dictionary<string, long>();

            var filteredStats = additions
                .Where(s => !string.IsNullOrWhiteSpace(s.Key) && s.Value != 0)
                .ToDictionary(s => s.Key, s => s.Value);

            return await RunPlayerDocTransaction(playerId, (current, _) =>
            {
                foreach (var stat in filteredStats)
                {
                    current.Stats.TryGetValue(stat.Key, out var cur);
                    var next = cur + stat.Value;
                    if (next < 0) next = 0;
                    current.Stats[stat.Key] = next;
                }
                return current.Stats;
            });
        }

        public async Task<QuestsProgress> SetStatsAtomic(string playerId, Dictionary<string, long> newStats)
        {
            if (string.IsNullOrWhiteSpace(playerId) || newStats == null || newStats.Count == 0)
                return null;

            var statsFiltered = newStats
                .Where(s => !string.IsNullOrWhiteSpace(s.Key) && s.Value != 0)
                .ToDictionary(s => s.Key, s => s.Value);

            return await RunPlayerDocTransaction(playerId, (current, _) =>
            {
                foreach (var stat in statsFiltered)
                {
                    current.Stats[stat.Key] = stat.Value;
                }

                return current;
            });
        }

        public async Task MarkQuestCompleted(string playerId, string questKey)
        {
            if (string.IsNullOrWhiteSpace(playerId) || string.IsNullOrWhiteSpace(questKey))
                return;

            await RunPlayerDocTransaction(playerId, (current, _) =>
            {
                current.CompletedAchievements ??= new Dictionary<string, bool>();
                current.CompletedAchievements[questKey] = true;
                return 0;
            });
        }

        /// <summary>
        /// Marks every eligible quest claimed in one transaction and returns only the keys it newly marked.
        /// Eligibility is checked inside the transaction (after the daily reset), so a claim that crosses
        /// midnight or arrives twice can't pay out.
        /// </summary>
        public async Task<List<string>> MarkQuestsCompleted(string playerId,
            IEnumerable<string> questKeys,
            Func<QuestsProgress, string, bool> isReady)
        {
            var wanted = questKeys?.Where(k => !string.IsNullOrWhiteSpace(k)).Distinct().ToList() ?? new List<string>();

            if (string.IsNullOrWhiteSpace(playerId) || wanted.Count == 0)
            {
                return new List<string>();
            }

            return await RunPlayerDocTransaction(playerId, (current, _) =>
            {
                var marked = new List<string>();

                foreach (var key in wanted)
                {
                    if (current.CompletedAchievements.ContainsKey(key) || !isReady(current, key))
                    {
                        continue;
                    }

                    current.CompletedAchievements[key] = true;
                    marked.Add(key);
                }

                return marked;
            });
        }
    }
}
