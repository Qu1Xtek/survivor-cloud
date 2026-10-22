using CloudLogic.RepoModels.Player;
using CloudLogic.RepoModels.PlayerRewards;
using CloudLogic.RepoModels.Shop;
using CloudLogic.Response.DTO;
using CloudLogic.Services.Shop;
using FluentAssertions;
using Google.Cloud.Firestore;

namespace CloudTests.Services
{
    [TestClass]
    public sealed class ShopOffersTests
    {
        private static IapProduct Offer(string id, RewardType type, bool oneTime = false, int minStage = 0)
        {
            return new IapProduct { Id = id, OneTime = oneTime, MinStage = minStage, Rewards = new List<Reward> { new Reward { RewardType = type, Value = 1 } } };
        }

        [TestMethod]
        public void OneTimeOffers_HideOnceBought()
        {
            var starter = Offer("starter_pack", RewardType.BlueGems, oneTime: true);
            var player = new PlayerData();

            ShopOffers.IsVisible(starter, player, new HashSet<string>()).Should().BeTrue();
            ShopOffers.IsVisible(starter, player, new HashSet<string> { "starter_pack" }).Should().BeFalse();
        }

        [TestMethod]
        public void StageOffers_ShowFromTheirStage()
        {
            var wall = Offer("wall_pack_10", RewardType.Gold, minStage: 10);

            ShopOffers.IsVisible(wall, new PlayerData { MaxLevelReached = 9 }, new HashSet<string>()).Should().BeFalse();
            ShopOffers.IsVisible(wall, new PlayerData { MaxLevelReached = 10 }, new HashSet<string>()).Should().BeTrue();
        }

        [TestMethod]
        public void Premium_HidesOnceOwned()
        {
            var premium = Offer("forge_premium", RewardType.Premium);

            ShopOffers.IsVisible(premium, new PlayerData { Premium = true }, new HashSet<string>()).Should().BeFalse();
        }

        [TestMethod]
        public void StashSlots_HideAtTheMax()
        {
            var stash = Offer("stash_slots_10", RewardType.StashSlots);

            ShopOffers.IsVisible(stash, new PlayerData { ExtraStashSlots = 30 }, new HashSet<string>()).Should().BeTrue();
            ShopOffers.IsVisible(stash, new PlayerData { ExtraStashSlots = 40 }, new HashSet<string>()).Should().BeFalse();
        }

        [TestMethod]
        public void ActiveCard_ShowsItsDailyClaim()
        {
            var now = DateTime.UtcNow;
            var player = new PlayerData
            {
                ForgeCardUntil = Timestamp.FromDateTime(now.AddDays(10)),
                ForgeCardClaimedAt = Timestamp.FromDateTime(now),
            };

            var item = ShopOffers.ToItem(Offer("forge_card", RewardType.ForgeCard), player, now);

            item.Type.Should().Be(ShopItemType.Offer);
            item.ActiveUntil.Should().BeGreaterThan(0);
            item.BoughtToday.Should().Be(1);
            ShopOffers.ToItem(Offer("forge_card", RewardType.ForgeCard), new PlayerData(), now).ActiveUntil.Should().Be(0);
        }
    }
}
