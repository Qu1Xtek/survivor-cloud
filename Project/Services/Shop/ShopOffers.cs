using CloudLogic.RepoModels.Player;
using CloudLogic.RepoModels.PlayerRewards;
using CloudLogic.RepoModels.Shop;
using CloudLogic.Response.DTO;
using CloudLogic.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CloudLogic.Services.Shop
{
    public static class ShopOffers
    {
        public static bool IsOffer(IapProduct pack)
        {
            return pack.Rewards?.Count > 0;
        }

        public static bool IsVisible(IapProduct pack, PlayerData player, ICollection<string> bought)
        {
            if (player == null)
            {
                return true;
            }

            if (pack.OneTime && bought.Contains(pack.Id))
            {
                return false;
            }

            if (pack.MinStage > player.MaxLevelReached)
            {
                return false;
            }

            if (player.ExtraStashSlots >= GameConstants.MaxExtraStashSlots && Grants(pack, RewardType.StashSlots))
            {
                return false;
            }

            return !(player.Premium && Grants(pack, RewardType.Premium));
        }

        public static ShopItemDTO ToItem(IapProduct pack, PlayerData player, DateTime now)
        {
            var item = new ShopItemDTO
            {
                Id = pack.Id,
                Type = ShopItemType.Offer,
                Title = pack.Title,
                Description = pack.Description,
                Icon = pack.Icon,
                Tag = pack.Tag,
                Price = pack.Price,
            };

            if (player != null && Grants(pack, RewardType.ForgeCard) && player.HasForgeCard(now))
            {
                item.ActiveUntil = new DateTimeOffset(player.ForgeCardUntil.ToDateTime()).ToUnixTimeSeconds();
                item.DailyLimit = 1;
                item.BoughtToday = player.ClaimedForgeCardOn(now) ? 1 : 0;
            }

            return item;
        }

        private static bool Grants(IapProduct pack, RewardType type)
        {
            return pack.Rewards?.Any(r => r.RewardType == type) == true;
        }
    }
}
