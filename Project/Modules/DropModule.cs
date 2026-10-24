using CloudLogic.DI;
using CloudLogic.RepoModels.Achievements;
using CloudLogic.Response;
using CloudLogic.Response.DTO;
using CloudLogic.Services.Achievements;
using CloudLogic.Services.Items;
using CloudLogic.Services.Player;
using CloudLogic.Util;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.CloudCode.Core;

namespace CloudLogic.Modules
{
    public class DropModule
    {
        private readonly ILogger _logger;
        private readonly RequestContextAccessor _contextAccessor;

        public DropModule(ILogger<DropModule> logger, RequestContextAccessor contextAccessor)
        {
            _logger = logger;
            _contextAccessor = contextAccessor;
        }

        [CloudCodeFunction(nameof(ClaimDrops))]
        public async Task<BaseResponse> ClaimDrops(IExecutionContext ctx, List<long> goldBagIds, int craftMatCount)
        {
            var result = await CloudDi.WithScope(_logger, ctx, async sp =>
            {
                var resourceDropService = sp.GetRequiredService<ResourceDropService>();
                var achievementsService = sp.GetRequiredService<AchievementsService>();

                var playerData = await resourceDropService.PickupGoldAndCraftMats(ctx.PlayerId, goldBagIds, craftMatCount);
                var incrementResult = await achievementsService.IncrementMultiple(
                    ctx.PlayerId,
                    new Dictionary<string, long>()
                    {
                        [AchievementType.CraftingMaterialsCollected.ToString()] = playerData.RunData.CraftingMaterialsPicked,
                        [AchievementType.GoldCollected.ToString()] = playerData.RunData.GoldPicked.Sum()
                    }
                );

                return new BaseResponse()
                {
                    Success = true,
                    AchievementProgressInfo = incrementResult.AchievementProgressD,
                    QuestsProgressInfo = incrementResult.QuestsProgressD,
                    PlayerData = new PlayerDataDTO()
                    {
                        CraftingMaterials = BindableInt.Of(playerData.CraftingMaterials),
                        Gold = BindableLong.Of(playerData.Gold)
                    },
                    AdditionalInfo = _contextAccessor?.ExecutionContext?.CorrelationId,
                };
            });

            return result;
        }

        [CloudCodeFunction(nameof(DropGold))]
        public async Task<long> DropGold(IExecutionContext ctx)
        {
            var result = await CloudDi.WithScope(_logger, ctx, sp =>
                sp.GetRequiredService<ResourceDropService>()
                .DropGold(ctx.PlayerId));

            return result;
        }

        [CloudCodeFunction(nameof(DropCraftingMaterial))]
        public async Task<long> DropCraftingMaterial(IExecutionContext ctx)
        {
            return await CloudDi.WithScope(_logger, ctx, sp =>
                sp.GetRequiredService<ResourceDropService>()
                .DropCraftingMaterial(ctx.PlayerId));
        }

        [CloudCodeFunction(nameof(CreateItemDrop))]
        public async Task<ItemDropDTO> CreateItemDrop(IExecutionContext ctx, int clientVersion = 0)
        {
            return await CloudDi.WithScope(_logger, ctx, async sp =>
            {
                var droppedItem = await sp
                .GetRequiredService<ItemDropService>()
                .DropItem(ctx.PlayerId, false, clientVersion);

                return new ItemDropDTO
                {
                    Id = droppedItem?.Id,
                    Rarity = droppedItem?.Rarity ?? 0,
                    Success = droppedItem != null,
                    AdditionalInfo = droppedItem.AdditionalInfo
                };
            });
        }

        [CloudCodeFunction(nameof(CreateBossDrop))]
        public async Task<ItemDropDTO> CreateBossDrop(IExecutionContext ctx, int clientVersion = 0)
        {
            return await CloudDi.WithScope(_logger, ctx, async sp =>
            {
                var droppedItem = await sp.GetRequiredService<ItemDropService>()
                    .DropItem(ctx.PlayerId, true, clientVersion);

                // PlayerId is already set in CreateItemDrop
                return new ItemDropDTO
                {
                    Id = droppedItem?.Id,
                    Rarity = droppedItem?.Rarity ?? 0,
                    Success = droppedItem != null,
                    AdditionalInfo = droppedItem.AdditionalInfo
                }; ;
            });
        }
    }
}
