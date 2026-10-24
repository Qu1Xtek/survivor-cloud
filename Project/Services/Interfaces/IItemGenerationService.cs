using CloudLogic.RepoModels.Items;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.CloudCode.Core;
using ItemTemplateModel = CloudLogic.RepoModels.Items.ItemTemplate;

namespace CloudLogic.Services.Items
{
    public interface IItemGenerationService
    {
        Task<Item> CreateItemFromTemplate(string templateId, RarityType rarity, int level = 0);
        Item CreateItemFromSpecificTemplates(List<ItemTemplateModel> itemTemplates, RarityType rarity);
        Task<Item> CreateItemForLevel(int levelMin, int levelMax, RarityType rarity, ItemType type = 0);
        Item CreateRerollRuneItem();
    }
}
