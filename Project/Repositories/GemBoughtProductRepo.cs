using CloudLogic.RepoModels.Purchase;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CloudLogic.Repositories
{
    public class GemBoughtProductRepo : BaseFireStoreService
    {
        public GemBoughtProductRepo(ILogger logger) : base("GemBoughtProducts", logger)
        {
        }

        public Task Add(string playerId, string productId, long cost, long gemsAfter, Transaction tran)
        {
            return CreateRecord(new GemBoughtProduct
            {
                PlayerId = playerId,
                ProductId = productId,
                Cost = cost,
                GemsAfter = gemsAfter,
                CreatedAt = Timestamp.GetCurrentTimestamp()
            }, tran);
        }

        public async Task<List<GemBoughtProduct>> GetByPlayer(string playerId)
        {
            return await ReadRecordsByField<GemBoughtProduct>(nameof(GemBoughtProduct.PlayerId), playerId) ?? new List<GemBoughtProduct>();
        }
    }
}
