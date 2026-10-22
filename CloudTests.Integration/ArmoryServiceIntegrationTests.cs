using CloudLogic.RepoModels.Items;
using CloudLogic.RepoModels.Player;
using CloudLogic.Repositories;
using CloudLogic.Services.Items;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Google.Cloud.Firestore;
using CloudLogic.Services;
using CloudLogic.Services.Interfaces;

namespace CloudTests.Integration
{
    /// <summary>
    /// Integration tests for ArmoryService.MergeItems.
    /// Uses real Firestore (or emulator). Requires mergeable item templates for success cases.
    /// </summary>
    [TestClass]
    public class ArmoryServiceIntegrationTests : IntegrationTestBase
    {
        private ArmoryService _armoryService;
        private ItemsRepo _itemsRepo;
        private IPlayerManagerService _playerManagerService;
        private IItemTemplateService _itemTemplateService;
        private ItemsService _itemsService;
        private string _testPlayerId;
        private PlayerGemsRepo _playerGemsRepo;
        private IPlayerGemService _playerGemsService;

        [TestInitialize]
        public void Setup()
        {
            _armoryService = ServiceProvider.GetRequiredService<ArmoryService>();
            _itemsRepo = ServiceProvider.GetRequiredService<ItemsRepo>();
            _playerManagerService = ServiceProvider.GetRequiredService<IPlayerManagerService>();
            _itemTemplateService = ServiceProvider.GetRequiredService<IItemTemplateService>();
            _itemsService = ServiceProvider.GetRequiredService<ItemsService>();
            _testPlayerId = GlobalTestPlayerId;
            _playerGemsRepo = ServiceProvider.GetRequiredService<PlayerGemsRepo>();
            _playerGemsService = ServiceProvider.GetRequiredService<IPlayerGemService>();
        }

        private static List<int> IntRange(int a, int b) => new List<int> { a, b };

        private async Task EnsureTestPlayerAsync(long gold, int craftingMaterials)
        {
            var player = new PlayerData
            {
                Id = _testPlayerId,
                Gold = gold,
                CraftingMaterials = craftingMaterials,
                LastEnergyGiveStamp = Timestamp.FromDateTime(DateTime.UtcNow),
                NextBlueGemAdAt = Timestamp.FromDateTime(DateTime.UtcNow), 
                LastRunCompletedAt = Timestamp.FromDateTime(DateTime.UtcNow),
                NextBlueGemAdAvailableAt = DateTime.UtcNow,
                RunData = new RunData { LastRunCompletedAt = Timestamp.FromDateTime(DateTime.UtcNow)}
            };
            await _playerManagerService.UpsertPlayerData(player);
        }

        private async Task<Item> CreateAndStoreItemAsync(string ownerId, RarityType rarity, int level, ItemType type, string templateId)
        {
            var item = new Item
            {
                Id = Guid.NewGuid().ToString(),
                OwnerId = ownerId,
                Rarity = rarity,
                Level = level,
                Type = type,
                ItemTemplateId = templateId,
                Name = "Test",
                GoldValue = 10,
                Stats = new List<ItemStat>(),
            };
            return await _itemsRepo.StoreItem(item);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task MergeItems_WithValidCommonItems_ReturnsMergedItem()
        {
            await EnsureTestPlayerAsync(gold: 1000, craftingMaterials: 100);

            var templateId = (await _itemTemplateService.GetItemTemplatesForMeging(
                1, RarityType.Common, ItemType.Head))[0].Id;

            var a = await CreateAndStoreItemAsync(_testPlayerId, RarityType.Common, 1, ItemType.Head, templateId);
            var b = await CreateAndStoreItemAsync(_testPlayerId, RarityType.Common, 1, ItemType.Head, templateId);
            var ids = new List<string> { a.Id, b.Id };

            var response = await _armoryService.MergeItems(ids, _testPlayerId);
            var result = response.Item;

            result.Should().NotBeNull();
            result.Rarity.Should().Be(RarityType.Uncommon);
            result.OwnerId.Should().Be(_testPlayerId);
            result.Type.Should().Be(ItemType.Head);
            result.Level.Should().Be(1);

            var owned = await _itemsRepo.GetMyOwnItems(_testPlayerId, ids);
            owned.Should().BeEmpty();
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task MergeItems_WithMixedRarities_ReturnsNull()
        {
            await EnsureTestPlayerAsync(gold: 1000, craftingMaterials: 100);

            var templateId = (await _itemTemplateService.GetItemTemplatesForMeging(3, RarityType.Uncommon, ItemType.Head))[0].Id;
            var common = await CreateAndStoreItemAsync(_testPlayerId, RarityType.Common, 1, ItemType.Head, templateId);
            var uncommon = await CreateAndStoreItemAsync(_testPlayerId, RarityType.Uncommon, 1, ItemType.Head, templateId);
            var ids = new List<string> { common.Id, uncommon.Id };

            var result = await _armoryService.MergeItems(ids, _testPlayerId);

            result.Should().BeNull();
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task MergeItems_WithInsufficientCount_ReturnsNull()
        {
            await EnsureTestPlayerAsync(gold: 1000, craftingMaterials: 100);

            var templateId = (await _itemTemplateService.GetItemTemplatesForMeging(3, RarityType.Uncommon, ItemType.Head))[0].Id;
            var single = await CreateAndStoreItemAsync(_testPlayerId, RarityType.Common, 1, ItemType.Head, templateId);
            var ids = new List<string> { single.Id };

            var result = await _armoryService.MergeItems(ids, _testPlayerId);

            result.Should().BeNull();
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task MergeItems_WithInsufficientResources_ReturnsNull()
        {
            await EnsureTestPlayerAsync(gold: -5, craftingMaterials: -1);

            var templateId = (await _itemTemplateService.GetItemTemplatesForMeging(3, RarityType.Uncommon, ItemType.Head))[0].Id;
            var a = await CreateAndStoreItemAsync(_testPlayerId, RarityType.Common, 1, ItemType.Head, templateId);
            var b = await CreateAndStoreItemAsync(_testPlayerId, RarityType.Common, 1, ItemType.Head, templateId);
            var ids = new List<string> { a.Id, b.Id };

            var result = await _armoryService.MergeItems(ids, _testPlayerId);

            result.Should().BeNull();

            var stillOwned = await _itemsRepo.GetMyOwnItems(_testPlayerId, ids);
            stillOwned.Should().HaveCount(2);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task MergeItems_WithItemsNotAllOwned_ReturnsNull()
        {
            await EnsureTestPlayerAsync(gold: 1000, craftingMaterials: 100);

            var templateId = (await _itemTemplateService.GetItemTemplatesForMeging(3, RarityType.Uncommon, ItemType.Head))[0].Id;
            var valid = await CreateAndStoreItemAsync(_testPlayerId, RarityType.Common, 1, ItemType.Head, templateId);
            var ids = new List<string> { valid.Id, "non-existent-id" };

            var result = await _armoryService.MergeItems(ids, _testPlayerId);

            result.Should().BeNull();
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
