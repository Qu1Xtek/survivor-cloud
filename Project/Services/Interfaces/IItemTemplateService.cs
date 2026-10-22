using CloudLogic.RepoModels.Items;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ItemTemplateModel = CloudLogic.RepoModels.Items.ItemTemplate;

namespace CloudLogic.Services
{
    public interface IItemTemplateService
    {
        Task<ItemTemplateModel> StoreNewTemplate(ItemTemplateModel template);
        Task<List<ItemTemplateModel>> GetItemTemplates();
        Task<List<ItemTemplateModel>> RefreshRedisCache();
        Task<ItemTemplateModel> GetItemTemplateById(string id);
        Task<List<ItemTemplateModel>> GetItemTemplatesForGeneration(RarityType rarity, int levelMin, int levelMax, bool exclusiveRarity);
        Task<ItemTemplateModel> GetItemTemplate(RarityType rarity, int levelMin, int levelMax, bool exclusiveRarity, ItemType type = 0);
        Task<List<ItemTemplateModel>> GetGenerationTemplates(RarityType rarity, int levelMin, int levelMax, bool exclusiveRarity, ItemType type = 0);
        Task<List<ItemTemplateModel>> GetItemTemplatesBy(int? level = null, RarityType? rarity = null, ItemType? itemType = null);
        Task<List<ItemTemplateModel>> GetItemTemplatesForMeging(int? level = null, RarityType? rarity = null, ItemType? itemType = null);
        Task<(List<ItemTemplateModel> templates, int resolvedLevel, RarityType resolvedRarity)> ResolveTemplatesForLevel(int requestedLevel, RarityType desiredRarity);
        Task<int> GetClosestLevelForRarityForDailyLogin(int requestedLevel, RarityType rarity);
    }
}
