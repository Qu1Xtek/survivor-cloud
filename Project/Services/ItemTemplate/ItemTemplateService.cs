using CloudLogic.Cache;
using CloudLogic.RepoModels.Items;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudLogic.Services;
using ItemTemplateModel = CloudLogic.RepoModels.Items.ItemTemplate;

namespace CloudLogic.Services.ItemTemplate
{
    public class ItemTemplateService : IItemTemplateService
    {

        private ICloudCache _cache;
        private ItemTemplateRepo _repo;
        private ILogger _logger;

        public ItemTemplateService(ICloudCache cache, ItemTemplateRepo repo, ILogger logger)
        {
            _cache = cache;
            _repo = repo;
            _logger = logger;
        }


        public async Task<ItemTemplateModel> StoreNewTemplate(ItemTemplateModel template)
        {
            return await _repo.SaveNewTemplate(template);
        }

        public async Task<List<ItemTemplateModel>> GetItemTemplates()
        {
            var templates = await _cache.GetItemTemplates();

            if (templates == null)
            {
                //_logger.LogInformation("Templates in cache are null trying to get em from somewhere else");
                var firestoreTemplates = await _repo.GetItemTemplates();

                return await _cache.LoadTemplatesFromFirestoreAsync(firestoreTemplates);
            }

            return templates;
        }

        public async Task<List<ItemTemplateModel>> RefreshRedisCache()
        {
            //_logger.LogInformation("Templates in cache are null trying to get em from somewhere else");
            var firestoreTemplates = await _repo.GetItemTemplates();

            await _cache.LoadTemplatesFromFirestoreAsync(firestoreTemplates);

            return firestoreTemplates;
        }


        public async Task<ItemTemplateModel> GetDaggerTemplate()
        {
            return (await GetItemTemplates()).FirstOrDefault(x => x.Type == ItemType.DuelWeapon);
        }

        private async Task<List<ItemTemplateModel>> GearTemplates()
        {
            return (await GetItemTemplates()).Where(x => x.Type != ItemType.DuelWeapon).ToList();
        }

        public async Task<ItemTemplateModel> GetItemTemplateById(string id)
        {
            var templates = await GetItemTemplates();

            return templates.FirstOrDefault(x => x.Id == id);
        }

        public async Task<ItemTemplateModel> GetItemTemplate(RarityType rarity,
            int levelMin,
            int levelMax,
            bool exclusiveRarity,
            ItemType type = 0)
        {
            var templates = await GetGenerationTemplates(rarity, levelMin, levelMax, exclusiveRarity, type);

            return templates[new Random().Next(0, templates.Count)];
        }

        public async Task<List<ItemTemplateModel>> GetGenerationTemplates(RarityType rarity,
            int levelMin,
            int levelMax,
            bool exclusiveRarity,
            ItemType type = 0)
        {
            List<ItemTemplateModel> itemPool = new List<ItemTemplateModel>();

            var fullTemplatePool = await GetItemTemplatesForGeneration(rarity, levelMin, levelMax, exclusiveRarity);

            var maxRarityPossible = fullTemplatePool.Max(x => x.Rarity);

            rarity = rarity > maxRarityPossible ? maxRarityPossible : rarity;
            
            while ((int)rarity > -1)
            {
                if (type == ItemType.Weapon)
                {
                    itemPool = fullTemplatePool
                     .Where(i => (i.Rarity == rarity || (i.RarityOptions != null && i.RarityOptions.ContainsKey((int)rarity))) && i.Type == ItemType.Weapon)
                     .ToList();
                }

                if (type != 0 && type != ItemType.Weapon)
                {
                    itemPool = fullTemplatePool
                    .Where(i => (i.Rarity == rarity || (i.RarityOptions != null && i.RarityOptions.ContainsKey((int)rarity))) && (type == ItemType.Head ? i.Type != ItemType.Weapon : i.Type == type))
                    .ToList();
                }

                if (type == 0)
                {
                    itemPool = fullTemplatePool
                    .Where(i => i.Rarity == rarity || (i.RarityOptions != null && i.RarityOptions.ContainsKey((int)rarity)))
                    .ToList();
                }

                               
                if (itemPool.Count > 0)
                {
                    break;
                }

                rarity--;
            }


            return itemPool;
        }

        // Returns the requested level if a template of the given rarity exists there;
        // otherwise the closest level (above or below) that has one. Falls back to the
        // requested level if no template of that rarity exists at all.
        public async Task<int> GetClosestLevelForRarityForDailyLogin(int requestedLevel, RarityType rarity)
        {
            var templates = await GearTemplates();

            var levels = templates
                .Where(t => (t.Rarity == rarity ||
                      (t.RarityOptions != null && t.RarityOptions.ContainsKey((int)rarity))) 
                     && t.Type != ItemType.Weapon)

                .Select(t => t.Level)
                .Distinct()
                .ToList();

            if (levels.Count == 0 || levels.Contains(requestedLevel))
            {
                return requestedLevel;
            }

            // Closest by absolute distance; ties resolve to the lower level.
            return levels
                .OrderBy(l => Math.Abs(l - requestedLevel))
                .ThenBy(l => l)
                .First();
        }

        public async Task<List<ItemTemplateModel>> GetTemplatesForSpecific(RarityType rarity, int level, bool exclusiveRarity)
        {
            var templates = await GearTemplates();

            return templates.Where(x => x.Level <= level && x.Level >= level - 5).ToList();
        }

        public async Task<List<ItemTemplateModel>> GetItemTemplatesForGeneration(
            RarityType rarity,
            int levelMin,
            int levelMax,
            bool exclusiveRarity)
        {
            var templates = await GearTemplates();

            if (templates.Count == 0)
            {
                return new List<ItemTemplateModel>();
            }

            var minAvailable = templates.Min(x => x.Level);
            var maxAvailable = templates.Max(x => x.Level);

            levelMin = Math.Clamp(levelMin, minAvailable, maxAvailable);
            levelMax = Math.Clamp(levelMax, minAvailable, maxAvailable);

            if (levelMin > levelMax)
            {
                (levelMin, levelMax) = (levelMax, levelMin);
            }

            return templates.Where(x => x.Level <= levelMax && x.Level >= levelMin).ToList();
        }

        public async Task<List<ItemTemplateModel>> GetItemTemplatesBy(int? level = null, RarityType? rarity = null, ItemType? itemType = null)
        {
            var templates = await GearTemplates();

            var filtered = templates.Where(t =>
                (!level.HasValue || t.Level == level.Value) &&
                (!rarity.HasValue || t.Rarity >= rarity.Value) &&
                (!itemType.HasValue || t.Type == itemType.Value)
            );

            return filtered.ToList();
        }

        public async Task<List<ItemTemplateModel>> GetItemTemplatesForMeging(int? level = null, RarityType? rarity = null, ItemType? itemType = null)
        {
            return (await GetItemTemplatesBy(level, rarity, itemType)).Where(x => x.Type != ItemType.Weapon).ToList();
        }

        public async Task<(List<ItemTemplateModel> templates, int resolvedLevel, RarityType resolvedRarity)> ResolveTemplatesForLevel(
            int requestedLevel,
            RarityType desiredRarity)
        {
            var templates = await GearTemplates();

            if (templates == null || templates.Count == 0)
            {
                return (new List<ItemTemplateModel>(), requestedLevel, desiredRarity);
            }

            var groupedByLevel = templates
                .GroupBy(t => t.Level)
                .ToDictionary(g => g.Key, g => g.ToList());

            var distinctLevels = groupedByLevel.Keys.OrderBy(level => level).ToList();

            if (distinctLevels.Count == 0)
            {
                return (new List<ItemTemplateModel>(), requestedLevel, desiredRarity);
            }

            var minAvailableLevel = distinctLevels.First();
            var maxAvailableLevel = distinctLevels.Last();

            requestedLevel = requestedLevel > 30 ? 30 : requestedLevel;

            var targetLevel = Math.Clamp(requestedLevel, minAvailableLevel, maxAvailableLevel);

            for (var level = targetLevel; level >= minAvailableLevel; level--)
            {
                if (!groupedByLevel.TryGetValue(level, out var levelTemplates) || levelTemplates.Count == 0)
                {
                    continue;
                }

                var matches = levelTemplates
                    .Where(t => t.Rarity == desiredRarity)
                    .ToList();

                if (matches.Count > 0)
                {
                    return (matches, level, desiredRarity);
                }
            }

            return (new List<ItemTemplateModel>(), targetLevel, desiredRarity);
        }
    }
}

