using CloudLogic.RepoModels.Shop;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace CloudLogic.Repositories
{
    public class ShopRepo : BaseFireStoreService
    {
        public ShopRepo(ILogger logger) : base("Shop", logger)
        {
        }

        public Task<List<Shop>> GetAll()
        {
            return GetRecords<Shop>() ?? Task.FromResult(new List<Shop>());
        }

        public Task<Shop> GetById(string id, Transaction tran = null)
        {
            return GetRecordById<Shop>(id, tran);
        }

        public async Task<Shop> Upsert(Shop item)
        {
            if (item == null) return null;
            await UpsertRecord(item);

            // BaseFireStoreService.UpsertRecord uses MergeAll and does not delete missing map keys.
            // We explicitly update the nested field so removals (or setting it to null) persist.
            if (item.Reward != null && !string.IsNullOrWhiteSpace(item.Id))
            {
                using var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(10));
                var docRef = _firestoreDb.Collection(_collectionName).Document(item.Id);
                await docRef.UpdateAsync("Reward.GuaranteedRarities", item.Reward.GuaranteedRarities, cancellationToken: cts.Token);
            }
            return item;
        }
    }
}

