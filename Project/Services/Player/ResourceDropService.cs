using CloudLogic.RepoModels.Achievements;
using CloudLogic.RepoModels.Items;
using CloudLogic.RepoModels.Player;
using CloudLogic.Repositories;
using CloudLogic.Services.Items;
using CloudLogic.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.CloudCode.Core;

namespace CloudLogic.Services.Player
{
    public class ResourceDropService
    {
        private ResourceUtil _resourceUtil;
        private IPlayerManagerService _playerManagerService;
        private AchievementProgressRepo _achievementRepo;
        private PlayerDataRepo _repo;

        public ResourceDropService(IPlayerManagerService playerManagerService,
            PlayerDataRepo repo,
            AchievementProgressRepo achievmentRepo)
        {
            _resourceUtil = new ResourceUtil();
            _playerManagerService = playerManagerService;
            _repo = repo;
            _achievementRepo = achievmentRepo;
        }

        public async Task<PlayerData> PickupGoldAndCraftMats(string playerId, List<long> bagIds, int craftPickedCount)
        {
            PlayerData result = null;

            await _repo.RunOurTransactionAsync(async tran =>
            {
                var playerData = await _playerManagerService.GetPlayerDataAsync(playerId, null, tran);

                // Rewards for this run were already granted (normal or ad-double).
                if (playerData.RunData.RunClaimed)
                {
                    result = playerData;
                    return;
                }

                for (int i = 0; i < bagIds.Count; i++)
                {
                    var gold = bagIds[i];

                    if (!playerData.RunData.GoldDropped.Remove(gold))
                    {
                        continue;
                    }

                    playerData.RunData.GoldPicked.Add(gold);
                }

                // Crafting is tracked client-side; the only server rule is picked <= 2 per run.
                var grantedCraft = Math.Min(craftPickedCount, 2);

                playerData.RunData.CraftingMaterialsPicked = grantedCraft;
                playerData.RunData.RunClaimed = true;

                var newPlrData = new PlayerData()
                {
                    Id = playerData.Id,
                    Gold = playerData.Gold + playerData.RunData.GoldPicked.Sum(),
                    CraftingMaterials = playerData.CraftingMaterials + grantedCraft,
                    RunData = playerData.RunData
                };

                result = await _playerManagerService.UpsertPlayerData(newPlrData, playerData, tran);
            });

            return result;
        }

        public async Task<long> DropGold(string playerId)
        {
            long goldResult = -1;

            await _repo.RunOurTransactionAsync(async tran =>
            {
                var playerData = await _playerManagerService.GetPlayerDataAsync(playerId, null, tran);
                
                var enemyLevel = playerData.SelectedLevel + 1;
                var currentDrops = playerData.RunData.GoldDropped.Count + playerData.RunData.GoldPicked.Count;
                long goldAmount = new Random().NextInt64(enemyLevel, (long)(1.5 * enemyLevel) + 1);

                if (currentDrops >= playerData.RunData.MaxGoldDrops)
                {
                    return;
                }

                var newPlrData = new PlayerData() { Id = playerData.Id, RunData = playerData.RunData };

                newPlrData.RunData.GoldDropped.Add(goldAmount);
                goldResult = goldAmount;

                await _playerManagerService.UpsertPlayerData(newPlrData, playerData, tran);
            });

            return goldResult;
        }

        public async Task<long> DropCraftingMaterial(string playerId)
        {
            long result = -1;

            // Transactional so a concurrent pickup/complete can't overwrite this increment.
            await _repo.RunOurTransactionAsync(async tran =>
            {
                var playerData = await _playerManagerService.GetPlayerDataAsync(playerId, null, tran);

                var allCrafts = playerData.RunData.CraftingMaterialsDropped + playerData.RunData.CraftingMaterialsPicked;

                if (allCrafts > 1)
                {
                    result = -1;
                    return;
                }

                var newPlrData = new PlayerData() { Id = playerData.Id, RunData = playerData.RunData };

                newPlrData.RunData.CraftingMaterialsDropped++;
                result = newPlrData.RunData.CraftingMaterialsDropped;

                await _playerManagerService.UpsertPlayerData(newPlrData, playerData, tran);
            });

            return result;
        }
    }
}
