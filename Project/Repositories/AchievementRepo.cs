using CloudLogic.RepoModels.Achievements;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace CloudLogic.Repositories
{
    public class AchievementRepo : BaseFireStoreService
    {
        public AchievementRepo(ILogger logger) : base("Achievements", logger)
        {
        }

        protected AchievementRepo(string dbSetName, ILogger logger) : base(dbSetName, logger)
        {
        }

        public async Task<List<Achievement>> GetAllActive()
        {
            var query = GetCollectionRef().WhereEqualTo(nameof(Achievement.IsActive), true);
            return await GetRecordsByQuery<Achievement>(query);
        }

        public async Task<List<Achievement>> GetAll()
        {
            return await GetRecords<Achievement>() ?? new List<Achievement>();
        }

        public async Task<List<Achievement>> GetActiveByStatKey(string statKey)
        {
            if (string.IsNullOrWhiteSpace(statKey))
                return new List<Achievement>();

            var query = GetCollectionRef()
                .WhereEqualTo(nameof(Achievement.IsActive), true)
                .WhereEqualTo(nameof(Achievement.StatKey), statKey);

            return await GetRecordsByQuery<Achievement>(query) ?? new List<Achievement>();
        }

        public async Task<Achievement> GetByKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return null;

            // We store achievements by docId == key for stable lookups.
            return await GetRecordById<Achievement>(key);
        }

        public async Task<Achievement> Upsert(Achievement achievement)
        {
            if (achievement == null)
                return null;

            if (string.IsNullOrWhiteSpace(achievement.Id))
            {
                // Enforce stable Id for definitions; fallback to Key if set.
                achievement.Id = achievement.Key;
            }

            await UpsertRecord(achievement);

            // UpsertRecord merges and does not delete missing map keys.
            // Explicitly update nested field so GuaranteedRarities removals persist.
            if (achievement.Reward != null && !string.IsNullOrWhiteSpace(achievement.Id))
            {
                using var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(10));
                var docRef = _firestoreDb.Collection(_collectionName).Document(achievement.Id);
                await docRef.UpdateAsync("Reward.GuaranteedRarities", achievement.Reward.GuaranteedRarities, cancellationToken: cts.Token);
            }

            return achievement;
        }
    }
}

