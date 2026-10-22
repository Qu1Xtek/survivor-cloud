using CloudLogic.RepoModels.Items;
using CloudLogic.Repositories;
using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CloudLogic.Services.Items
{
    public class ItemCreateService : IItemCreateService
    {
        private readonly ItemsRepo _itemsRepo;

        public ItemCreateService(ItemsRepo itemsRepo)
        {
            _itemsRepo = itemsRepo;
        }

        public Task<Item> CreateItem(Item item, Transaction tran = null)
        {
            item.DroppedAt = Timestamp.FromDateTime(DateTime.UtcNow);
            return _itemsRepo.StoreItem(item);
        }

        public Task<List<Item>> CreateItems(List<Item> items, Transaction tran = null)
        {
            if (items == null || items.Count == 0)
                return Task.FromResult(items ?? new List<Item>());

            foreach (var it in items)
            {
                if (it != null)
                    it.DroppedAt = Timestamp.FromDateTime(DateTime.UtcNow);
            }

            return _itemsRepo.StoreItems(items);
        }
    }
}

