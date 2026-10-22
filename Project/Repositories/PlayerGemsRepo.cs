using CloudLogic.RepoModels.Gem;
using CloudLogic.RepoModels.Player;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace CloudLogic.Repositories
{
    public class PlayerGemsRepo : BaseFireStoreService
    {
        public PlayerGemsRepo(ILogger logger) : base("PlayerGems", logger)
        {
        }

        public virtual async Task<PlayerGems> GetPlayerGemsByPlayerId(string playerId, Transaction tran = null)
        {
            var result = await GetRecordById<PlayerGems>(playerId, tran);

            return result;
        }

        public virtual async Task<PlayerGems> AddPlayerGem(string playerId, PlayerGem gem, Transaction tran = null)
        {
            var result = await GetRecordById<PlayerGems>(playerId, tran);
            result.Gems[gem.GemType.ToString()] = gem;

            return await SavePlayerGems(result, tran);
        }

        public virtual async Task<PlayerGems> SavePlayerGems(PlayerGems playerGems, Transaction tran = null)
        {
            var result = await UpdateRecord(playerGems, tran);

            return result;
        }

        public async Task<PlayerGems> CreatePlayerGems(string playerId)
        {
            return await CreateRecord(new PlayerGems() 
            { Id = playerId
            });
        }
    }
}
