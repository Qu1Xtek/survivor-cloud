using CloudLogic.RepoModels.Player;
using CloudLogic.Repositories;
using CloudLogic.Services.Items;
using CloudLogic.Services.Player;
using FluentAssertions;
using Google.Cloud.Firestore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.CloudCode.Core;

namespace CloudTests.Integration
{
    /// <summary>
    /// Integration tests for IPlayerManagerService.
    /// Uses real Firestore (or emulator).
    /// </summary>
    [TestClass]
    public class PlayerManagerServiceIntegrationTests : IntegrationTestBase
    {
        private IPlayerManagerService _playerManagerService;
        private PlayerDataRepo _playerDataRepo;
        private string _testPlayerId;

        /// <summary>
        /// Minimal stub for IExecutionContext used by LevelUp and UpdateDeviceId.
        /// </summary>
        private sealed class TestExecutionContext : IExecutionContext
        {
            public string PlayerId { get; set; }
            public string UserId { get; set; }
            public string ServiceToken => "";
            public string AccessToken => "";
            public string ProjectId => "";
            public string EnvironmentId => "";
            public string EnvironmentName => "";
            public string Issuer => "";
            public string AnalyticsUserId => "";
            public string UnityInstallationId => "";
            public string CorrelationId => "";
        }

        [TestInitialize]
        public void Setup()
        {
            _playerManagerService = ServiceProvider.GetRequiredService<IPlayerManagerService>();
            _playerDataRepo = ServiceProvider.GetRequiredService<PlayerDataRepo>();
            _testPlayerId = GlobalTestPlayerId;
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task GetPlayerDataAsync_WhenPlayerDoesNotExist_CreatesNewPlayerWithDefaults()
        {
            await _playerManagerService.RemovePLAYERDATAUSEDONLYFORTESTING(GlobalTestPlayerId);
            var result = await _playerManagerService.GetPlayerDataAsync(GlobalTestPlayerId, "TestUser");

            result.Should().NotBeNull();
            result.Id.Should().Be(_testPlayerId);
            result.PlayerId.Should().Be("TestUser");
            result.Crystals.Should().Be(3);
            result.NLCR.Should().Be(3);
            result.LearnedTalents.Should().NotBeNull().And.BeEmpty();
            result.RunData.Should().NotBeNull();
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task GetPlayerDataAsync_WhenPlayerExists_ReturnsExistingData()
        {
            var existing = new PlayerData
            {
                Id = _testPlayerId,
                PlayerId = _testPlayerId,
                Gold = 500,
                HeroLevel = 5,
                LastEnergyGiveStamp = Timestamp.FromDateTime(DateTime.UtcNow),
                NextBlueGemAdAt = Timestamp.FromDateTime(DateTime.UtcNow),
                LastRunCompletedAt = Timestamp.FromDateTime(DateTime.UtcNow),
                NextBlueGemAdAvailableAt = DateTime.UtcNow,
                RunData = new RunData { LastRunCompletedAt = Timestamp.FromDateTime(DateTime.UtcNow) },
                LoginRewardInfo = new LoginRewardInfo()
            };
            await _playerManagerService.UpsertPlayerData(existing);

            var result = await _playerManagerService.GetPlayerDataAsync(_testPlayerId);

            result.Should().NotBeNull();
            result.Id.Should().Be(_testPlayerId);
            result.Gold.Should().Be(500);
            result.HeroLevel.Should().Be(5);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task AddRerollRune_IncrementsRerollRuneCount()
        {
            var player = new PlayerData
            {
                Id = _testPlayerId,
                RerollRuneCount = 2,
                LastEnergyGiveStamp = Timestamp.FromDateTime(DateTime.UtcNow),
                NextBlueGemAdAt = Timestamp.FromDateTime(DateTime.UtcNow),
                LastRunCompletedAt = Timestamp.FromDateTime(DateTime.UtcNow),
                NextBlueGemAdAvailableAt = DateTime.UtcNow,
                RunData = new RunData { LastRunCompletedAt = Timestamp.FromDateTime(DateTime.UtcNow) }
            };
            await _playerManagerService.UpsertPlayerData(player);

            var result = await _playerManagerService.AddRerollRune(_testPlayerId);

            result.Should().NotBeNull();
            result.RerollRuneCount.Should().Be(3);
            var after = await _playerDataRepo.GetPlayerDataByPlayerId(_testPlayerId);
            after.RerollRuneCount.Should().Be(3);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task UseRerollRune_WhenHasRunes_DecrementsAndReturnsTrue()
        {
            var player = new PlayerData
            {
                Id = _testPlayerId,
                RerollRuneCount = 1,
                LastEnergyGiveStamp = Timestamp.FromDateTime(DateTime.UtcNow),
                NextBlueGemAdAt = Timestamp.FromDateTime(DateTime.UtcNow),
                LastRunCompletedAt = Timestamp.FromDateTime(DateTime.UtcNow),
                NextBlueGemAdAvailableAt = DateTime.UtcNow,
                RunData = new RunData { LastRunCompletedAt = Timestamp.FromDateTime(DateTime.UtcNow) }
            };
            await _playerManagerService.UpsertPlayerData(player);

            var result = await _playerManagerService.UseRerollRune(_testPlayerId);

            result.Success.Should().BeTrue();
            var after = await _playerDataRepo.GetPlayerDataByPlayerId(_testPlayerId);
            after.RerollRuneCount.Should().Be(0);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task UseRerollRune_WhenNoRunes_ReturnsFalse()
        {
            var player = new PlayerData
            {
                Id = _testPlayerId,
                RerollRuneCount = 0,
                LastEnergyGiveStamp = Timestamp.FromDateTime(DateTime.UtcNow),
                NextBlueGemAdAt = Timestamp.FromDateTime(DateTime.UtcNow),
                LastRunCompletedAt = Timestamp.FromDateTime(DateTime.UtcNow),
                NextBlueGemAdAvailableAt = DateTime.UtcNow,
                RunData = new RunData { LastRunCompletedAt = Timestamp.FromDateTime(DateTime.UtcNow) },
                ExplicitUpserts = new List<string>() { nameof(PlayerData.RerollRuneCount) }
            };

            await _playerManagerService.UpsertPlayerData(player);

            var result = await _playerManagerService.UseRerollRune(_testPlayerId);

            result.Success.Should().BeFalse();
            var after = await _playerDataRepo.GetPlayerDataByPlayerId(_testPlayerId);
            after.RerollRuneCount.Should().Be(0);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task TakeCraftingMaterialsAndGold_WithEnoughResources_UpdatesPlayerAndReturnsTrue()
        {
            var player = new PlayerData
            {
                Id = _testPlayerId,
                Gold = 200,
                CraftingMaterials = 50,
                LastEnergyGiveStamp = Timestamp.FromDateTime(DateTime.UtcNow),
                NextBlueGemAdAt = Timestamp.FromDateTime(DateTime.UtcNow),
                LastRunCompletedAt = Timestamp.FromDateTime(DateTime.UtcNow),
                NextBlueGemAdAvailableAt = DateTime.UtcNow,
                RunData = new RunData { LastRunCompletedAt = Timestamp.FromDateTime(DateTime.UtcNow) }
            };
            await _playerManagerService.UpsertPlayerData(player);

            var result = await _playerManagerService.TakeCraftingMaterialsAndGold(_testPlayerId, 100, 10);

            result.Success.Should().BeTrue();
            var after = await _playerDataRepo.GetPlayerDataByPlayerId(_testPlayerId);
            after.Gold.Should().Be(100);
            after.CraftingMaterials.Should().Be(40);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task TakeCraftingMaterialsAndGold_WithInsufficientGold_ReturnsFalse()
        {
            var player = new PlayerData
            {
                Id = _testPlayerId,
                Gold = 50,
                CraftingMaterials = 50,
                LastEnergyGiveStamp = Timestamp.FromDateTime(DateTime.UtcNow),
                NextBlueGemAdAt = Timestamp.FromDateTime(DateTime.UtcNow),
                LastRunCompletedAt = Timestamp.FromDateTime(DateTime.UtcNow),
                NextBlueGemAdAvailableAt = DateTime.UtcNow,
                RunData = new RunData { LastRunCompletedAt = Timestamp.FromDateTime(DateTime.UtcNow) }
            };
            await _playerManagerService.UpsertPlayerData(player);

            var result = await _playerManagerService.TakeCraftingMaterialsAndGold(_testPlayerId, 100, 10);

            result.Success.Should().BeFalse();
            var after = await _playerDataRepo.GetPlayerDataByPlayerId(_testPlayerId);
            after.Gold.Should().Be(50);
            after.CraftingMaterials.Should().Be(50);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task LevelUp_WithEnoughCrystals_IncrementsLevelAndSpendsCrystals()
        {
            var player = new PlayerData
            {
                Id = _testPlayerId,
                Crystals = 5,
                NLCR = 3,
                HeroLevel = 1,
                TalentPoints = 0,
                LastEnergyGiveStamp = Timestamp.FromDateTime(DateTime.UtcNow),
                NextBlueGemAdAt = Timestamp.FromDateTime(DateTime.UtcNow),
                LastRunCompletedAt = Timestamp.FromDateTime(DateTime.UtcNow),
                NextBlueGemAdAvailableAt = DateTime.UtcNow,
                RunData = new RunData { LastRunCompletedAt = Timestamp.FromDateTime(DateTime.UtcNow) },
                ExplicitUpserts = new List<string> { nameof(PlayerData.TalentPoints)}
            };
            await _playerManagerService.UpsertPlayerData(player);
            var ctx = new TestExecutionContext { PlayerId = _testPlayerId, UserId = "TestUser" };

            var result = await _playerManagerService.LevelUp(ctx);

            result.Should().NotBeNull();
            result.HeroLevel.Should().Be(2);
            result.Crystals.Should().Be(2);
            result.NLCR.Should().Be(3);
            result.TalentPoints.Should().Be(1);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task LevelUp_WithInsufficientCrystals_ReturnsUnchanged()
        {
            var player = new PlayerData
            {
                Id = _testPlayerId,
                Crystals = 2,
                NLCR = 3,
                HeroLevel = 1,
                LastEnergyGiveStamp = Timestamp.FromDateTime(DateTime.UtcNow),
                NextBlueGemAdAt = Timestamp.FromDateTime(DateTime.UtcNow),
                LastRunCompletedAt = Timestamp.FromDateTime(DateTime.UtcNow),
                NextBlueGemAdAvailableAt = DateTime.UtcNow,
                RunData = new RunData { LastRunCompletedAt = Timestamp.FromDateTime(DateTime.UtcNow) }
            };
            await _playerManagerService.UpsertPlayerData(player);
            var ctx = new TestExecutionContext { PlayerId = _testPlayerId, UserId = "TestUser" };

            var result = await _playerManagerService.LevelUp(ctx);

            result.Should().NotBeNull();
            result.HeroLevel.Should().Be(1);
            result.Crystals.Should().Be(2);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task UpdateDeviceId_SetsDeviceId()
        {
            var player = new PlayerData
            {
                Id = _testPlayerId,
                DeviceId = "old-device",
                LastEnergyGiveStamp = Timestamp.FromDateTime(DateTime.UtcNow),
                NextBlueGemAdAt = Timestamp.FromDateTime(DateTime.UtcNow),
                LastRunCompletedAt = Timestamp.FromDateTime(DateTime.UtcNow),
                NextBlueGemAdAvailableAt = DateTime.UtcNow,
                RunData = new RunData { LastRunCompletedAt = Timestamp.FromDateTime(DateTime.UtcNow) }
            };
            await _playerManagerService.UpsertPlayerData(player);
            var ctx = new TestExecutionContext { PlayerId = _testPlayerId, UserId = "TestUser" };

            await _playerManagerService.UpdateDeviceId(ctx, "new-device-id");

            var after = await _playerDataRepo.GetPlayerDataByPlayerId(_testPlayerId);
            after.DeviceId.Should().Be("new-device-id");
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task UpdatePlayerIdUsername_UpdatesUsername()
        {
            //var player = new PlayerData
            //{
            //    Id = _testPlayerId,
            //    PlayerId = "OldName",
            //    LastEnergyGiveStamp = Timestamp.FromDateTime(DateTime.UtcNow),
            //    NextBlueGemAdAt = Timestamp.FromDateTime(DateTime.UtcNow),
            //    LastRunCompletedAt = Timestamp.FromDateTime(DateTime.UtcNow),
            //    NextBlueGemAdAvailableAt = DateTime.UtcNow,
            //    RunData = new RunData { LastRunCompletedAt = Timestamp.FromDateTime(DateTime.UtcNow) }
            //};
            //await _playerManagerService.UpsertPlayerData(player);

            var result = await _playerManagerService.UpdatePlayerIdUsername(_testPlayerId, "NewName");

            result.Should().BeTrue();
            var after = await _playerDataRepo.GetPlayerDataByPlayerId(_testPlayerId);
            after.PlayerId.Should().Be("NewName");
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task IncreaseItemDropCount_IncrementsRunDataItemsDropped()
        {
            var player = new PlayerData
            {
                Id = _testPlayerId,
                RunData = new RunData { ItemsDropped = 2, LastRunCompletedAt = Timestamp.FromDateTime(DateTime.UtcNow) },
                LastEnergyGiveStamp = Timestamp.FromDateTime(DateTime.UtcNow),
                NextBlueGemAdAt = Timestamp.FromDateTime(DateTime.UtcNow),
                LastRunCompletedAt = Timestamp.FromDateTime(DateTime.UtcNow),
                NextBlueGemAdAvailableAt = DateTime.UtcNow
            };
            await _playerManagerService.UpsertPlayerData(player);

            var result = await _playerManagerService.IncreaseItemDropCountForId(_testPlayerId);

            result.Should().NotBeNull();
            result.RunData.Should().NotBeNull();
            result.RunData.ItemsDropped.Should().Be(3);
            var after = await _playerDataRepo.GetPlayerDataByPlayerId(_testPlayerId);
            after.RunData.ItemsDropped.Should().Be(3);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task UpsertPlayerData_PersistsData()
        {
            var player = new PlayerData
            {
                Id = _testPlayerId,
                Gold = 999,
                HeroLevel = 10,
                LastEnergyGiveStamp = Timestamp.FromDateTime(DateTime.UtcNow),
                NextBlueGemAdAt = Timestamp.FromDateTime(DateTime.UtcNow),
                LastRunCompletedAt = Timestamp.FromDateTime(DateTime.UtcNow),
                NextBlueGemAdAvailableAt = DateTime.UtcNow,
                RunData = new RunData { LastRunCompletedAt = Timestamp.FromDateTime(DateTime.UtcNow) }
            };

            var result = await _playerManagerService.UpsertPlayerData(player, player);

            result.Should().NotBeNull();
            var after = await _playerDataRepo.GetPlayerDataByPlayerId(_testPlayerId);
            after.Gold.Should().Be(999);
            after.HeroLevel.Should().Be(10);
        }
    }
}
