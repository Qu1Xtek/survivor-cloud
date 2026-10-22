using System.Collections.Generic;
using System.Threading.Tasks;
using CloudLogic.Modules;
using CloudLogic.RepoModels;
using CloudLogic.RepoModels.Items;
using CloudLogic.Services.Achievements;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CloudTests.Integration
{
    /// <summary>
    /// Integration tests for GearModule.SetNewGearscore.
    /// Runs the full cloud function path (player lookup, gear score calc, achievement update).
    /// </summary>
    [TestClass]
    public class GearModuleIntegrationTests : IntegrationTestBase
    {
        private GearModule _gearModule;

        [TestInitialize]
        public void Setup()
        {
            _gearModule = new GearModule(NullLogger<GearModule>.Instance);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task SetNewGearscore_ReturnsSuccess()
        {
            var entry = new GearScoreEntry
            {
                // Empty inventory => score resolves to 0; still exercises the full path.
                Items = new Dictionary<ItemType, Item>(),
            };

            var result = await _gearModule.SetNewGearscore(
                new Ctx { PlayerId = GlobalTestPlayerId },
                entry);

            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Gearscore.Should().BeGreaterThanOrEqualTo(0);
            entry.PlayerId.Should().Be(GlobalTestPlayerId, "the function stamps the caller's player id");
        }
    }
}
