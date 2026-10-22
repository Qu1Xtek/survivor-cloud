using CloudLogic.Services.Items;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace CloudTests.Integration
{
    /// <summary>
    /// Integration tests for ItemsService.PickItemDrop.
    /// These tests use real Firestore (or emulator) and test the full flow of picking up item drops.
    /// </summary>
    [TestClass]
    public class ItemsServicePickItemDropIntegrationTests : IntegrationTestBase
    {
        private ItemsService _itemsService;
        private ItemDropService _itemDropService;
        private string _testPlayerId;

        [TestInitialize]
        public void Setup()
        {
            _itemsService = ServiceProvider.GetRequiredService<ItemsService>();
            _itemDropService = ServiceProvider.GetRequiredService<ItemDropService>();
            _testPlayerId = IntegrationTestBase.GlobalTestPlayerId;
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task PickItemDrop_WithValidItemDropAndSpace_CreatesItemAndDeletesDrop()
        {
            // Arrange
            int selectedLevel = 10;
            var droppedItem = await _itemDropService.DropItem(_testPlayerId);

            // Act
            var pickedItem = await _itemsService.PickItemDrop(_testPlayerId, droppedItem.Id, hasSpace: true);

            // Assert
            pickedItem.Should().NotBeNull();
            pickedItem.OwnerId.Should().Be(_testPlayerId);
            pickedItem.Rarity.Should().Be(droppedItem.Rarity);

            // Verify the drop was deleted
            var remainingDrop = await _itemDropService.GetItemDropByIdAsync(_testPlayerId, droppedItem.Id);
            var items = await _itemsService.GetAllPlayerItems(_testPlayerId);

            items.Should().Contain(i => i.Id == pickedItem.Id);
            remainingDrop.Should().BeNull();
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task PickItemDrop_WithNoSpace_ReturnsNullAndKeepsDrop()
        {
            // Arrange
            int selectedLevel = 10;
            var droppedItem = await _itemDropService.DropItem(_testPlayerId);

            // Act
            var pickedItem = await _itemsService.PickItemDrop(_testPlayerId, droppedItem.Id, hasSpace: false);

            // Assert
            pickedItem.Should().BeNull();

            // Verify the drop was deleted even when there's no space
            var remainingDrop = await _itemDropService.GetItemDropByIdAsync(_testPlayerId, droppedItem.Id);

            remainingDrop.Should().NotBeNull();
        }

        [TestCleanup]
        public async Task CleanupTestData()
        {
            // Clean up any remaining item drops for this test player
            try
            {
                // GetRunBagAsync requires dropId but it's not used - pass empty string
                var itemDrop = await _itemDropService.GetRunBagAsync(_testPlayerId, "");
                var items = await _itemsService.GetAllPlayerItems(_testPlayerId);

                if (itemDrop?.RunDropBag != null)
                {
                    foreach (var drop in itemDrop.RunDropBag.Values)
                    {
                        await _itemDropService.DeleteItemDropById(_testPlayerId, drop.Id);
                    }
                }

                await _itemsService.DestroyManyItems(items.Select(i => i.Id).ToList());

            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }
}
