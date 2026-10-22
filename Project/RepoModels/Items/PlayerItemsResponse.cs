using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudLogic.RepoModels.Items
{
    public class PlayerItemsResponse
    {
        public Dictionary<ItemType, Item> EquippedItems { get; set; } = new Dictionary<ItemType, Item>();

        public List<Item> StashItems { get; set; } = new List<Item>();
    }
}
