using CloudLogic.RepoModels.Player;
using CloudLogic.RepoModels.TrailingModels;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CloudLogic.Repositories
{
    public class AuditRepo : BaseFireStoreService
    {
        public AuditRepo(ILogger logger) : base("AuditInfo", logger)
        {
        }

        public async Task AuditData(RunData data, string playerId)
        {
            try
            {
                var auditData = new AuditData
                {
                    TotalItemsDropped = data.ItemsDropped,
                    PlayerId = playerId,
                    DateCreated = Timestamp.FromDateTime(DateTime.UtcNow),
                };

                await CreateRecord(auditData);
            }
            catch (Exception ex)
            {
                
            }
        }       
    }
}
