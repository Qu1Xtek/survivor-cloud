using CloudLogic.RepoModels.Player;
using CloudLogic.RepoModels.PlayerRewards;
using System;
using CloudLogic.RepoModels.Shop;
using CloudLogic.Repositories;
using Google.Cloud.Firestore;
using CloudLogic.Response;
using CloudLogic.Services.Reward;
using CloudLogic.Services.Interfaces;
using RewardModel = CloudLogic.RepoModels.PlayerRewards.Reward;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudLogic.Response.DTO;
using CloudLogic.Util;

namespace CloudLogic.Services.Shop
{
    public class ShopService
    {
        private readonly ShopRepo _shopRepo;
        private readonly IapProductRepo _iapProductRepo;
        private readonly PlayerDataRepo _playerDataRepo;
        private readonly IRewardService _rewardService;
        private readonly DailyDealsService _deals;
        private readonly PurchaseRecordRepo _purchaseRecordRepo;
        private readonly GemBoughtProductRepo _gemBoughtProductRepo;
        private readonly SkinsRepo _skinsRepo;
        private readonly IPlayerSkinService _playerSkinService;
        private readonly SkinConfigsRepo _skinConfigsRepo;

        public ShopService(ShopRepo shopRepo, IapProductRepo iapProductRepo, PlayerDataRepo playerDataRepo,
            IRewardService rewardService, DailyDealsService deals, PurchaseRecordRepo purchaseRecordRepo,
            GemBoughtProductRepo gemBoughtProductRepo, SkinsRepo skinsRepo, IPlayerSkinService playerSkinService,
            SkinConfigsRepo skinConfigsRepo)
        {
            _gemBoughtProductRepo = gemBoughtProductRepo;
            _purchaseRecordRepo = purchaseRecordRepo;
            _shopRepo = shopRepo;
            _iapProductRepo = iapProductRepo;
            _playerDataRepo = playerDataRepo;
            _rewardService = rewardService;
            _deals = deals;
            _skinsRepo = skinsRepo;
            _playerSkinService = playerSkinService;
            _skinConfigsRepo = skinConfigsRepo;
        }

        public async Task<List<ShopItemDTO>> GetShop(string playerId)
        {
            var chests = _shopRepo.GetAll();
            var packs = _iapProductRepo.GetAll();
            var player = _playerDataRepo.GetPlayerDataByPlayerId(playerId);
            var purchases = _purchaseRecordRepo.GetByPlayer(playerId);
            await Task.WhenAll(chests, packs, player, purchases);
            var deals = await _deals.Today(chests.Result);
            var bought = purchases.Result.Where(p => !p.Refunded).Select(p => p.ProductId).ToHashSet();
            var result =  ShopItems.From(chests.Result, packs.Result, player.Result, deals, bought);

            List<string> ourAccs = new List<string>() { "HgxZ5B0DqBnXvnJFDHMCHLIxEPSy", "6MuQryZL2MdMM9rkPz2UOLmAvW2S", "c6Z2SkyzwGEddzXkqbNUjJKAKOGn" };

            if (!ourAccs.Contains(playerId))
            {
                result = result.Where(x => x.Type != ShopItemType.Skin).ToList();
            }

            return result;
        }

        public async Task<ShopResult> ClaimForgeCard(string playerId)
        {
            var now = DateTime.UtcNow;
            string refusal = null;

            await _playerDataRepo.RunOurTransactionAsync(async tran =>
            {
                var player = await _playerDataRepo.GetPlayerDataByPlayerId(playerId, tran);

                if (player == null || !player.HasForgeCard(now))
                {
                    refusal = "no_card";
                    return;
                }

                if (player.ClaimedForgeCardOn(now))
                {
                    refusal = "claimed";
                    return;
                }

                var update = new PlayerData { Id = player.Id, ForgeCardClaimedAt = Timestamp.FromDateTime(now) };
                await _playerDataRepo.UpsertPlayerData(update, player, tran);
            });

            if (refusal != null)
            {
                return new ShopResult { Success = false, AdditionalInfo = refusal };
            }

            var current = await _playerDataRepo.GetPlayerDataByPlayerId(playerId);
            var gems = await _rewardService.ClaimReward(current, new RewardModel { RewardType = RewardType.BlueGems, Value = GameConstants.ForgeCardDailyGems });
            var potions = await _rewardService.ClaimReward(current, new RewardModel { RewardType = RewardType.CraftingMaterial, Value = GameConstants.ForgeCardDailyPotions });

            return new ShopResult
            {
                Success = true,
                Type = RewardType.BlueGems,
                RewardVal = gems.RewardVal,
                BlueGems = (int)gems.RewardVal.Value,
                GemsGranted = gems.GemsGranted,
                CraftingMatsGranted = potions.CraftingMatsGranted,
            };
        }

        public async Task<ShopResult> BuyShopItem(string playerId, string shopId)
        {
            var shopResult = new ShopResult();

            if (string.IsNullOrWhiteSpace(playerId) || string.IsNullOrWhiteSpace(shopId))
            {
                return new ShopResult { Success = false };
            }

            ShopResult? failure = null;
            RewardModel? rewardToGrant = null;
            DailyDeal? deal = null;

            if (shopId.StartsWith(DailyDealsService.Prefix))
            {
                deal = (await _deals.Today()).FirstOrDefault(d => DailyDealsService.Prefix + d.ShopId == shopId);

                if (deal == null)
                {
                    return new ShopResult { Success = false, AdditionalInfo = "not_a_deal" };
                }
            }

            await _playerDataRepo.RunOurTransactionAsync(async tran =>
            {
                var shop = await _shopRepo.GetById(deal?.ShopId ?? shopId, tran);
                if (shop == null || shop.Cost <= 0 || shop.Cost > int.MaxValue)
                {
                    failure = new ShopResult { Success = false };
                    return;
                }

                var cost = (int)(deal != null ? DailyDealsService.Price(shop.Cost, deal.Discount) : shop.Cost);
                var limit = deal != null ? 1 : shop.DailyLimit;

                var player = await _playerDataRepo.GetPlayerDataByPlayerId(playerId, tran);

                if (player == null)
                {
                    failure = new ShopResult { Success = false };
                    return;
                }

                var refusal = Refusal(shop, player, cost, shopId, limit);

                if (refusal != null)
                {
                    failure = new ShopResult { Success = false, AdditionalInfo = refusal };
                    return;
                }

                var update = new PlayerData
                {
                    Id = playerId,
                    BlueGems = player.BlueGems - cost
                };

                // Ensure BlueGems is always written, including when it becomes 0.
                update.ExplicitUpserts = new List<string> { nameof(PlayerData.BlueGems) };

                if (limit > 0)
                {
                    RecordPurchase(update, player, shopId);
                }

                await _playerDataRepo.UpsertPlayerData(update, player, tran);
                await _gemBoughtProductRepo.Add(playerId, shopId, cost, update.BlueGems, tran);

                rewardToGrant = shop.Reward;
            });

            if (failure != null)
            {
                return failure;
            }

            if (rewardToGrant == null)
            {
                return new ShopResult { Success = false };
            }

            var playerAfterPurchase = await _playerDataRepo.GetPlayerDataByPlayerId(playerId);

            if (playerAfterPurchase == null)
            {
                return new ShopResult { Success = false };
            }

            var reward = await _rewardService.ClaimReward(playerAfterPurchase, rewardToGrant);

            shopResult = new ShopResult
            {
                BlueGems = playerAfterPurchase.BlueGems,
                Success = reward.Success,
                RewardObj = reward.RewardObj,
                RewardVal = reward.RewardVal,
                Type = reward.Type,
                GoldGranted = reward.GoldGranted,
                GemsGranted = reward.GemsGranted,
                EnergyGranted = reward.EnergyGranted,
                CraftingMatsGranted = reward.CraftingMatsGranted,
            };

            return shopResult;
        }

        public async Task<ShopResult> BuySkinItem(string playerId, string skinId)
        {
            var owned = await _skinsRepo.GetByPlayerId(playerId);

            if (owned.Any(s => s.SkinId == skinId))
            {
                return new ShopResult { Success = false, AdditionalInfo = "already_owned" };
            }

            ShopResult failure = null;
            RewardModel rewardToGrant = null;

            await _playerDataRepo.RunOurTransactionAsync(async tran =>
            {
                var shop = await _shopRepo.GetById(skinId, tran);

                if (shop == null || shop.Cost <= 0 || shop.Reward?.RewardType != RewardType.Skin)
                {
                    failure = new ShopResult { Success = false };
                    return;
                }

                var cost = (int)shop.Cost;
                var player = await _playerDataRepo.GetPlayerDataByPlayerId(playerId, tran);

                if (player == null)
                {
                    failure = new ShopResult { Success = false };
                    return;
                }

                if (player.BlueGems < cost)
                {
                    failure = new ShopResult { Success = false, AdditionalInfo = "not_enough_gems" };
                    return;
                }

                var update = new PlayerData
                {
                    Id = playerId,
                    BlueGems = player.BlueGems - cost,
                    ExplicitUpserts = new List<string> { nameof(PlayerData.BlueGems) }
                };

                await _playerDataRepo.UpsertPlayerData(update, player, tran);
                await _gemBoughtProductRepo.Add(playerId, skinId, cost, update.BlueGems, tran);
                rewardToGrant = shop.Reward;
            });

            if (failure != null)
            {
                return failure;
            }

            var configs = await _skinConfigsRepo.GetSkinConfigs();
            var config = configs.FirstOrDefault(c => c.SkinId == skinId);

            var skin = await _playerSkinService.CreateSkinPurchase(
                playerId, config?.SkinId, config?.Name, config.ItemType, "shop");
            var playerAfter = await _playerDataRepo.GetPlayerDataByPlayerId(playerId);

            return new ShopResult
            {
                Success = true,
                BlueGems = playerAfter.BlueGems,
                Type = RewardType.Skin,
                RewardObj = config.Id,
            };
        }

        private static string Refusal(RepoModels.Shop.Shop shop, PlayerData player, int cost, string key, int limit)
        {
            if (player.BlueGems < cost)
            {
                return "not_enough_gems";
            }

            if (limit > 0 && player.BoughtToday(key, DateTime.UtcNow) >= limit)
            {
                return "daily_limit";
            }

            if (shop.Reward?.RewardType == RewardType.StashSlots && player.ExtraStashSlots >= GameConstants.MaxExtraStashSlots)
            {
                return "stash_max";
            }

            return null;
        }

        private static void RecordPurchase(PlayerData update, PlayerData player, string shopId)
        {
            var now = DateTime.UtcNow;
            var today = player.BoughtOn(now)
                ? new Dictionary<string, int>(player.ShopBuysToday)
                : new Dictionary<string, int>();

            today[shopId] = player.BoughtToday(shopId, now) + 1;
            update.LastShopBuyAt = Timestamp.FromDateTime(now);
            update.ShopBuysToday = today;
        }
    }
}
