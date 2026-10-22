using CloudLogic.RepoModels.Items;
using CloudLogic.RepoModels.Player;
using CloudLogic.Services.Items;
using CloudLogic.Services.Player;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.DependencyInjection;
using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudTests.Integration
{
    /// <summary>
    /// Integration tests for ItemDropService.
    /// These tests use real Firestore (or emulator) and test the full flow.
    /// </summary>
    [TestClass]
    public class ItemDropIntegrationTests : IntegrationTestBase
    {
        private ItemDropService _itemDropService;
        private IPlayerManagerService _playerManagerService;
        private string _testPlayerId;

        [TestInitialize]
        public void Setup()
        {
            _itemDropService = ServiceProvider.GetRequiredService<ItemDropService>();
            _playerManagerService = ServiceProvider.GetRequiredService<IPlayerManagerService>();
            _testPlayerId = IntegrationTestBase.GlobalTestPlayerId;
        }

        private async Task EnsureTestPlayerAndRunBagAsync()
        {
            var playerData = new PlayerData
            {
                Id = _testPlayerId,
                RunData = new RunData { ItemsDropped = 0, LastRunCompletedAt = Timestamp.FromDateTime(DateTime.UtcNow.AddMinutes(-10))},
                LastEnergyGiveStamp = Timestamp.FromDateTime(DateTime.UtcNow)
            };
            await _playerManagerService.UpsertPlayerData(playerData);
            await _itemDropService.PrepareNewRunDropBag(_testPlayerId);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task PrepareNewRunDropBag_CreatesOrClearsRunBag()
        {
            await EnsureTestPlayerAndRunBagAsync();

            var bag = await _itemDropService.GetRunBagAsync(_testPlayerId, "");
            bag.Should().NotBeNull();
            bag.Id.Should().Be(_testPlayerId);
            bag.RunDropBag.Should().NotBeNull();
            bag.RunDropBag.Should().BeEmpty();
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task GetItemDropByIdAsync_WithExistingDrop_ReturnsDrop()
        {

            int level = 10;
            var dropped = await _itemDropService.DropItem(_testPlayerId);
            dropped.Should().NotBeNull();

            var found = await _itemDropService.GetItemDropByIdAsync(_testPlayerId, dropped.Id);
            found.Should().NotBeNull();
            found.Id.Should().Be(dropped.Id);
            found.PlayerId.Should().Be(_testPlayerId);
            found.ItemTemplateId.Should().Be(dropped.ItemTemplateId);
            found.DropType.Should().Be(dropped.DropType);

            await _itemDropService.DeleteItemDropById(_testPlayerId, dropped.Id);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task DeleteItemDropById_RemovesDropFromRunBag()
        {            
            int level = 10;
            var dropped = await _itemDropService.DropItem(_testPlayerId);
            dropped.Should().NotBeNull();

            await _itemDropService.DeleteItemDropById(_testPlayerId, dropped.Id);

            var found = await _itemDropService.GetItemDropByIdAsync(_testPlayerId, dropped.Id);
            found.Should().BeNull();
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task DropItem_WithValidSetup_ReturnsDroppedItem()
        {
            int level = 10;
            await EnsureTestPlayerAndRunBagAsync();
            var dropped = await _itemDropService.DropItem(_testPlayerId);

            dropped.Should().NotBeNull();
            dropped.PlayerId.Should().Be(_testPlayerId);
            dropped.ItemTemplateId.Should().NotBeNullOrEmpty();
            dropped.DropType.Should().BeOneOf(DropType.Item, DropType.RerollRune);
            dropped.Id.Should().NotBeNullOrEmpty();

            await _itemDropService.DeleteItemDropById(_testPlayerId, dropped.Id);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task DropItem_WhenItemsDroppedAtLimit_ReturnsNull()
        {
            var playerData = new PlayerData
            {
                Id = _testPlayerId,
                RunData = new RunData { ItemsDropped = 6 },
                LastEnergyGiveStamp = Timestamp.FromDateTime(DateTime.UtcNow)
            };

            await _playerManagerService.UpsertPlayerData(playerData);

            await _itemDropService.PrepareNewRunDropBag(_testPlayerId);

            var dropped = await _itemDropService.DropItem(_testPlayerId);

            dropped.Should().BeNull();
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task Drop_2_RunItems_And_Delete_Them()
        {
            int selectedLevel = 10;

            var droppedItem = await _itemDropService.DropItem(_testPlayerId);
            var droppedItem2 = await _itemDropService.DropItem(_testPlayerId);

            droppedItem.Should().NotBeNull();
            droppedItem.PlayerId.Should().Be(_testPlayerId);
            droppedItem.ItemTemplateId.Should().NotBeNullOrEmpty();
            droppedItem.DropType.Should().BeOneOf(DropType.Item, DropType.RerollRune);

            await _itemDropService.DeleteItemDropById(_testPlayerId, droppedItem.Id);
            await _itemDropService.DeleteItemDropById(_testPlayerId, droppedItem2.Id);

            var bag = await _itemDropService.GetRunBagAsync(_testPlayerId, "");
            bag.Should().NotBeNull();
            if (bag.RunDropBag != null)
            {
                bag.RunDropBag.Should().NotContainKey(droppedItem.Id);
                bag.RunDropBag.Should().NotContainKey(droppedItem2.Id);
            }
        }

        [TestCleanup]
        public async Task CleanupTestData()
        {
            try
            {
                var bag = await _itemDropService.GetRunBagAsync(_testPlayerId, "");
                if (bag?.RunDropBag != null)
                {
                    foreach (var drop in bag.RunDropBag.Values.ToList())
                        await _itemDropService.DeleteItemDropById(_testPlayerId, drop.Id);
                }
            }
            catch { /* ignore */ }
        }
    }
}
