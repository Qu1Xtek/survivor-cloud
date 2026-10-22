using CloudLogic.RepoModels.Player;
using CloudLogic.Services.Player;
using FluentAssertions;

namespace CloudTests.Services
{
    [TestClass]
    public sealed class ReviveTests
    {
        [TestMethod]
        public void OwnedRevive_IsSpentBeforeGems()
        {
            var update = ReviveService.Spend(new PlayerData { Revives = 2, BlueGems = 500 });

            update.Revives.Should().Be(1);
            update.BlueGems.Should().Be(500);
            update.RunRevives.Should().Be(1);
        }

        [TestMethod]
        public void GemRevives_Cost50Then100()
        {
            ReviveService.Spend(new PlayerData { BlueGems = 500 }).BlueGems.Should().Be(450);
            ReviveService.Spend(new PlayerData { BlueGems = 500, RunRevives = 1 }).BlueGems.Should().Be(400);
        }

        [TestMethod]
        public void Refuses_PastTheLimitOrWithoutGems()
        {
            ReviveService.Refusal(new PlayerData { Revives = 5, RunRevives = 2 }).Should().Be("limit");
            ReviveService.Refusal(new PlayerData { BlueGems = 99, RunRevives = 1 }).Should().Be("not_enough_gems");
            ReviveService.Refusal(new PlayerData { BlueGems = 50 }).Should().BeNull();
        }
    }
}
