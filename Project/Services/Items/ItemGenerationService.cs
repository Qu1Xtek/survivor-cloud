using CloudLogic.RepoModels.Items;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ItemTemplateModel = CloudLogic.RepoModels.Items.ItemTemplate;

namespace CloudLogic.Services.Items
{
    public class ItemGenerationService : IItemGenerationService
    {
        private IItemTemplateService _templateService;
        private readonly ILogger _logger;

        public ItemGenerationService(IItemTemplateService templateService, ILogger logger)
        {
            _templateService = templateService;
            _logger = logger;
        }

        public async Task<Item> CreateItemFromTemplate(string templateId, RarityType rarity, int level = 0)
        {
            var itemTMPL = await _templateService.GetItemTemplateById(templateId);

            if (itemTMPL.Type == ItemType.DuelWeapon)
            {
                itemTMPL = DuelDagger.At(itemTMPL, level, rarity);
            }

            return ItemStatGenerator.GenerateStats(itemTMPL, rarity);
        }

        public Item CreateItemFromSpecificTemplates(List<ItemTemplateModel> itemTemplates, RarityType rarity)
        {
            var itemTMPL = itemTemplates[new Random().Next(0, itemTemplates.Count)];

            return ItemStatGenerator.GenerateStats(itemTMPL, rarity);
        }


        public async Task<Item> CreateItemForLevel(int levelMin, int levelMax, RarityType rarity, ItemType type = 0)
        {
            var min = Math.Max(levelMin, 1);
            var max = Math.Max(levelMax, 1);
            var template = await _templateService.GetItemTemplate(rarity, min, max, false, type);
            template.Rarity = rarity;
            var generatedItem = ItemStatGenerator.GenerateStats(template, rarity);

            return generatedItem;
        }

        public static RarityType GetRarity(int enemyLevel)
        {
            int index = new Random().Next(0, 1000);

            if (index >= 950)
            {
                return RarityType.Epic;
            }
            else if (index >= 800)
            {
                return RarityType.Rare;
            }
            else if (index >= 650)
            {
                return RarityType.Uncommon;
            }
            else
            {
                return RarityType.Common;
            }
        }

        public Item CreateRerollRuneItem()
        {
            Item item = new Item()
            {
                Name = "Reroll Rune",
                Icon = "reroll_rune",
                Type = ItemType.Reagent,
                Rarity = RarityType.Legendary,
                Stats = new List<ItemStat>(),
                GoldValue = 0,
                Level = 1
            };

            return item;
        }
    }
}
