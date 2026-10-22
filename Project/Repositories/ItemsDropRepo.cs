using Microsoft.Extensions.Logging;
using Google.Cloud.Firestore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using CloudLogic.RepoModels.Items;
using System.Net.WebSockets;

namespace CloudLogic.Repositories
{
    public class ItemDropRepo : BaseFireStoreService
    {
        public ItemDropRepo(ILogger logger) : base("ItemDrops", logger)
        {
        }

        public virtual async Task<ItemDrop> AddItemDrop(string playerId, DroppedItem droppedItem, Transaction tran =null)
        {
            var itemDrop = new ItemDrop();

            itemDrop.Id = playerId;
            itemDrop.RunDropBag = new Dictionary<string, DroppedItem>();
            itemDrop.RunDropBag[droppedItem.Id] = droppedItem;

            return await CreateRecord(itemDrop, tran);
        }     

        internal virtual async Task<ItemDrop> GetItemDropsByPlayerId(string playerId, Transaction tran = null)
        {
            var document = await ReadRecordById(playerId, tran);
            return ConvertDocument<ItemDrop>(document);
        }

        public virtual async Task UpsertRunBagAsync(ItemDrop itemDrop)
        {
            await UpsertRecord(itemDrop);
        }

        public async Task<bool> DeleteItemDropsByIds(List<string> itemIds)
        {
            return await DeleteRecordsByIds(itemIds);
        }

    }
}
