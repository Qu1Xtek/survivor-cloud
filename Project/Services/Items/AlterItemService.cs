using CloudLogic.RepoModels.Achievements;
using CloudLogic.RepoModels.Items;
using CloudLogic.Repositories;
using CloudLogic.Response;
using CloudLogic.Services.Achievements;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.CloudCode.Core;

namespace CloudLogic.Services.Items
{
    public class AlterItemService
    {
        private IItemTemplateService _itemTemplateService;
        private ItemsRepo _itemsRepository;
        private IPlayerManagerService _playerManagerService;
        private AchievementsService _achievementsService;


        public AlterItemService(IItemTemplateService itemTemplateService,
            ItemsRepo itemsRepo,
            IPlayerManagerService playerManagerService,
            AchievementsService achievementsService)
        {
            _itemTemplateService = itemTemplateService;
            _itemsRepository = itemsRepo;
            _playerManagerService = playerManagerService;
            _achievementsService = achievementsService;
        }
        public async Task<AlterItemResponse> AlterItem(string itemId, int statNum, bool isSuper, IExecutionContext ctx)
        {
            Item item = await _itemsRepository.GetPlayerItem(ctx.PlayerId, itemId);

            // If you try to alter different stats than already altered
            if (item.Type == ItemType.DuelWeapon || (item.AlteredTimes > 0 && !item.Stats[statNum].Altered))
            {
                return new AlterItemResponse() { Success = false };
            }

            int alteringCost = (int)Math.Max(Math.Pow(item.AlteredTimes, 2), 2);
            var goldCost = item.Level * alteringCost;
            int craftingMatCost = (int)item.Rarity + 2;
            string oldStat = item.Stats[statNum].Name;
            var template = await _itemTemplateService.GetItemTemplateById(item.ItemTemplateId);
            var alteredItem = ItemStatGenerator.AlterStat(item, template, statNum, isSuper);

            alteredItem.Stats[statNum].Altered = true;
            alteredItem.AlteredTimes++;

            var enoughResources = isSuper
                ? await _playerManagerService.UseRerollRune(ctx.PlayerId)
                : await _playerManagerService.TakeCraftingMaterialsAndGold(ctx.PlayerId, goldCost, craftingMatCost);

            if (!enoughResources.Success)
            {
                return new AlterItemResponse() { Success = false };
            }

            var newStat = item.Stats[statNum];
            
            await _itemsRepository.UpdateItem(alteredItem);

            var progress = await _achievementsService.Increment(ctx.PlayerId, AchievementType.ChangeStatType.ToString(), 1);

            return BuildSuccessResponse(alteredItem.Id, oldStat,
                newStat, goldCost, craftingMatCost, isSuper,
                progress.AchievementProgress.Stats[AchievementType.ChangeStatType.ToString()],
                progress.QuestsProgress.Stats[AchievementType.ChangeStatType.ToString()]);    
        }

        private Dictionary<string,int> BuildResourcesTaken(int goldCost, int craftingMatCost, bool isSuper)
        {
            if (isSuper)
            {
                return new Dictionary<string, int> { { "rune", 1 } };
            }
            else
            {
                return new Dictionary<string, int> { { "gold", goldCost }, { "craftingEssence", craftingMatCost } };
            }

        }

        private AlterItemResponse BuildSuccessResponse(string itemId, string oldStat, ItemStat newStat, int goldCost, int craftingMat, bool isSuper, long achValue, long questValue) 
        {
            return new AlterItemResponse()
            {
                Success = true,
                ItemId = itemId,
                OldStat = oldStat,
                NewStat = newStat.Name,
                NewStatValue = newStat.Value,
                ResourcesTaken = BuildResourcesTaken(goldCost, craftingMat, isSuper),
                AchievementProgressInfo = new Dictionary<string, long>() { [AchievementType.ChangeStatType.ToString()] = achValue},
                QuestsProgressInfo = new Dictionary<string, long>() { [AchievementType.ChangeStatType.ToString()] = questValue }
            };
        }

    }
}
