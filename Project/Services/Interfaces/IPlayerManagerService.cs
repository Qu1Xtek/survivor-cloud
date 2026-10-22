using CloudLogic.RepoModels.Items;
using CloudLogic.RepoModels.Player;
using CloudLogic.Response;
using CloudLogic.Util.ComModels;
using Google.Cloud.Firestore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.CloudCode.Core;

namespace CloudLogic.Services.Items
{
    public interface IPlayerManagerService
    {
        Task<PlayerData> AddRerollRune(string playerId);

        //Task<Item> StorePlayerItem(Item item, string playerId, Transaction tran = null);
        Task<PlayerData> GetPlayerDataAsync(string playerId, string? userName = null, Transaction tran = null);
        Task<PlayerData> LevelUp(IExecutionContext ctx);
        Task UpdateDeviceId(IExecutionContext ctx, string deviceId);

        Task<ResourceUpdate> TakeCraftingMaterialsAndGold(string playerId, long goldAmount, int craftingAmount, Transaction tran = null);

        Task<ResourceUpdate> UseRerollRune(string playerId);
        Task<bool> UpdatePlayerIdUsername(string playerId, string username);
        Task<PlayerData> UpsertPlayerData(PlayerData plrData, PlayerData oldPlrData, Transaction tran = null);
        Task UpsertPlayerData(PlayerData plrData, Transaction tran = null);
        Task RemovePLAYERDATAUSEDONLYFORTESTING(string playerId, Transaction tran = null);
        Task<PlayerData> IncreaseItemDropCountForId(string playerId, Transaction tran = null);
        Task<PlayerData> IncreaseItemDropCountForData(PlayerData plrData, Transaction tran = null);

        Task<bool> SetFirstRunStart(string playerId);

        Task SetPlatform(string playerId, string platform);
    }
}
