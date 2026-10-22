using CloudLogic.RepoModels.Items;
using CloudLogic.RepoModels.Player;
using CloudLogic.RepoModels.PlayerRewards;
using CloudLogic.Repositories;
using CloudLogic.Response;
using CloudLogic.Response.DTO;
using CloudLogic.Services.Items;
using CloudLogic.Services.Player;
using CloudLogic.Util;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RewardModel = CloudLogic.RepoModels.PlayerRewards.Reward;

namespace CloudLogic.Services.Reward
{
    public class RewardService : IRewardService
    {
        private ClaimedRewardRepo _claimedRewardRepo;
        private readonly IItemGenerationService _itemGenerationService;
        private readonly ResourceService _resourceService;
        private readonly IItemCreateService _itemCreateService;
        private readonly IEnergyService _energyService;
        private readonly IItemTemplateService _itemTemplateService;
        private readonly ILogger _logger;

        public RewardService(ClaimedRewardRepo claimedRewardRepo,
            IItemGenerationService itemGenerationService,
            ResourceService resourceService,
            ILogger logger,
            IEnergyService energy,
            IItemCreateService itemCreateService,
            IItemTemplateService itemTemplateService)
        {
            _claimedRewardRepo = claimedRewardRepo;
            _itemGenerationService = itemGenerationService;
            _resourceService = resourceService;
            _logger = logger;
            _energyService = energy;
            _itemCreateService = itemCreateService;
            _itemTemplateService = itemTemplateService;
        }

        public async Task<RewardGrantDTO> ClaimReward(PlayerData playerData, RewardModel reward)
        {
            if (reward == null)
            {
                _logger.LogError("Attempted to claim a reward with null definition.");
                return null;
            }

            var rewardType = reward.RewardType;
            var value = reward.Value;

            var result = new RewardGrantDTO
            {
                Type = rewardType,
            };

            if (IsChest(rewardType) && (reward.LevelRange == null || reward.LevelRange.Length == 0 || reward.LevelRange[0] == 0))
            {
                reward = await AtHeroLevel(reward, playerData);
            }

            switch (rewardType)
            {
                case RewardType.Gold:
                    var newGold = await _resourceService.AddGold(playerData, value);
                    result.RewardVal = BindableLong.Of(newGold);
                    result.GoldGranted = value;
                    break;
                case RewardType.WeaponChest:
                    result.RewardObj = await ClaimWeaponChest(reward, playerData.Id);
                    break;
                case RewardType.ArmorChest:
                    result.RewardObj = await ClaimArmorChest(reward, playerData.Id);
                    break;
                case RewardType.DailyLoginChest:
                    var chestLevel = playerData.HeroLevel; // Set for current level
                    chestLevel = await _itemTemplateService.GetClosestLevelForRarityForDailyLogin((int)chestLevel, reward.Rarity);
                    reward.LevelRange = new long[] { chestLevel, chestLevel };
                    result.RewardObj = await ClaimArmorChest(reward, playerData.Id);
                    break;
                case RewardType.Chest:
                    result.RewardObj = await ClaimChest(reward, playerData.Id);
                    break;
                case RewardType.CraftingMaterial:
                    var craftedMats = await _resourceService.AddCraftingMaterials(playerData.Id, (int)value);
                    result.RewardVal = BindableLong.Of(craftedMats);
                    result.CraftingMatsGranted = value;
                    break;
                case RewardType.Energy:
                    var energy = await _energyService.AddEnergy(playerData.Id, (int)value, true);
                    result.RewardVal = BindableLong.Of(energy);
                    result.EnergyGranted = value;
                    break;
                case RewardType.BlueGems:
                    var gems = await _resourceService.AddBlueGems(playerData.Id, (int)value);
                    result.RewardVal = BindableLong.Of(gems);
                    result.GemsGranted = value;
                    break;
                case RewardType.Rune:
                    var runes = await _resourceService.AddRerollRunes(playerData.Id, (int)value);
                    result.RewardVal = BindableLong.Of(runes);
                    result.RewardObj = JsonConvert.SerializeObject(Enumerable.Range(0, (int)value)
                        .Select(_ => _itemGenerationService.CreateRerollRuneItem()).ToList());
                    break;
                case RewardType.StashSlots:
                    var stash = await _resourceService.AddStashSlots(playerData.Id, (int)value);
                    result.RewardVal = BindableLong.Of(stash);
                    break;
                case RewardType.ForgeCard:
                    var until = await _resourceService.AddForgeCardDays(playerData.Id, (int)value);
                    result.RewardVal = BindableLong.Of(new DateTimeOffset(until).ToUnixTimeSeconds());
                    break;
                case RewardType.Premium:
                    await _resourceService.SetPremium(playerData.Id);
                    result.RewardVal = BindableLong.Of(1);
                    break;
                case RewardType.Revive:
                    await _resourceService.AddRevives(playerData.Id, (int)value);
                    result.RewardVal = BindableLong.Of(value);
                    break;
                case RewardType.TalentResetToken:
                    await _resourceService.AddTalentResetToken(playerData.Id, (int)value);
                    break;
            }

            var claimedReward = await LogRewardClaim(playerData.PlayerId,
                playerData.HeroLevel,
                reward,
                "Na ji tupite idei");

            //result.RewardId = claimedReward.Id;

            result.Success = true;
            return result;
        }

        private static bool IsChest(RewardType type)
        {
            return type is RewardType.WeaponChest or RewardType.ArmorChest or RewardType.Chest;
        }

        private async Task<RewardModel> AtHeroLevel(RewardModel reward, PlayerData playerData)
        {
            var level = await _itemTemplateService.GetClosestLevelForRarityForDailyLogin((int)playerData.HeroLevel, reward.Rarity);

            return new RewardModel
            {
                RewardType = reward.RewardType,
                Value = reward.Value,
                Rarity = reward.Rarity,
                GuaranteedRarities = reward.GuaranteedRarities,
                LevelRange = new long[] { level, level },
                Slot = reward.Slot,
                ItemLevel = reward.ItemLevel,
            };
        }

        List<RarityType> GetRarities(RarityType maxRarity, 
            long count, 
            Dictionary<string, long>? guaranteedRarities)
        {
            List<RarityType> rarities = new List<RarityType>();

            if (guaranteedRarities != null)
            {
                foreach (var item in guaranteedRarities)
                {
                    for (int i = 0; i < item.Value; i++)
                    {
                        rarities.Add(Enum.Parse<RarityType>(item.Key));
                        count--;
                    }
                }
            }

            for (int i = 0; i < count; i++)
            {
                var rngRarity = ItemDropService.GetRarity();
                rarities.Add((RarityType)MathF.Min((int)rngRarity, (int)maxRarity));
            }

            return rarities;
        }

        private async Task<string> ClaimChest(RewardModel reward, string playerId, ItemType specificType = 0)
        {
            var rarities = GetRarities(reward.Rarity, reward.Value, reward.GuaranteedRarities);
            List<Item> items = new List<Item>();

            foreach (var rarity in rarities)
            {
                var item = await _itemGenerationService.CreateItemForLevel(
                    (int)reward.LevelRange[0],
                    (int)reward.LevelRange[1],
                    rarity,
                    specificType);

                item.OwnerId = playerId;

                if (reward.ItemLevel > 0 && specificType == ItemType.Weapon && item.Level <= GameConstants.MaxRelevelWeaponLevel)
                {
                    item.Level = reward.ItemLevel;
                }

                items.Add(item);
            }

            var added = await _itemCreateService.CreateItems(items);
            return JsonConvert.SerializeObject(added);
        }

        private async Task<string> ClaimArmorChest(RewardModel reward, string playerId)
        {
            return await ClaimChest(reward, playerId, reward.Slot != 0 ? reward.Slot : ItemType.Head); // GIVES ALL ARMOR SLOTS
        }

        private async Task<string> ClaimWeaponChest(RewardModel reward, string playerId)
        {
            return await ClaimChest(reward, playerId, ItemType.Weapon);
        }

        private async Task<ClaimedReward> LogRewardClaim(string playerId, long heroLevel,
            RewardModel reward, string? specificInfo)
        {
            var claimedReward = new ClaimedReward()
            {               
                ClaimedAtLevel = heroLevel,
                PlayerId = playerId,
                Value = reward,
                ClaimedAt = Timestamp.FromDateTime(DateTime.UtcNow),
            };

            return await _claimedRewardRepo.StoreClaimedReward(claimedReward);
        }
    }
}