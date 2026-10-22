using CloudLogic.DI;
using CloudLogic.Response;
using CloudLogic.Response.Base;
using CloudLogic.Services.Achievements;
using CloudLogic.Services.Items;
using CloudLogic.Util;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.CloudCode.Core;

namespace CloudLogic.Modules
{
    public class LaboratoryModule
    {
        private readonly ILogger _logger;
        private readonly EventsLogger _eventLogger;

        public LaboratoryModule(ILogger<DropModule> logger, EventsLogger eventLogger)
        {
            _logger = logger;
            _eventLogger = eventLogger;
        }


        [CloudCodeFunction(nameof(MergeItems))]
        public async Task<ItemResponse> MergeItems(IExecutionContext ctx, List<string> itemIds)
        {
            var response = new ItemResponse() { Success = false, AdditionalInfo = ctx.CorrelationId };            

            try
            {
                var mergeDTO = await CloudDi.WithScope(_logger, ctx, sp =>
                sp.GetRequiredService<ArmoryService>().MergeItems(itemIds, ctx.PlayerId));

                if (mergeDTO != null)
                {
                    response.Success = true;
                    response.Item = mergeDTO.Item;
                    response.GoldCost = mergeDTO.Gold;
                    response.CraftingCost = mergeDTO.CraftingMaterials;

                    var progress = await CloudDi.WithScope(_logger, ctx, sp =>
                         sp.GetRequiredService<AchievementsService>().IncrementCraft(ctx.PlayerId, mergeDTO.Item.Rarity));

                    response.AchievementProgressInfo = progress.AchievementProgressD;
                    response.QuestsProgressInfo = progress.QuestsProgressD;
                }

                return response;
            }
            catch (System.Exception ex)
            {
                response.AdditionalInfo = ctx.CorrelationId;
                _eventLogger.LogWithEx(nameof(MergeItems), ex);

                return response;
            }
            
        }

        [CloudCodeFunction(nameof(AlterItem))]
        public async Task<AlterItemResponse> AlterItem(IExecutionContext ctx, string itemId, int statNum, bool isSuper)
        {
            var response = new AlterItemResponse() { Success = false, AdditionalInfo = ctx.CorrelationId };

            try
            {
                response =  await CloudDi.WithScope(_logger, ctx, sp =>
                sp.GetRequiredService<AlterItemService>()
                .AlterItem(itemId, statNum, isSuper, ctx));
            }
            catch (System.Exception ex)
            {
                response.AdditionalInfo = ctx.CorrelationId;
                _eventLogger.LogWithEx(nameof(AlterItem), ex);                
            }

            return response;
        }

        [CloudCodeFunction(nameof(ReforgeItem))]
        public async Task<ReforgeItemResponse> ReforgeItem(IExecutionContext ctx, string itemId, int statNum)
        {
            var response = new ReforgeItemResponse() { Success = false, AdditionalInfo = ctx.CorrelationId };
            
            try
            {
                response = await CloudDi.WithScope(_logger, ctx, sp =>
                    sp.GetRequiredService<ReforgeService>().ReforgeItem(itemId, statNum, ctx));
            }
            catch (System.Exception ex)
            {
                response.AdditionalInfo = ctx.CorrelationId;

                _eventLogger.LogWithEx(nameof(ReforgeItem), ex);
            }

            return response;
        }

        [CloudCodeFunction(nameof(SalvageWeapon))]
        public async Task<SalvageItemResponse> SalvageWeapon(IExecutionContext ctx, string weaponId)
        {
            var response = new SalvageItemResponse() { Success = false, AdditionalInfo = ctx.CorrelationId };

            try
            {
                response = await CloudDi.WithScope(_logger, ctx, sp =>
                sp.GetRequiredService<BlacksmithService>().SalvageWeapon(weaponId, ctx));
            }
            catch (System.Exception ex)
            {
                response.AdditionalInfo = ctx.CorrelationId;

                _eventLogger.LogWithEx(nameof(SalvageWeapon), ex);
            }

            return response;
        }

        [CloudCodeFunction(nameof(SalvageWeaponMass))]
        public async Task<SalvageItemResponse> SalvageWeaponMass(IExecutionContext ctx, List<string> weaponIds)
        {
            var response = new SalvageItemResponse() { Success = false, AdditionalInfo = ctx.CorrelationId };

            try
            {
                response = await CloudDi.WithScope(_logger, ctx, sp =>
                    sp.GetRequiredService<BlacksmithService>().SalvageWeaponMass(weaponIds, ctx));
            }
            catch (System.Exception ex)
            {
                response.AdditionalInfo = ctx.CorrelationId;

                _eventLogger.LogWithEx(nameof(SalvageWeapon), ex);
            }

            return response;

        }
    }
}
