using CloudLogic.DI;
using CloudLogic.RepoModels.Items;
using CloudLogic.Response;
using CloudLogic.Response.Base;
using CloudLogic.Services.Achievements;
using CloudLogic.Services.Items;
using CloudLogic.Util;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;

namespace CloudLogic.Modules
{
    public class ItemsModule
    {
        private readonly IGameApiClient _gameApiClient;
        private readonly ILogger _logger;

        public ItemsModule(ILogger<ItemsModule> logger)
        {
            _logger = logger;
            _gameApiClient = GameApiClient.Create();
        }

        [CloudCodeFunction(nameof(PickDrops))]
        public async Task<ItemResponse> PickDrops(IExecutionContext ctx, string dropId)
        {
            return await CloudDi.WithScope(_logger!, ctx, async sp =>
            {
                var eventLogger = sp.GetRequiredService<EventsLogger>();

                try
                {
                    var itemsService = sp.GetRequiredService<ItemsService>();
                    var achievementService = sp.GetRequiredService<AchievementsService>();
                    var ownedItemService = sp.GetRequiredService<OwnedItemsService>();
                    var itemGenerationService = sp.GetRequiredService<IItemGenerationService>();

                    var hasSpace = await ownedItemService.CanPickItem(ctx.PlayerId);

                    var itemDrop = await itemsService.PickItemDrop(ctx.PlayerId, dropId, hasSpace);

                    var response = new ItemResponse()
                    {
                        Success = true,
                        Item = itemDrop,
                        AdditionalInfo = ctx?.CorrelationId
                    };

                    if (itemDrop != null)
                    {
                        var progress = await achievementService.IncrementDrop(ctx.PlayerId, itemDrop.Rarity);
                        response.AchievementProgressInfo = progress.AchievementProgressD;
                        response.QuestsProgressInfo = progress.QuestsProgressD;
                    }

                    return response;
                }
                catch (System.Exception ex)
                {
                    eventLogger.LogWithEx(nameof(PickDrops), ex);

                    return new ItemResponse()
                    {
                        Success = false,
                        AdditionalInfo = ctx?.CorrelationId
                    };
                }
            });
        }

        [CloudCodeFunction(nameof(SellPlayerItem))]
        public async Task<bool> SellPlayerItem(IExecutionContext ctx, string itemId)
        {
            return await CloudDi.WithScope(_logger, ctx, sp =>
                sp.GetRequiredService<SellService>().SellPlayerItem(itemId, ctx.PlayerId));
        }

        [CloudCodeFunction(nameof(SellMassItems))]
        public async Task<bool> SellMassItems(IExecutionContext ctx, List<string> itemIds)
        {
            return await CloudDi.WithScope(_logger, ctx, sp =>
                sp.GetRequiredService<SellService>().SellMassItems(itemIds, ctx.PlayerId));
        }

        [CloudCodeFunction(nameof(GetPlayerItemsSplit))]
        public async Task<PlayerItemsResponse> GetPlayerItemsSplit(IExecutionContext ctx)
        {
            return await CloudDi.WithScope(_logger, ctx, sp =>
                sp.GetRequiredService<OwnedItemsService>().GetPlayerItemsSplit(ctx.PlayerId));
        }
    }
}
