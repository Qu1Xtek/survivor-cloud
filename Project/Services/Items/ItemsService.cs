using CloudLogic.RepoModels.Achievements;
using CloudLogic.RepoModels.Items;
using CloudLogic.Repositories;
using CloudLogic.Services.Achievements;
using CloudLogic.Services.Interfaces;
using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CloudLogic.Services.Items
{
    public class ItemsService
    {
        private ItemDropService _itemDropService;
        private IItemGenerationService _itemGenerationService;
        private IPlayerManagerService _playerManagerService;
        private ItemsRepo _itemsRepo;
        private AchievementsService _achievementsService;

        public ItemsService(ItemDropService itemDropService,
            ItemsRepo itemsRepo,
            IItemGenerationService itemGenerationService,
            IPlayerManagerService playerManagerService,
            AchievementsService achievementsService)
        {
            _itemDropService = itemDropService;
            _itemsRepo = itemsRepo;
            _itemGenerationService = itemGenerationService;
            _playerManagerService = playerManagerService;
            _achievementsService = achievementsService;
        }

        public async Task<List<Item>> GetAllPlayerItems(string playerId)                            
        {
            return await _itemsRepo.GetPlayerItems(playerId);
        }

        public async Task<Item> GetPlayerItem(string playerId, string itemId)
        {
            return await _itemsRepo.GetPlayerItem(playerId, itemId);
        }

        public async Task<Item> CreateItem(Item item, Transaction tran = null)
        {
            item.DroppedAt = Timestamp.FromDateTime(DateTime.UtcNow);
            return await _itemsRepo.StoreItem(item);
        }

        public async Task<Item> PickItemDrop(string playerId, string dropId, bool hasSpace)
        {
            var itemDrop = await _itemDropService.GetItemDropByIdAsync(playerId, dropId);
            if (itemDrop == null)
            {
                return null;
            }

            Item item;

            if (itemDrop.DropType == DropType.RerollRune)
            {
                await _playerManagerService.AddRerollRune(playerId);
                await _itemDropService.DeleteItemDropById(playerId, dropId);
                item = _itemGenerationService.CreateRerollRuneItem();
                item.OwnerId = playerId;
                //item = await CreateItem(item);
            }
            else
            {
                if (!hasSpace) // Delete itemDrop and stop execution no space 
                {
                    return null;
                }

                item = await _itemGenerationService.CreateItemFromTemplate(itemDrop.ItemTemplateId, itemDrop.Rarity);

                item.OwnerId = playerId;
                item = await CreateItem(item);
            }

            await _itemDropService.DeleteItemDropById(playerId, dropId);
            return item;
        }

        internal async Task<Item> GetFirstWeapon(string playerId, int choice)
        {
            var items = ItemStatGenerator.GenerateInitialWeapons();
            var item = items[choice];
            item.OwnerId = playerId;

            return await CreateItem(item);
        }

        public async Task DestroyItem(string itemId)
        {
            await _itemsRepo.DeleteItemById(itemId);
        }

        public async Task DestroyManyItems(List<string> itemIds)
        {
            await _itemsRepo.DeleteItemsByIds(itemIds);
        }

        public async Task<Item> GetItemById(string id)
        {
            return await _itemsRepo.GetItemById(id);
        }

        public async Task<List<Item>> GetItemsByIds(List<string> ids)
        {
            return await _itemsRepo.GetItemsByIds(ids);
        }
    }
}
