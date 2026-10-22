using CloudLogic.RepoModels.Player;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CloudLogic.Repositories
{
    public class PlayerInfoRepo : BaseFireStoreService
    {
        public PlayerInfoRepo(ILogger logger) : base("PlayerInfo", logger)
        {
        }

        public async Task LogRunStart(string playerId, Transaction tran = null)
        {
            try
            {
                var plrInfo = new PlayerInfo
                {
                    Id = playerId,
                    LastRunStartedAt = Google.Cloud.Firestore.Timestamp.FromDateTime(DateTime.UtcNow)
                };

                await UpsertRecord(plrInfo, tran);
            }
            catch (Exception ex)
            {
                
            }
        }

        public async Task LogLogin(string playerId, string playerName, Transaction tran = null)
        {
            try
            {
                var plrInfo = new PlayerInfo
                {
                    Id = playerId,
                    LastLoginAt = Google.Cloud.Firestore.Timestamp.FromDateTime(DateTime.UtcNow),
                    PlayerName = playerName,
                };

                await UpsertRecord(plrInfo, plrInfo, tran);
            }
            catch (Exception)
            {
            }

        }
    }
}
