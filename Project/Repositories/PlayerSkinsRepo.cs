using CloudLogic.RepoModels.Player;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace CloudLogic.Repositories
{
    public class PlayerSkinsRepo : BaseFireStoreService
    {
        public PlayerSkinsRepo(ILogger logger) : base("PlayerSkins", logger)
        {
        }

        public virtual async Task<PlayerSkins> GetByPlayerId(string playerId, Transaction tran = null)
        {
            return await GetRecordById<PlayerSkins>(playerId, tran);
        }

        public virtual async Task<PlayerSkins> Upsert(PlayerSkins playerSkins, Transaction tran = null)
        {
            return await UpdateRecord(playerSkins, tran);
        }

        public async Task<PlayerSkins> Create(string playerId)
        {
            return await CreateRecord(new PlayerSkins { Id = playerId });
        }
    }
}
