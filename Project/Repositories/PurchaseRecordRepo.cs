using CloudLogic.RepoModels.Purchase;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CloudLogic.Repositories
{
    public class PurchaseRecordRepo : BaseFireStoreService
    {
        public PurchaseRecordRepo(ILogger logger) : base("PurchaseRecords", logger)
        {
        }

        // proof    = purchaseToken (Google) or transactionId (Apple)
        // record.OrderId must already be set (orderId for Google, transactionId for Apple)
        public async Task<bool> TryCreate(string proof, PurchaseRecord record)
        {
            record.Id            = record.OrderId;
            record.PurchaseToken = proof;

            var existing = await GetRecordById<PurchaseRecord>(record.OrderId);
            if (existing != null)
            {
                return false;
            }

            await CreateUniqueRecord(record);
            return true;
        }

        public async Task<PurchaseRecord> GetByToken(string purchaseToken)
        {
            var results = await ReadRecordsByField<PurchaseRecord>("PurchaseToken", purchaseToken);
            return results?.Count > 0 ? results[0] : null;
        }

        public async Task<List<PurchaseRecord>> GetByPlayer(string playerId)
        {
            return await ReadRecordsByField<PurchaseRecord>("PlayerId", playerId) ?? new List<PurchaseRecord>();
        }

        public async Task<PurchaseRecord> GetByOrderId(string orderId)
            => await GetRecordById<PurchaseRecord>(orderId);

        public async Task MarkRefunded(string orderId)
        {
            var record = await GetRecordById<PurchaseRecord>(orderId);
            if (record == null)
            {
                return;
            }

            record.Refunded = true;
            await UpdateRecord(record);
        }
    }
}
