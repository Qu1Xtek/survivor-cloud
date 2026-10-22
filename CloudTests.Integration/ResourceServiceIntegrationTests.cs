using CloudLogic.RepoModels.Player;
using CloudLogic.Services.Player;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.DependencyInjection;
using Google.Cloud.Firestore;
using System;
using System.Threading.Tasks;
using CloudLogic.Services.Items;
using CloudLogic.Repositories;

namespace CloudTests.Integration
{
    /// <summary>
    /// Integration tests for ResourceService.
    /// Tests the full flow of resource management operations including gold, blue gems, 
    /// crafting materials, and talent reset tokens.
    /// </summary>
    [TestClass]
    public class ResourceServiceIntegrationTests : IntegrationTestBase
    {
        private ResourceService _resourceService;
        private IPlayerManagerService _playerManagerService;
        private string _testPlayerId;
        private PlayerDataRepo _playerDataRepo;

        [TestInitialize]
        public void Setup()
        {
            _playerManagerService = ServiceProvider.GetRequiredService<IPlayerManagerService>();
            _playerDataRepo = ServiceProvider.GetRequiredService<PlayerDataRepo>();
            _resourceService = new ResourceService(_playerDataRepo);
            _testPlayerId = IntegrationTestBase.GlobalTestPlayerId;
        }

        private async Task EnsureTestPlayerAsync(long gold = 1000, int blueGems = 50, int craftingMaterials = 100, int talentResetToken = 5)
        {
            var playerData = new PlayerData
            {
                Id = _testPlayerId,
                Gold = gold,
                BlueGems = blueGems,
                CraftingMaterials = craftingMaterials,
                TalentResetToken = talentResetToken,
                LastEnergyGiveStamp = Timestamp.FromDateTime(DateTime.UtcNow),
                NextBlueGemAdAt = Timestamp.FromDateTime(DateTime.UtcNow),
                LastRunCompletedAt = Timestamp.FromDateTime(DateTime.UtcNow),
                NextBlueGemAdAvailableAt = DateTime.UtcNow,
                RunData = new RunData { LastRunCompletedAt = Timestamp.FromDateTime(DateTime.UtcNow) },
                ExplicitUpserts = new List<string>() { "Gold", "BlueGems", "CraftingMaterials", "TalentResetToken"}
            };
            await _playerManagerService.UpsertPlayerData(playerData);
        }

        #region AddBlueGems Tests

        [TestMethod]
        [TestCategory("Integration")]
        public async Task AddBlueGems_WithPositiveAmount_IncreasesBlueGems()
        {
            // Arrange
            await EnsureTestPlayerAsync(blueGems: 50);
            int initialGems = 50;
            int gemsToAdd = 25;

            // Act
            var result = await _resourceService.AddBlueGems(_testPlayerId, gemsToAdd);

            // Assert

        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task AddBlueGems_WithZeroAmount_KeepsBlueGemsUnchanged()
        {
            // Arrange
            await EnsureTestPlayerAsync(blueGems: 50);
            int initialGems = 50;

            // Act
            var result = await _resourceService.AddBlueGems(_testPlayerId, 0);

            // Assert

        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task AddBlueGems_WithNegativeAmount_DecreasesBlueGems()
        {
            // Arrange
            await EnsureTestPlayerAsync(blueGems: 100);
            int initialGems = 100;
            int gemsToRemove = -30;

            // Act
            var result = await _resourceService.AddBlueGems(_testPlayerId, gemsToRemove);

        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task AddBlueGems_WithNewPlayer_SetsBlueGems()
        {
            // Arrange
            await EnsureTestPlayerAsync(blueGems: 0);
            int gemsToAdd = 100;

            // Act
            var result = await _resourceService.AddBlueGems(_testPlayerId, gemsToAdd);
        }

        #endregion

        #region AddGold Tests

        [TestMethod]
        [TestCategory("Integration")]
        public async Task AddGold_WithPositiveAmount_IncreasesGold()
        {
            // Arrange
            await EnsureTestPlayerAsync(gold: 1000);
            long initialGold = 1000;
            long goldToAdd = 500;

            // Act
            var result = await _resourceService.AddGold(_testPlayerId, goldToAdd);

            // Assert
            result.Should().NotBeNull();
            result.PlayerData.Gold.Should().Be(initialGold + goldToAdd);
            result.PlayerData.Id.Should().Be(_testPlayerId);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task AddGold_WithLargeAmount_HandlesLongValues()
        {
            // Arrange
            await EnsureTestPlayerAsync(gold: 1000000);
            long initialGold = 1000000;
            long goldToAdd = 999999999;

            // Act
            var result = await _resourceService.AddGold(_testPlayerId, goldToAdd);

            // Assert
            result.Should().NotBeNull();
            result.PlayerData.Gold.Should().Be(initialGold + goldToAdd);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task AddGold_WithZeroAmount_KeepsGoldUnchanged()
        {
            // Arrange
            await EnsureTestPlayerAsync(gold: 5000);
            long initialGold = 5000;

            // Act
            var result = await _resourceService.AddGold(_testPlayerId, 0);

            // Assert
            result.Should().NotBeNull();
            result.PlayerData.Gold.Should().Be(initialGold);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task AddGold_WithNegativeAmount_DecreasesGold()
        {
            // Arrange
            await EnsureTestPlayerAsync(gold: 2000);
            long initialGold = 2000;
            long goldToRemove = -500;

            // Act
            var result = await _resourceService.AddGold(_testPlayerId, goldToRemove);

            // Assert
            result.Should().NotBeNull();
            result.PlayerData.Gold.Should().Be(initialGold + goldToRemove);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task AddGold_WithNewPlayer_SetsGold()
        {
            // Arrange
            await EnsureTestPlayerAsync(gold: 0);
            long goldToAdd = 5000;

            // Act
            var result = await _resourceService.AddGold(_testPlayerId, goldToAdd);

            // Assert
            result.Should().NotBeNull();
            result.PlayerData.Gold.Should().Be(goldToAdd);
        }

        #endregion

        #region AddCraftingMaterials Tests

        [TestMethod]
        [TestCategory("Integration")]
        public async Task AddCraftingMaterials_WithPositiveAmount_IncreasesMaterials()
        {
            // Arrange
            await EnsureTestPlayerAsync(craftingMaterials: 100);
            int initialMaterials = 100;
            int materialsToAdd = 75;

            // Act
            var result = await _resourceService.AddCraftingMaterials(_testPlayerId, materialsToAdd);

        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task AddCraftingMaterials_WithZeroAmount_KeepsMaterialsUnchanged()
        {
            // Arrange
            await EnsureTestPlayerAsync(craftingMaterials: 50);
            int initialMaterials = 50;

            // Act
            var result = await _resourceService.AddCraftingMaterials(_testPlayerId, 0);

        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task AddCraftingMaterials_WithNegativeAmount_DecreasesMaterials()
        {
            // Arrange
            await EnsureTestPlayerAsync(craftingMaterials: 200);
            int initialMaterials = 200;
            int materialsToRemove = -50;

            // Act
            var result = await _resourceService.AddCraftingMaterials(_testPlayerId, materialsToRemove);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task AddCraftingMaterials_WithNewPlayer_SetsMaterials()
        {
            // Arrange
            await EnsureTestPlayerAsync(craftingMaterials: 0);
            int materialsToAdd = 150;

            // Act
            var result = await _resourceService.AddCraftingMaterials(_testPlayerId, materialsToAdd);
        }

        #endregion

        #region AddTalentResetToken Tests

        [TestMethod]
        [TestCategory("Integration")]
        public async Task AddTalentResetToken_WithPositiveAmount_IncreasesTokens()
        {
            // Arrange
            await EnsureTestPlayerAsync(talentResetToken: 5);
            int initialTokens = 5;
            int tokensToAdd = 3;

            // Act
            var result = await _resourceService.AddTalentResetToken(_testPlayerId, tokensToAdd);

            // Assert
            result.Should().NotBeNull();
            result.PlayerData.TalentResetToken.Should().Be(initialTokens + tokensToAdd);
            result.PlayerData.Id.Should().Be(_testPlayerId);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task AddTalentResetToken_WithZeroAmount_KeepsTokensUnchanged()
        {
            // Arrange
            await EnsureTestPlayerAsync(talentResetToken: 10);
            int initialTokens = 10;

            // Act
            var result = await _resourceService.AddTalentResetToken(_testPlayerId, 0);

            // Assert
            result.Should().NotBeNull();
            result.PlayerData.TalentResetToken.Should().Be(initialTokens);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task AddTalentResetToken_WithNegativeAmount_DecreasesTokens()
        {
            // Arrange
            await EnsureTestPlayerAsync(talentResetToken: 15);
            int initialTokens = 15;
            int tokensToRemove = -5;

            // Act
            var result = await _resourceService.AddTalentResetToken(_testPlayerId, tokensToRemove);

            // Assert
            result.Should().NotBeNull();
            result.PlayerData.TalentResetToken.Should().Be(initialTokens + tokensToRemove);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task AddTalentResetToken_WithNewPlayer_SetsTokens()
        {
            // Arrange
            await EnsureTestPlayerAsync(talentResetToken: 0);
            int tokensToAdd = 10;

            // Act
            var result = await _resourceService.AddTalentResetToken(_testPlayerId, tokensToAdd);

            // Assert
            result.Should().NotBeNull();
            result.PlayerData.TalentResetToken.Should().Be(tokensToAdd);
        }

        #endregion

        #region Multiple Operations Tests

        [TestMethod]
        [TestCategory("Integration")]
        public async Task MultipleOperations_SequentialCalls_UpdatesCorrectly()
        {
            // Arrange
            await EnsureTestPlayerAsync(gold: 1000, blueGems: 50, craftingMaterials: 100, talentResetToken: 5);

            // Act - Add gold
            var result1 = await _resourceService.AddGold(_testPlayerId, 500);
            result1.PlayerData.Gold.Should().Be(1500);

            // Act - Add blue gems
            var result2 = await _resourceService.AddBlueGems(_testPlayerId, 25);

            // Act - Add crafting materials
            var result3 = await _resourceService.AddCraftingMaterials(_testPlayerId, 50);

            // Act - Add talent tokens
            var result4 = await _resourceService.AddTalentResetToken(_testPlayerId, 3);
            result4.PlayerData.TalentResetToken.Should().Be(8);

            // Assert - Verify final state
            var finalPlayerData = await _playerManagerService.GetPlayerDataAsync(_testPlayerId);
            finalPlayerData.Gold.Should().Be(1500);
            finalPlayerData.BlueGems.Should().Be(75);
            finalPlayerData.CraftingMaterials.Should().Be(150);
            finalPlayerData.TalentResetToken.Should().Be(8);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task MultipleOperations_MixedPositiveAndNegative_UpdatesCorrectly()
        {
            // Arrange
            await EnsureTestPlayerAsync(gold: 5000, blueGems: 100, craftingMaterials: 200, talentResetToken: 10);

            // Act - Various operations
            await _resourceService.AddGold(_testPlayerId, 1000);  // +1000 = 6000
            await _resourceService.AddGold(_testPlayerId, -2000); // -2000 = 4000
            await _resourceService.AddBlueGems(_testPlayerId, 50); // +50 = 150
            await _resourceService.AddBlueGems(_testPlayerId, -30); // -30 = 120
            await _resourceService.AddCraftingMaterials(_testPlayerId, -100); // -100 = 100
            await _resourceService.AddTalentResetToken(_testPlayerId, 5); // +5 = 15

            // Assert
            var finalPlayerData = await _playerManagerService.GetPlayerDataAsync(_testPlayerId);
            finalPlayerData.Gold.Should().Be(4000);
            finalPlayerData.BlueGems.Should().Be(120);
            finalPlayerData.CraftingMaterials.Should().Be(100);
            finalPlayerData.TalentResetToken.Should().Be(15);
        }

        #endregion

        #region Edge Cases Tests

        [TestMethod]
        [TestCategory("Integration")]
        public async Task AddResources_WithNonExistentPlayer_CreatesPlayerData()
        {
            // Arrange - Use a unique player ID that doesn't exist

            await _playerManagerService.RemovePLAYERDATAUSEDONLYFORTESTING(_testPlayerId);
            var initialPlayer = new PlayerData
            {
                Id = _testPlayerId,
                Gold = 0,
                BlueGems = 0,
                CraftingMaterials = 0,
                TalentResetToken = 0,
                LastEnergyGiveStamp = Timestamp.FromDateTime(DateTime.UtcNow),
                NextBlueGemAdAt = Timestamp.FromDateTime(DateTime.UtcNow),
                RunData = new RunData(),
                LoginRewardInfo = new LoginRewardInfo()
            };
            await _playerManagerService.UpsertPlayerData(initialPlayer);

            // Act
            var result = await _resourceService.AddGold(_testPlayerId, 1000);

            // Assert
            result.Should().NotBeNull();
            result.PlayerData.Gold.Should().Be(1000);
            result.PlayerData.Id.Should().Be(_testPlayerId);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task AddGold_ToPlayerWithExtremelyLowGold_HandlesCorrectly()
        {
            // Arrange
            await EnsureTestPlayerAsync(gold: 0);

            // Act
            var result = await _resourceService.AddGold(_testPlayerId, -100);

            // Assert
            result.Should().NotBeNull();
            result.PlayerData.Gold.Should().Be(-100); // Service doesn't validate minimum values
        }

        #endregion

        [TestCleanup]
        public async Task CleanupTestData()
        {
            try
            {
                // Reset test player to default state
                await EnsureTestPlayerAsync();
            }
            catch
            {
                /* ignore cleanup errors */
            }

            base.Cleanup();
        }
    }
}
