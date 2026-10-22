using CloudLogic.RepoModels.Items;
using CloudLogic.RepoModels.Player;
using CloudLogic.RepoModels.Skin;
using CloudLogic.Repositories;
using CloudLogic.Services.Interfaces;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudLogic.Services.Player
{
    public class PlayerSkinService : IPlayerSkinService
    {
        private readonly PlayerSkinsRepo _repo;
        private readonly SkinsRepo _skinsRepo;
        private readonly ILogger _logger;

        public PlayerSkinService(PlayerSkinsRepo repo, SkinsRepo skinsRepo, ILogger logger)
        {
            _repo = repo;
            _skinsRepo = skinsRepo;
            _logger = logger;
        }

        public async Task<PlayerSkins> GetPlayerSkins(string playerId)
        {
            var skins = await _repo.GetByPlayerId(playerId);

            if (skins == null)
            {
                skins = await _repo.Create(playerId);
            }

            return skins;
        }

        public async Task<List<Skin>> GetPlayerOwnedSkins(string playerId)
        {
            return await _skinsRepo.GetByPlayerId(playerId);
        }

        public async Task<Skin> CreateSkinPurchase(string playerId, string skinId, string skinName, ItemType skinItemType, string shopPurchaseRef)
        {
            var purchase = new Skin
            {
                Id = $"{playerId}_{skinId}",
                PlayerId = playerId,
                SkinId = skinId,
                SkinName = skinName,
                ItemType = skinItemType,
                ShopPurchaseRef = shopPurchaseRef,
                DateCreated = Timestamp.GetCurrentTimestamp()
            };

            return await _skinsRepo.Create(purchase);
        }

        public async Task<PlayerSkins> EquipSkin(string playerId, string slotName, string skinId)
        {
            var purchases = await _skinsRepo.GetByPlayerId(playerId);

            if (!purchases.Any(p => p.SkinId == skinId))
            {
                throw new InvalidOperationException($"Skin '{skinId}' is not owned by player '{playerId}'.");
            }

            var skins = await GetPlayerSkins(playerId);
            SetEquippedSlot(skins.EquippedSkins, slotName, skinId);

            return await _repo.Upsert(skins);
        }

        public async Task<PlayerSkins> EquipSkins(string playerId, Dictionary<string, string> changes)
        {
            var purchases = await _skinsRepo.GetByPlayerId(playerId);
            var ownedIds = purchases.Select(p => p.SkinId).ToHashSet();
            var skins = await GetPlayerSkins(playerId);

            foreach (var change in changes)
            {
                if (string.IsNullOrEmpty(change.Value))
                {
                    SetEquippedSlot(skins.EquippedSkins, change.Key, null);
                    continue;
                }

                if (!ownedIds.Contains(change.Value))
                {
                    throw new InvalidOperationException($"Skin '{change.Value}' is not owned for slot '{change.Key}'.");
                }

                SetEquippedSlot(skins.EquippedSkins, change.Key, change.Value);
            }

            return await _repo.Upsert(skins);
        }

        private void SetEquippedSlot(EquippedSkins equipped, string slotName, string skinId)
        {
            switch (slotName)
            {
                case "Head":   equipped.Head   = skinId; break;
                case "Chest":  equipped.Chest  = skinId; break;
                case "Belt":   equipped.Belt   = skinId; break;
                case "Staff":  equipped.Staff  = skinId; break;
                case "Sword":  equipped.Sword  = skinId; break;
                case "Bow":    equipped.Bow    = skinId; break;
                case "Legs":   equipped.Legs   = skinId; break;
                case "Boots":  equipped.Boots  = skinId; break;
                case "Gloves": equipped.Gloves = skinId; break;
            }
        }
    }
}
