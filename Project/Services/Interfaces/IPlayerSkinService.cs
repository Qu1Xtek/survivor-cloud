using CloudLogic.RepoModels.Items;
using CloudLogic.RepoModels.Player;
using CloudLogic.RepoModels.Skin;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CloudLogic.Services.Interfaces
{
    public interface IPlayerSkinService
    {
        Task<PlayerSkins> GetPlayerSkins(string playerId);
        Task<List<Skin>> GetPlayerOwnedSkins(string playerId);
        Task<Skin> CreateSkinPurchase(string playerId, string skinId, string skinName, ItemType skinItemType, string shopPurchaseRef);
        Task<PlayerSkins> EquipSkin(string playerId, string slotName, string skinId);
        Task<PlayerSkins> EquipSkins(string playerId, Dictionary<string, string> changes);
    }
}
