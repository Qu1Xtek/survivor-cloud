using CloudLogic.RepoModels.Player;
using CloudLogic.RepoModels.PlayerRewards;
using CloudLogic.RepoModels.Shop;
using CloudLogic.Services.Shop;
using FluentAssertions;
using Google.Cloud.Firestore;
using ShopModel = CloudLogic.RepoModels.Shop.Shop;

namespace CloudTests.Services
{
    [TestClass]
    public sealed class DailyDealsTests
    {
        private static readonly DateTime Day = new(2026, 10, 5, 0, 0, 0, DateTimeKind.Utc);

        private static List<ShopModel> Pool()
        {
            return Enumerable.Range(1, 8)
                .Select(i => new ShopModel { Id = $"item_{i}", Cost = 100, Discount = i == 8 ? 0 : 10 * i, Reward = new Reward { RewardType = RewardType.Gold, Value = 1 } })
                .ToList();
        }

        [TestMethod]
        public void Pick_IsTheSameAllDayAndOnlyUsesDiscountedItems()
        {
            var morning = DailyDealsService.Pick(Pool(), Day.AddHours(1), 5);
            var evening = DailyDealsService.Pick(Pool(), Day.AddHours(23), 5);

            morning.Select(d => d.ShopId).Should().Equal(evening.Select(d => d.ShopId));
            morning.Should().HaveCount(5).And.OnlyContain(d => d.ShopId != "item_8" && d.Discount > 0);
        }

        [TestMethod]
        public void Pick_ChangesOverTheWeek()
        {
            var days = Enumerable.Range(0, 7)
                .Select(i => string.Join(",", DailyDealsService.Pick(Pool(), Day.AddDays(i), 5).Select(d => d.ShopId)))
                .Distinct();

            days.Should().HaveCountGreaterThan(1);
        }

        [TestMethod]
        public void Deals_ComeBackDiscountedOncePerDay()
        {
            var deal = new DailyDeal { ShopId = "item_3", Discount = 30 };
            var player = new PlayerData
            {
                LastShopBuyAt = Timestamp.FromDateTime(DateTime.UtcNow),
                ShopBuysToday = new Dictionary<string, int> { { "deal_item_3", 1 } },
            };

            var item = ShopItems.From(Pool(), null!, player, new List<DailyDeal> { deal }).Single(i => i.Id == "deal_item_3");

            item.Should().BeEquivalentTo(new { Cost = 70L, FullCost = 100L, DailyLimit = 1, BoughtToday = 1, Icon = "item_3" });
        }
    }
}
