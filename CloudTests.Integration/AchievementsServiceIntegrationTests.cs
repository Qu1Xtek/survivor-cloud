using System.Threading.Tasks;
using CloudLogic.RepoModels.Achievements;
using CloudLogic.RepoModels.Items;
using CloudLogic.Services.Achievements;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CloudTests.Integration
{
    /// <summary>
    /// Integration tests for AchievementsService.Increment.
    /// Verifies it bumps both the achievement and the daily quest counters.
    /// </summary>
    [TestClass]
    public class AchievementsServiceIntegrationTests : IntegrationTestBase
    {
        private AchievementsService _service;
        private string _playerId;

        [TestInitialize]
        public void Setup()
        {
            _service = ServiceProvider.GetRequiredService<AchievementsService>();
            _playerId = GlobalTestPlayerId;
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task Increment_BumpsAchievementAndQuestByDelta()
        {
            var statKey = AchievementType.KillCount.ToString();

            // First increment establishes a baseline (handles the daily-reset case).
            var first = await _service.Increment(_playerId, statKey, 1);

            first.Should().NotBeNull();
            first.AchievementProgress.Should().NotBeNull();
            first.QuestsProgress.Should().NotBeNull();

            var achBaseline = first.AchievementProgress.Stats[statKey];
            var questBaseline = first.QuestsProgress.Stats[statKey];

            // Second increment must move both counters by exactly the delta.
            var second = await _service.Increment(_playerId, statKey, 2);

            second.AchievementProgress.Stats[statKey].Should().Be(achBaseline + 2);
            second.QuestsProgress.Stats[statKey].Should().Be(questBaseline + 2);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task IncrementMultiple_BumpsAchievementsAndQuestsByDelta()
        {
            var keyA = AchievementType.KillCount.ToString();
            var keyB = AchievementType.GoldCollected.ToString();

            // Baseline pass (handles daily reset on the quest side).
            var first = await _service.IncrementMultiple(_playerId,
                new Dictionary<string, long> { { keyA, 1 }, { keyB, 1 } });

            first.Should().NotBeNull();
            first.AchievementProgressD.Should().NotBeNull();
            first.QuestsProgressD.Should().NotBeNull();

            var achBaseA = first.AchievementProgressD[keyA];
            var achBaseB = first.AchievementProgressD[keyB];
            var questBaseA = first.QuestsProgressD[keyA];
            var questBaseB = first.QuestsProgressD[keyB];

            // Second pass must move every counter by exactly its delta.
            var second = await _service.IncrementMultiple(_playerId,
                new Dictionary<string, long> { { keyA, 2 }, { keyB, 3 } });

            second.AchievementProgressD[keyA].Should().Be(achBaseA + 2);
            second.AchievementProgressD[keyB].Should().Be(achBaseB + 3);
            second.QuestsProgressD[keyA].Should().Be(questBaseA + 2);
            second.QuestsProgressD[keyB].Should().Be(questBaseB + 3);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task IncrementCraft_BumpsAchievementAndQuestByOne()
        {
            var rarity = RarityType.Epic;
            var statKey = AchievementType.EpicCrafted.ToString();

            // Baseline pass (handles daily reset on the quest side).
            var first = await _service.IncrementCraft(_playerId, rarity);

            first.Should().NotBeNull();
            var achBaseline = first.AchievementProgressD[statKey];
            var questBaseline = first.QuestsProgressD[statKey];

            // Each craft bumps both counters by exactly 1.
            var second = await _service.IncrementCraft(_playerId, rarity);

            second.AchievementProgressD[statKey].Should().Be(achBaseline + 1);
            second.QuestsProgressD[statKey].Should().Be(questBaseline + 1);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task IncrementDrop_BumpsRarityAndItemsDroppedByOne()
        {
            var rarity = RarityType.Legendary;
            var rarityKey = AchievementType.LegendariesDropped.ToString();
            var totalKey = AchievementType.ItemsDropped.ToString();

            // Baseline pass (handles daily reset on the quest side).
            var first = await _service.IncrementDrop(_playerId, rarity);

            first.Should().NotBeNull();
            var achBaseRarity = first.AchievementProgressD[rarityKey];
            var achBaseTotal = first.AchievementProgressD[totalKey];
            var questBaseRarity = first.QuestsProgressD[rarityKey];
            var questBaseTotal = first.QuestsProgressD[totalKey];

            // Each drop bumps both the rarity counter and the total by 1, on both sides.
            var second = await _service.IncrementDrop(_playerId, rarity);

            second.AchievementProgressD[rarityKey].Should().Be(achBaseRarity + 1);
            second.AchievementProgressD[totalKey].Should().Be(achBaseTotal + 1);
            second.QuestsProgressD[rarityKey].Should().Be(questBaseRarity + 1);
            second.QuestsProgressD[totalKey].Should().Be(questBaseTotal + 1);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task Set_OverwritesAchievementAndQuestToValue()
        {
            var statKey = AchievementType.ReachMap.ToString();

            // Set to one value, then a different one: proves it overwrites, not increments.
            await _service.Set(_playerId, statKey, 5);
            var result = await _service.Set(_playerId, statKey, 8);

            result.Should().NotBeNull();
            result.AchievementProgress.Stats[statKey].Should().Be(8);
            result.QuestsProgress.Stats[statKey].Should().Be(8);
        }
    }
}
