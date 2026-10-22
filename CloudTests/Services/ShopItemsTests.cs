using CloudLogic.RepoModels.Player;
using CloudLogic.RepoModels.PlayerRewards;
using CloudLogic.RepoModels.Shop;
using CloudLogic.Response.DTO;
using CloudLogic.Services.Shop;
using FluentAssertions;
using Google.Cloud.Firestore;
using ShopModel = CloudLogic.RepoModels.Shop.Shop;

namespace CloudTests.Services
{
    [TestClass]
    public sealed class ShopItemsTests
    {
        [TestMethod]
        public void From_MapsBothCollectionsAsConfigured()
        {
            var chest = new ShopModel
            {
                Id = "10_weapon_legendary",
                Title = "Legendary Weapon",
                Description = "A level 10 legendary weapon.",
                Cost = 150,
                Reward = new Reward { RewardType = RewardType.WeaponChest, Value = 1 },
            };
            var pack = new IapProduct { Id = "blue_gems_130", Title = "130 Gems", Amount = 130, Price = "$0.99", Icon = "GemPack1", Tag = "Hot" };

            var items = ShopItems.From(new List<ShopModel> { chest }, new List<IapProduct> { pack });

            items.Select(i => i.Type).Should().Equal(ShopItemType.Chest, ShopItemType.GemPack);
            items[0].Should().BeEquivalentTo(new { chest.Id, chest.Title, chest.Description, Icon = chest.Id, chest.Cost, chest.Reward });
            items[1].Should().BeEquivalentTo(new { pack.Id, pack.Title, pack.Icon, pack.Tag, pack.Price, Cost = 0L });
            items[1].Reward.RewardType.Should().Be(RewardType.BlueGems);
            items[1].Reward.Value.Should().Be(130);
        }

        [TestMethod]
        public void From_CarriesTagLimitAndTodaysBuys()
        {
            var rune = new ShopModel { Id = "legendary_rune", Tag = "Hot", DailyLimit = 1, Cost = 200, Reward = new Reward { RewardType = RewardType.Rune, Value = 1 } };
            var player = new PlayerData
            {
                LastShopBuyAt = Timestamp.FromDateTime(DateTime.UtcNow),
                ShopBuysToday = new Dictionary<string, int> { { "legendary_rune", 1 } },
            };

            var item = ShopItems.From(new List<ShopModel> { rune }, null!, player).Single();

            item.Should().BeEquivalentTo(new { Tag = "Hot", DailyLimit = 1, BoughtToday = 1 });
        }

        [TestMethod]
        public void BoughtToday_ResetsOnANewDay()
        {
            var player = new PlayerData { LastShopBuyAt = Timestamp.FromDateTime(new DateTime(2026, 10, 4, 9, 30, 0, DateTimeKind.Utc)), ShopBuysToday = new Dictionary<string, int> { { "energy_20", 3 } } };

            player.BoughtToday("energy_20", new DateTime(2026, 10, 4, 23, 0, 0, DateTimeKind.Utc)).Should().Be(3);
            player.BoughtToday("energy_20", new DateTime(2026, 10, 5, 0, 1, 0, DateTimeKind.Utc)).Should().Be(0);
            new PlayerData().BoughtToday("energy_20", DateTime.UtcNow).Should().Be(0);
        }

        [TestMethod]
        public void StashLimit_IsTheDefaultPlusExtraSlots()
        {
            new PlayerData().StashLimit.Should().Be(40);
            new PlayerData { ExtraStashSlots = 20 }.StashLimit.Should().Be(60);
        }

        [TestMethod]
        public void From_HandlesMissingCollections()
        {
            ShopItems.From(null!, null!).Should().BeEmpty();
        }
    }
}
