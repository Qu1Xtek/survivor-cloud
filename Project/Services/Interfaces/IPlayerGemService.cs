using System;
using System.Collections.Generic;
using CloudLogic.RepoModels.Gem;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
using CloudLogic.Response;

namespace CloudLogic.Services.Interfaces
{
    public interface IPlayerGemService
    {
        public Task<PlayerGems> GetPlayerGems(string playerId);
        public Task<UpgradeGemResponse> UpgradeGem(string playerId, GemType gemType, Transaction tran= null);
        //public Task<PlayerGems> SelectGemMilestonePower(string playerId, GemType gemType, int level, int index);
    }
}
