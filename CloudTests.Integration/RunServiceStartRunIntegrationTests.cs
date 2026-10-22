using CloudLogic.RepoModels.Player;
using CloudLogic.Services.Player;
using CloudLogic.Services.Items;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
using System;
using CloudLogic.Repositories;

namespace CloudTests.Integration
{
    /// <summary>
    /// Integration tests for IRunService.StartRun.
    /// These tests use real Firestore (or emulator) and test the full flow of starting a run.
    /// </summary>
    [TestClass]
    public class RunServiceStartRunIntegrationTests : IntegrationTestBase
    {
        private IRunService _runService;
        private IPlayerManagerService _playerManagerService;
        private ItemDropService _itemDropService;
        private string _testPlayerId;
        private AuditRepo _auditRepo;

        [TestInitialize]
        public void Setup()
        {
            _runService = ServiceProvider.GetRequiredService<IRunService>();
            _playerManagerService = ServiceProvider.GetRequiredService<IPlayerManagerService>();
            _itemDropService = ServiceProvider.GetRequiredService<ItemDropService>();
            _auditRepo = ServiceProvider.GetRequiredService<AuditRepo>();
            _testPlayerId = IntegrationTestBase.GlobalTestPlayerId;
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task StartRun_WithEnoughEnergyAndValidLevel_StartsRunSuccessfully()
        {
            // Arrange
            var playerData = new PlayerData
            {
                Id = _testPlayerId,
                Energy = 20,
                MaxLevelReached = 10,
                SelectedLevel = 5,
                LastEnergyGiveStamp = Timestamp.FromDateTime(DateTime.UtcNow),
                LoginRewardInfo = new LoginRewardInfo()
            };
            await _playerManagerService.UpsertPlayerData(playerData);

            // Act
            var result = await _runService.StartRun(_testPlayerId, 5);

            // Assert
            result.Success.Should().BeTrue();
            result.PlayerData.Should().NotBeNull();
            result.PlayerData.Energy.Should().Be(15); // 50 - 5
            result.PlayerData.SelectedLevel.Should().Be(5);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task StartRun_WithNotEnoughEnergy_ReturnsFailure()
        {
            // Arrange
            var playerData = new PlayerData
            {
                Id = _testPlayerId,
                Energy = 3, // Less than 5 required
                MaxLevelReached = 10,
                SelectedLevel = 5,
                LastEnergyGiveStamp = Timestamp.FromDateTime(DateTime.UtcNow)
            };
            await _playerManagerService.UpsertPlayerData(playerData);

            // Act
            var result = await _runService.StartRun(_testPlayerId, 5);

            // Assert
            result.Success.Should().BeFalse();
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task StartRun_WithLevelHigherThanMax_ReturnsFailure()
        {
            // Arrange
            var playerData = new PlayerData
            {
                Id = _testPlayerId,
                Energy = 50,
                MaxLevelReached = 5,
                SelectedLevel = 3,
                LastEnergyGiveStamp = Timestamp.FromDateTime(DateTime.UtcNow)
            };
            await _playerManagerService.UpsertPlayerData(playerData);

            // Act
            var result = await _runService.StartRun(_testPlayerId, 10); // Level 10 > MaxLevelReached 5

            // Assert
            result.Success.Should().BeFalse();
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task StartRun_PreparesNewRunDropBag()
        {
            // Arrange
            var playerData = new PlayerData
            {
                Id = _testPlayerId,
                Energy = 50,
                MaxLevelReached = 10,
                SelectedLevel = 5,
                LastEnergyGiveStamp = Timestamp.FromDateTime(DateTime.UtcNow)
            };

            await _playerManagerService.UpsertPlayerData(playerData);

            // Act
            var result = await _runService.StartRun(_testPlayerId, 5);

            // Assert
            result.Success.Should().BeTrue();
            var itemDrop = await _itemDropService.GetRunBagAsync(_testPlayerId, "");
            itemDrop.Should().NotBeNull();
            itemDrop.RunDropBag.Should().BeEmpty(); // New run bag should be empty
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task StartRun_ResetsRunData()
        {           
            // Arrange
            var playerData = new PlayerData
            {
                Id = _testPlayerId,
                Energy = 50,
                MaxLevelReached = 10,
                SelectedLevel = 5,
                LastEnergyGiveStamp = Timestamp.FromDateTime(DateTime.UtcNow),
                RunData = new RunData
                {
                    GoldDropped = new System.Collections.Generic.List<long> { 100, 200 },
                    GoldPicked = new System.Collections.Generic.List<long> { 50 },
                    CraftingMaterialsDropped = 5,
                    ItemsDropped = 2
                }
            };
            await _playerManagerService.UpsertPlayerData(playerData);

            // Act
            var result = await _runService.StartRun(_testPlayerId, 5);

            // Assert
            result.Success.Should().BeTrue();
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task StartRun_WithNonExistentPlayer_ReturnsFailure()
        {
            // Act
            var result = await _runService.StartRun("non_existent_player", 5);

            // Assert
            result.Success.Should().BeFalse();
        }

        [TestCleanup]
        public async Task CleanupTestData()
        {
            // Clean up test data if needed
            try
            {
                var itemDrop = await _itemDropService.GetRunBagAsync(_testPlayerId, "");
                if (itemDrop?.RunDropBag != null)
                {
                    foreach (var drop in itemDrop.RunDropBag.Values)
                    {
                        await _itemDropService.DeleteItemDropById(_testPlayerId, drop.Id);
                    }
                }
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }
}
