using CloudLogic.RepoModels.Items;
using Google.Cloud.Firestore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CloudLogic.Services.Items
{
    public interface IItemCreateService
    {
        Task<Item> CreateItem(Item item, Transaction tran = null);

        Task<List<Item>> CreateItems(List<Item> items, Transaction tran = null);
    }
}

