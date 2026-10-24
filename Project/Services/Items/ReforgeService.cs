using CloudLogic.DI;
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
    public class ReforgeService
    {
        private IItemTemplateService _itemTemplateService;
        private ItemsRepo _itemsRepo;
        private AchievementsService _achievementsService;
        private IPlayerManagerService _playerManagerService;
        private RequestContextAccessor _requestContext;

        public ReforgeService(
            IItemTemplateService itemTemplateService,
            ItemsRepo itemsRepo,
            IPlayerManagerService playerManagerService,
            AchievementsService achievementsService,
            RequestContextAccessor requestContext)
        {
            _itemTemplateService = itemTemplateService;
            _itemsRepo = itemsRepo;
            _playerManagerService = playerManagerService;
            _achievementsService = achievementsService;
            _requestContext = requestContext;
        }
        public async Task<ReforgeItemResponse> ReforgeItem(string itemId, int statNum, IExecutionContext ctx)
        {
            Item item = await _itemsRepo.GetPlayerItem(ctx.PlayerId, itemId);

            if (item.ReforgedTimes > 0 && !item.Stats[statNum].Reforged)
            {
                return new ReforgeItemResponse() { Success = false };
            }

            int alteringQuotient = (int)Math.Max(Math.Pow(item.ReforgedTimes, 2), 2);
            var goldCost = item.Level * alteringQuotient;
            // Tests + economy expect Common => 2 mats
            var craftingMatCost = (int)item.Rarity + 2;
            var template = await _itemTemplateService.GetItemTemplateById(item.ItemTemplateId);

            if (template == null)
            {
                return new ReforgeItemResponse() { Success = false };
            }

            if (template.Type == ItemType.DuelWeapon)
            {
                template = DuelDagger.At(template, item.Level, item.Rarity);
            }

            var reforgedItem = ItemStatGenerator.GetReforgedItem(item, template, statNum);

            var enoughResources = await _playerManagerService.TakeCraftingMaterialsAndGold(ctx.PlayerId, goldCost, craftingMatCost);


            if (!enoughResources.Success)
            {
                return new ReforgeItemResponse() { Success = false };
            }

            reforgedItem.Stats[statNum].Reforged = true;
            reforgedItem.ReforgedTimes++;

            await _itemsRepo.UpdateItem(reforgedItem);

            var progress = await _achievementsService.Increment(ctx.PlayerId, AchievementType.ChangeStatValue.ToString(), 1);

            return BuildSuccessResponse(item.Id, reforgedItem.Stats[statNum].Value, goldCost, craftingMatCost,
                progress.AchievementProgress.Stats[AchievementType.ChangeStatValue.ToString()],
                progress.QuestsProgress.Stats[AchievementType.ChangeStatValue.ToString()]);
        }

        private ReforgeItemResponse BuildSuccessResponse(string itemId, int newStatValue, int goldCost,
            int craftingMatCost,
            long achValue,
            long questValue)
        {
            return new ReforgeItemResponse()
            {
                Success = true,
                ItemId = itemId,
                NewStatValue = newStatValue,
                ResourcesTaken = new Dictionary<string, int> { { "gold", goldCost }, { "craftingEssence", craftingMatCost } },
                AchievementProgressInfo = new Dictionary<string, long>() { [AchievementType.ChangeStatValue.ToString()] = achValue},
                QuestsProgressInfo = new Dictionary<string, long>() { [AchievementType.ChangeStatValue.ToString()] = questValue },
                AdditionalInfo = _requestContext.ExecutionContext?.CorrelationId
            };
        }
    }
}
