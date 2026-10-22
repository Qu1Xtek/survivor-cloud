using CloudLogic.RepoModels.Achievements;
using CloudLogic.RepoModels.Items;
using CloudLogic.RepoModels.Player;
using CloudLogic.Repositories;
using CloudLogic.Response;
using CloudLogic.Services.Achievements;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.CloudCode.Core;

namespace CloudLogic.Services.Items
{
    public class BlacksmithService
    {
        private IPlayerManagerService _playerManagerService;
        private IItemTemplateService _itemTemplateService;
        private ItemsRepo _itemsRepo;
        private PlayerDataRepo _playerDataRepo;
        private AchievementsService _achievementsService;
        private IItemGenerationService _itemGenerationService;

        public BlacksmithService(IPlayerManagerService playerManagerService,
            IItemTemplateService itemTemplateService,
            ItemsRepo itemsRepo,
            IItemGenerationService generationService,
            PlayerDataRepo repo,
            AchievementsService achievementsService)
        {
            _playerManagerService = playerManagerService;
            _itemTemplateService = itemTemplateService;
            _itemsRepo = itemsRepo;
            _itemGenerationService = generationService;
            _playerDataRepo = repo;
            _achievementsService = achievementsService;
        }

        public async Task<SalvageItemResponse> SalvageWeapon(string weaponId, IExecutionContext ctx)
        {
            var result = new SalvageItemResponse()
            {
                Success = false,
                CraftingMaterialsAdded = 0
            };

            var playerData = await _playerManagerService.GetPlayerDataAsync(ctx.PlayerId);

            await _itemsRepo.RunOurTransactionAsync(async tran => 
            {
                var rewardAmount = await GetSalvageReward(weaponId, ctx);

                if (rewardAmount > 0)
                {
                    result.CraftingMaterialsAdded = rewardAmount;

                    await _itemsRepo.DeleteItemById(weaponId, tran);
                    var newPlayerData = new PlayerData()
                    {
                        Id = playerData.Id,
                        CraftingMaterials = playerData.CraftingMaterials + rewardAmount
                    };

                    await _playerDataRepo.UpsertPlayerData(newPlayerData, tran);

                    var key = AchievementType.Salvage.ToString();
                    var progress = await _achievementsService.Increment(playerData.Id, key, 1);

                    result.Success = true;
                    result.AchievementProgressInfo = new Dictionary<string, long> { [key] = progress.AchievementProgress.Stats[key] };
                    result.QuestsProgressInfo = new Dictionary<string, long> { [key] = progress.QuestsProgress.Stats[key] };

                    if (rewardAmount == 5)
                    {
                        var legendaryKey = AchievementType.SalvageLegendary.ToString();

                        var legendaryProgress = await _achievementsService.Increment(playerData.Id, legendaryKey, 1);

                        result.AchievementProgressInfo.Add(legendaryKey, legendaryProgress.AchievementProgress.Stats[legendaryKey]);
                        result.QuestsProgressInfo.Add(legendaryKey, legendaryProgress.QuestsProgress.Stats[legendaryKey]);

                    }
                }

            });


            return result;
        }

        public async Task<SalvageItemResponse> SalvageWeaponMass(List<string> weaponIds, IExecutionContext ctx)
        {
            var result = new SalvageItemResponse()
            {
                Success = false,
                CraftingMaterialsAdded = 0
            };

            var weapons = (await _itemsRepo.GetMyOwnItems(ctx.PlayerId, weaponIds))
                .Where(w => w.Type == ItemType.Weapon)
                .ToList();

            if (weapons.Count == 0)
            {
                return result;
            }


            var totalReward = weapons.Sum(w => (int)w.Rarity + 1);

            var legendaryCount = weapons.Count(w => w.Rarity == RarityType.Legendary);

            PlayerData playerData = null;

            await _itemsRepo.RunOurTransactionAsync(async tran =>
            {
                playerData = await _playerManagerService.GetPlayerDataAsync(ctx.PlayerId);
                await _itemsRepo.DeleteItemsByIds(weapons.Select(w => w.Id).ToList(), tran);

                var newPlayerData = new PlayerData()
                {
                    Id = playerData.Id,
                    CraftingMaterials = playerData.CraftingMaterials + totalReward
                };
                newPlayerData.ExplicitValuesUpsert(new[] { nameof(PlayerData.CraftingMaterials) });

                await _playerDataRepo.UpsertPlayerData(newPlayerData, tran);
            });

            result.CraftingMaterialsAdded = totalReward;
            result.Success = true;

            var key = AchievementType.Salvage.ToString();
            var progress = await _achievementsService.Increment(playerData.Id, key, weapons.Count);

            result.AchievementProgressInfo = new Dictionary<string, long> { [key] = progress.AchievementProgress.Stats[key] };
            result.QuestsProgressInfo = new Dictionary<string, long> { [key] = progress.QuestsProgress.Stats[key] };

            if (legendaryCount > 0)
            {
                var legendaryKey = AchievementType.SalvageLegendary.ToString();
                var legendaryProgress = await _achievementsService.Increment(playerData.Id, legendaryKey, legendaryCount);

                result.AchievementProgressInfo[legendaryKey] = legendaryProgress.AchievementProgress.Stats[legendaryKey];
                result.QuestsProgressInfo[legendaryKey] = legendaryProgress.QuestsProgress.Stats[legendaryKey];
            }

            return result;
        }

        private async Task<int> GetSalvageReward(string weaponId, IExecutionContext ctx)
        {
            Item weapon = await _itemsRepo.GetPlayerItem(ctx.PlayerId, weaponId);

            if (weapon == null)
            {
                return 0;
            }

            var craftingReward = weapon.Rarity + 1;

            return (int)craftingReward;
        }
    }
}
