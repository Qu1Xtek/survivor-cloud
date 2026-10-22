using CloudLogic.RepoModels.Items;
using CloudLogic.RepoModels.Player;
using CloudLogic.Repositories;
using CloudLogic.Services.Items;
using CloudLogic.Services.Player;
using FluentAssertions;
using Google.Cloud.Firestore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudTests.Integration
{
    /// <summary>
    /// Integration tests for SellService (SellPlayerItem, SellMassItems).
    /// Uses real Firestore (or emulator).
    /// </summary>
    [TestClass]
    public class SellServiceIntegrationTests : IntegrationTestBase
    {
        private SellService _sellService;
        private ItemsRepo _itemsRepo;
        private PlayerDataRepo _playerDataRepo;
        private IPlayerManagerService _playerManagerService;
        private ItemsService _itemsService;
        private string _testPlayerId;

        [TestInitialize]
        public void Setup()
        {
            _sellService = ServiceProvider.GetRequiredService<SellService>();
            _itemsRepo = ServiceProvider.GetRequiredService<ItemsRepo>();
            _playerDataRepo = ServiceProvider.GetRequiredService<PlayerDataRepo>();
            _playerManagerService = ServiceProvider.GetRequiredService<IPlayerManagerService>();
            _itemsService = ServiceProvider.GetRequiredService<ItemsService>();
            _testPlayerId = GlobalTestPlayerId;
        }

        private async Task EnsureTestPlayerAsync(long gold)
        {
            var player = new PlayerData
            {
                Id = _testPlayerId,
                Gold = gold,
                CraftingMaterials = 0,
                LastEnergyGiveStamp = Timestamp.FromDateTime(DateTime.UtcNow),
                NextBlueGemAdAt = Timestamp.FromDateTime(DateTime.UtcNow),
                LastRunCompletedAt = Timestamp.FromDateTime(DateTime.UtcNow),
                NextBlueGemAdAvailableAt = DateTime.UtcNow,
                RunData = new RunData { LastRunCompletedAt = Timestamp.FromDateTime(DateTime.UtcNow) }
            };

            await _playerManagerService.UpsertPlayerData(player);
        }

        private async Task<Item> CreateAndStoreItemAsync(string ownerId, int goldValue)
        {
            var item = new Item
            {
                Id = Guid.NewGuid().ToString(),
                OwnerId = ownerId,
                Rarity = RarityType.Common,
                Level = 1,
                Type = ItemType.Head,
                ItemTemplateId = "test-template",
                Name = "TestItem",
                GoldValue = goldValue,
                Stats = new List<ItemStat>(),
            };
            return await _itemsRepo.StoreItem(item);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task SellPlayerItem_WithValidOwnedItem_AddsGoldAndDeletesItem()
        {
            await EnsureTestPlayerAsync(gold: 100);
            var item = await CreateAndStoreItemAsync(_testPlayerId, goldValue: 50);

            var result = await _sellService.SellPlayerItem(item.Id, _testPlayerId);

            result.Should().BeTrue();
            var goldAfter = await _playerManagerService.GetPlayerDataAsync(_testPlayerId);
            goldAfter.Gold.Should().Be(150);
            var itemAfter = await _itemsRepo.GetPlayerItem(_testPlayerId, item.Id);
            itemAfter.Should().BeNull();
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task SellPlayerItem_WithNonExistentItem_ReturnsFalse()
        {
            await EnsureTestPlayerAsync(gold: 100);

            var result = await _sellService.SellPlayerItem("non-existent-id", _testPlayerId);

            result.Should().BeFalse();
            var goldAfter = await _playerManagerService.GetPlayerDataAsync(_testPlayerId);
            goldAfter.Gold.Should().Be(100);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task SellPlayerItem_WithItemOwnedByOther_ReturnsFalse()
        {
            await EnsureTestPlayerAsync(gold: 100);
            var otherPlayerId = _testPlayerId + "_other";
            var otherPlayer = new PlayerData
            {
                Id = otherPlayerId,
                Gold = 0,
                CraftingMaterials = 0,
                LastEnergyGiveStamp = Timestamp.FromDateTime(DateTime.UtcNow),
                NextBlueGemAdAt = Timestamp.FromDateTime(DateTime.UtcNow),
                LastRunCompletedAt = Timestamp.FromDateTime(DateTime.UtcNow),
                NextBlueGemAdAvailableAt = DateTime.UtcNow,
                RunData = new RunData { LastRunCompletedAt = Timestamp.FromDateTime(DateTime.UtcNow) }
            };
            await _playerManagerService.UpsertPlayerData(otherPlayer);
            var item = await CreateAndStoreItemAsync(otherPlayerId, goldValue: 30);

            var result = await _sellService.SellPlayerItem(item.Id, _testPlayerId);

            result.Should().BeFalse();
            var goldAfter = await _playerManagerService.GetPlayerDataAsync(_testPlayerId);
            goldAfter.Gold.Should().Be(100);
            var itemAfter = await _itemsRepo.GetPlayerItem(otherPlayerId, item.Id);
            itemAfter.Should().NotBeNull();
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task SellMassItems_WithValidOwnedItems_AddsGoldAndDeletesItems()
        {
            await EnsureTestPlayerAsync(gold: 200);
            var a = await CreateAndStoreItemAsync(_testPlayerId, goldValue: 10);
            var b = await CreateAndStoreItemAsync(_testPlayerId, goldValue: 25);
            var ids = new List<string> { a.Id, b.Id };

            var result = await _sellService.SellMassItems(ids, _testPlayerId);

            result.Should().BeTrue();
            var goldAfter = await _playerManagerService.GetPlayerDataAsync(_testPlayerId);
            goldAfter.Gold.Should().Be(235);
            var owned = await _itemsRepo.GetMyOwnItems(_testPlayerId, ids);
            owned.Should().BeEmpty();
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task SellMassItems_WithEmptyList_ReturnsTrue()
        {
            await EnsureTestPlayerAsync(gold: 100);

            var result = await _sellService.SellMassItems(new List<string>(), _testPlayerId);

            result.Should().BeTrue();
            var goldAfter = await _playerManagerService.GetPlayerDataAsync(_testPlayerId);
            goldAfter.Gold.Should().Be(100);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task SellMassItems_WithMixedOwnedAndNotOwned_AddsGoldOnlyForOwned()
        {
            await EnsureTestPlayerAsync(gold: 50);
            var owned = await CreateAndStoreItemAsync(_testPlayerId, goldValue: 15);
            var ids = new List<string> { owned.Id, "non-existent-id" };

            var result = await _sellService.SellMassItems(ids, _testPlayerId);

            result.Should().BeTrue();
            var goldAfter = await _playerManagerService.GetPlayerDataAsync(_testPlayerId);
            goldAfter.Gold.Should().Be(65);
            var remaining = await _itemsRepo.GetPlayerItem(_testPlayerId, owned.Id);
            remaining.Should().BeNull();
        }

        [TestCleanup]
        public async Task CleanupTestData()
        {
            try
            {
                var items = await _itemsService.GetAllPlayerItems(_testPlayerId);
                if (items != null && items.Count > 0)
                    await _itemsService.DestroyManyItems(items.Select(i => i.Id).ToList());
            }
            catch { /* ignore */ }
        }
    }
}
