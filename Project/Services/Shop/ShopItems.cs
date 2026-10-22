using CloudLogic.RepoModels.Player;
using CloudLogic.RepoModels.PlayerRewards;
using CloudLogic.RepoModels.Shop;
using CloudLogic.Response.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using ShopModel = CloudLogic.RepoModels.Shop.Shop;
using RewardModel = CloudLogic.RepoModels.PlayerRewards.Reward;

namespace CloudLogic.Services.Shop
{
    public static class ShopItems
    {
        public static List<ShopItemDTO> From(List<ShopModel> chests, List<IapProduct> packs, PlayerData player = null,
            List<DailyDeal> deals = null, ICollection<string> bought = null)
        {
            var now = DateTime.UtcNow;
            chests ??= new();
            var items = chests.Select(c => FromChest(c, player?.BoughtToday(c.Id, now) ?? 0));
            var dealItems = (deals ?? new())
                .Select(d => (Deal: d, Shop: chests.FirstOrDefault(c => c.Id == d.ShopId)))
                .Where(d => d.Shop != null)
                .Select(d => FromDeal(d.Shop, d.Deal.Discount, player?.BoughtToday(DailyDealsService.Prefix + d.Shop.Id, now) ?? 0));
            packs ??= new();
            var offers = packs
                .Where(p => ShopOffers.IsOffer(p) && ShopOffers.IsVisible(p, player, bought ?? new HashSet<string>()))
                .Select(p => ShopOffers.ToItem(p, player, now));
            var gemPacks = packs.Where(p => !ShopOffers.IsOffer(p)).Select(FromPack);
            return items.Concat(dealItems).Concat(offers).Concat(gemPacks).ToList();
        }

        private static ShopItemDTO FromDeal(ShopModel shop, int discount, int boughtToday)
        {
            var item = FromChest(shop, boughtToday);
            item.Id = DailyDealsService.Prefix + shop.Id;
            item.Tag = null;
            item.FullCost = shop.Cost;
            item.Cost = DailyDealsService.Price(shop.Cost, discount);
            item.DailyLimit = 1;
            return item;
        }

        private static ShopItemDTO FromChest(ShopModel shop, int boughtToday)
        {
            return new ShopItemDTO
            {
                Id = shop.Id,
                Type = shop.Reward?.RewardType == RewardType.Skin ? ShopItemType.Skin : ShopItemType.Chest,
                Title = shop.Title,
                Description = shop.Description,
                Icon = shop.Id,
                Cost = shop.Cost,
                Tag = shop.Tag,
                Reward = shop.Reward,
                DailyLimit = shop.DailyLimit,
                BoughtToday = boughtToday,
            };
        }

        private static ShopItemDTO FromPack(IapProduct pack)
        {
            return new ShopItemDTO
            {
                Id = pack.Id,
                Type = ShopItemType.GemPack,
                Title = pack.Title,
                Description = pack.Description,
                Icon = pack.Icon,
                Tag = pack.Tag,
                Price = pack.Price,
                Reward = new RewardModel { RewardType = RewardType.BlueGems, Value = pack.Amount },
            };
        }
    }
}
