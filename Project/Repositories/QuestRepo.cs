using CloudLogic.RepoModels.Items;
using CloudLogic.RepoModels.PlayerRewards;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CloudLogic.Repositories
{
    public class QuestRepo : BaseFireStoreService
    {
        public QuestRepo(ILogger logger) : base("Quest", logger)
        {
        }

        public async Task<List<Quest>> GetAll()
        {
            return await GetRecords<Quest>() ?? new List<Quest>();
        }

        public async Task<Quest> GetQuestById(string id)
        {
            return await GetRecordById<Quest>(id);
        }

        public async Task<Quest> StoreQuest(Quest quest)
        {
            return await StoreRecord(quest);
        }

        public async Task<Quest> Upsert(Quest quest)
        {
            if (quest == null) return null;
            await UpsertRecord(quest);

            // MergeAll doesn't delete missing map keys; explicitly update nested field to persist removals.
            if (quest.Reward != null && !string.IsNullOrWhiteSpace(quest.Id))
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                var docRef = _firestoreDb.Collection(_collectionName).Document(quest.Id);
                await docRef.UpdateAsync("Reward.GuaranteedRarities", quest.Reward.GuaranteedRarities, cancellationToken: cts.Token);
            }

            return quest;
        }


        public async Task DeleteQuest(string questId)
        {
            await DeleteRecordById(questId);
        }
    }
}
