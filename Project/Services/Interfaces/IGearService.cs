using CloudLogic.RepoModels.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Services.CloudCode.Core;

namespace CloudLogic.Services.Interfaces
{
    public interface IGearService
    {
        Task EquipItem(Item item);
        Task EquipItems(List<Item> item);
        Task UnequipItem(ItemType slotId);
        Task UnequipItems(List<ItemType> slotId);

        Task HandleEquipmentChange(List<ItemType> unequips, List<Item> equips);

        Task<Dictionary<ItemType, Item>> GetEquipedItems();
        Task<List<string>> GetEquippedItemsIds();

        Task<bool> SetInitialGiven(int choice);

    }
}
