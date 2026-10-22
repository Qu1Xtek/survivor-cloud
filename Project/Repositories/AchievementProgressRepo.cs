using CloudLogic.RepoModels.Achievements;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudLogic.Repositories
{
    public class AchievementProgressRepo : BaseFireStoreService
    {
        public AchievementProgressRepo(ILogger logger) : base("AchievementsPerUser", logger)
        {
        }
        public static string BuildPlayerDocId(string playerId) => playerId;

        public static string BuildProgressId(string playerId, string achievementKey)
        {
            return $"{playerId}_{achievementKey}";
        }

        private async Task<T> RunPlayerDocTransaction<T>(
            string playerId,
            Func<AchievementProgress, Timestamp, T> mutateAndReturn)
        {
            var docId = BuildPlayerDocId(playerId);

            T result = default;

            await RunOurTransactionAsync(async tran =>
            {
                var now = Timestamp.FromDateTime(DateTime.UtcNow);
                var current = await GetRecordById<AchievementProgress>(docId, tran) ?? new AchievementProgress
                {
                    Id = docId,
                    PlayerId = playerId,
                    Stats = new Dictionary<string, long>(),
                    CompletedAchievements = new Dictionary<string, bool>(),
                    UpdatedAt = now
                };

                current.PlayerId = playerId;
                current.Stats ??= new Dictionary<string, long>();
                current.CompletedAchievements ??= new Dictionary<string, bool>();
                current.UpdatedAt = now;

                result = mutateAndReturn(current, now);
                await UpsertRecord(current, tran);
            });

            return result;
        }

        public async Task<List<AchievementProgress>> GetAllForPlayer(string playerId)
        {
            var single = await GetPlayerProgressDoc(playerId);
            return single == null ? new List<AchievementProgress>() : new List<AchievementProgress> { single };
        }

        public async Task<AchievementProgress> GetPlayerProgressDoc(string playerId)
        {
            if (string.IsNullOrWhiteSpace(playerId))
                return null;

            var id = BuildPlayerDocId(playerId);
            var existing = await GetRecordById<AchievementProgress>(id);

            if (existing != null)
                return existing;

            var created = new AchievementProgress
            {
                Id = id,
                PlayerId = playerId,
                Stats = new Dictionary<string, long>(),                
                CompletedAchievements = new Dictionary<string, bool>(),
                UpdatedAt = Timestamp.FromDateTime(DateTime.UtcNow)
            };

            await CreateUniqueRecord(created);

            return await GetRecordById<AchievementProgress>(id) ?? created;
        }

        public async Task<Dictionary<string,long>> SetStatIfMoreAtomic(string playerId, string statKey, long newValue)
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
        public async Task<AchievementProgress> IncrementStatAtomic(string playerId, string statKey, long delta)
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

            // Filter invalid keys early.
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

        public async Task<AchievementProgress> SetStatsAtomic(string playerId, Dictionary<string, long> newStats)
        {
            if (string.IsNullOrWhiteSpace(playerId) || newStats == null || newStats.Count == 0)
                return null;

            // Filter invalid keys early.
            var statsFiltered = newStats
                .Where(s => !string.IsNullOrWhiteSpace(s.Key) && s.Value != 0)
                .ToDictionary(s => s.Key, s => s.Value);

            return await RunPlayerDocTransaction(playerId, (current, _) =>
            {
                foreach (var stat in statsFiltered)
                {
                    current.Stats.TryGetValue(stat.Key, out var cur);                    
                    current.Stats[stat.Key] = stat.Value;                    
                }

                return current;
            });
        }

        public async Task MarkAchievementCompleted(string playerId, string achievementKey)
        {
            if (string.IsNullOrWhiteSpace(playerId) || string.IsNullOrWhiteSpace(achievementKey))
                return;

            await RunPlayerDocTransaction(playerId, (current, _) =>
            {
                current.CompletedAchievements ??= new Dictionary<string, bool>();
                current.CompletedAchievements[achievementKey] = true;
                return 0;
            });
        }

        /// <summary>
        /// Marks every eligible key claimed in one transaction and returns only the keys it newly marked.
        /// Eligibility is checked inside the transaction, so repeated or concurrent requests can't pay a reward twice.
        /// </summary>
        public async Task<List<string>> MarkAchievementsCompleted(string playerId,
            IEnumerable<string> keys,
            Func<AchievementProgress, string, bool> isReady)
        {
            var wanted = keys?.Where(k => !string.IsNullOrWhiteSpace(k)).Distinct().ToList() ?? new List<string>();

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

