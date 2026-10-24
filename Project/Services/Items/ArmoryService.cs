using CloudLogic.RepoModels.Achievements;
using CloudLogic.RepoModels.Items;
using CloudLogic.RepoModels.Player;
using CloudLogic.Repositories;
using CloudLogic.Response.DTO;
using CloudLogic.Services.Achievements;
using CloudLogic.Services.Player;
using CloudLogic.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Item = CloudLogic.RepoModels.Items.Item;

namespace CloudLogic.Services.Items
{
    public class ArmoryService
    {
        private IItemTemplateService _itemTemplateService;
        private IPlayerManagerService _playerManagerService;
        private ItemsRepo _itemsRepo;
        private IItemGenerationService _itemGenerationService;
        private ItemsService _itemsService;
        private AchievementsService _achievementsService;

        private readonly Dictionary<RarityType, int> _rarityLimits = new()
        {
            { RarityType.Common, 2 },
            { RarityType.Uncommon, 3 },
            { RarityType.Rare, 4 }
        };

        private readonly Dictionary<RarityType, Func<int, int>> _minTemplateLvls = new()
        {
            { RarityType.Uncommon, (int lvl) => {return Math.Max(lvl, 3); } },
            { RarityType.Rare, (int lvl) => {return Math.Max(lvl, 4); } },
            { RarityType.Epic, (int lvl) => {return Math.Max(lvl, 5); } },
        };

        public ArmoryService(IItemTemplateService itemTemplateService,
            ItemsRepo itemsRepo,
            IItemGenerationService generationService,
            IPlayerManagerService playerManagerService,
            ItemsService itemsService,
            AchievementsService achievementsService)
        {
            _itemTemplateService = itemTemplateService;
            _itemsRepo = itemsRepo;
            _itemGenerationService = generationService;
            _playerManagerService = playerManagerService;
            _itemsService = itemsService;
            _achievementsService = achievementsService;
        }

        public async Task<ItemMergeDTO> MergeItems(List<string> itemIds, string playerId)
        {
            ItemMergeDTO response = new ItemMergeDTO();
            RarityType? craftedRarity = null;
            await _itemsRepo.RunOurTransactionAsync(async tran =>
            {
                var myItems = await _itemsRepo.GetMyOwnItems(playerId, itemIds);
                var requiredCount = _rarityLimits[myItems[0].Rarity];

                if (myItems.Any(x => x.Type == ItemType.DuelWeapon) || myItems.DistinctBy(x => x.Rarity).Count() > 1 || myItems.Count < requiredCount)
                {
                    return;
                }

                var newRarity = myItems[0].Rarity + 1;
                craftedRarity = newRarity;
                var itemLevel = GetLevel(myItems);
                var price = 5 * itemLevel * ((int)newRarity);
                var craftingMatCost = (int)newRarity + 1;
                var templateLevel = _minTemplateLvls[newRarity].Invoke(itemLevel);

                var enoughResources = await _playerManagerService.TakeCraftingMaterialsAndGold(
                    playerId,
                    price,
                    craftingMatCost,
                    tran);

                if (!enoughResources.Success)
                {
                    response = null;
                    return;
                }

                var item = await GenerateItem(myItems, itemLevel, templateLevel, newRarity);
                item.OwnerId = playerId;

                await _itemsService.CreateItem(item, tran);
                await _itemsRepo.DeleteItemsByIds(itemIds.ToList(), tran);

                response.Item = item;
                response.Gold = BindableInt.Of(price);
                response.CraftingMaterials = BindableInt.Of(craftingMatCost);
            });

            return response;
        }

        private async Task<Item> GenerateItem(List<Item> items,
            int itemLevel,
            int templateLevel,
            RarityType newItemRarity)
        {
            var rarity = items[0].Rarity + 1;

            var newItemType = items.DistinctBy(i => i.Type).Count() == 1
                ? items[0]?.Type
                : null;

            var templates = await _itemTemplateService
                .GetItemTemplatesForMeging(templateLevel, newItemRarity, newItemType);

            Item generatedItem = _itemGenerationService
                .CreateItemFromSpecificTemplates(templates, newItemRarity);

            if (generatedItem.Level > itemLevel)
            {
                generatedItem.Level = itemLevel;
            }

            return generatedItem;
        }

        private int GetLevel(List<Item> items)
        {
            float totalLevelPoints = items.Sum(i => i.Level);

            var level = ((int)Math.Ceiling(totalLevelPoints / items.Count));            

            return level;
        }
    }
}
