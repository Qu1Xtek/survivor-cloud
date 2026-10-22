using CloudLogic.DI;
using CloudLogic.RepoModels.Items;
using CloudLogic.RepoModels.Player;
using CloudLogic.RepoModels.Skin;
using CloudLogic.Repositories;
using CloudLogic.Response.DTO;
using CloudLogic.Services.Interfaces;
using CloudLogic.Services.Shop;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.CloudCode.Core;

namespace CloudLogic.Modules
{
    public class SkinsModule
    {
        private readonly ILogger _logger;

        public SkinsModule(ILogger<SkinsModule> logger)
        {
            _logger = logger;
        }

        [CloudCodeFunction(nameof(GetPlayerSkins))]
        public async Task<PlayerSkins> GetPlayerSkins(IExecutionContext ctx)
        {
            return await CloudDi.WithScope(_logger, ctx, sp =>
                sp.GetRequiredService<IPlayerSkinService>()
                  .GetPlayerSkins(ctx.PlayerId));
        }

        [CloudCodeFunction(nameof(GetPlayerOwnedSkins))]
        public async Task<List<Skin>> GetPlayerOwnedSkins(IExecutionContext ctx)
        {
            return await CloudDi.WithScope(_logger, ctx, sp =>
                sp.GetRequiredService<IPlayerSkinService>()
                  .GetPlayerOwnedSkins(ctx.PlayerId));
        }

        [CloudCodeFunction(nameof(CreateSkinPurchase))]
        public async Task<Skin> CreateSkinPurchase(IExecutionContext ctx, string skinId, string skinName, ItemType skinItemType, string shopPurchaseRef)
        {
            return await CloudDi.WithScope(_logger, ctx, sp =>
                sp.GetRequiredService<IPlayerSkinService>()
                  .CreateSkinPurchase(ctx.PlayerId, skinId, skinName, skinItemType, shopPurchaseRef));
        }

        [CloudCodeFunction(nameof(EquipSkin))]
        public async Task<PlayerSkins> EquipSkin(IExecutionContext ctx, string slotName, string skinId)
        {
            return await CloudDi.WithScope(_logger, ctx, sp =>
                sp.GetRequiredService<IPlayerSkinService>()
                  .EquipSkin(ctx.PlayerId, slotName, skinId));
        }

        [CloudCodeFunction(nameof(EquipSkins))]
        public async Task<PlayerSkins> EquipSkins(IExecutionContext ctx, Dictionary<string, string> changes)
        {
            return await CloudDi.WithScope(_logger, ctx, sp =>
                sp.GetRequiredService<IPlayerSkinService>()
                  .EquipSkins(ctx.PlayerId, changes));
        }

        [CloudCodeFunction(nameof(GetSkinConfigs))]
        public async Task<List<SkinConfig>> GetSkinConfigs(IExecutionContext ctx)
        {
            return await CloudDi.WithScope(_logger, ctx, sp =>
                sp.GetRequiredService<SkinConfigsRepo>()
                  .GetSkinConfigs());
        }

        [CloudCodeFunction(nameof(BuySkin))]
        public async Task<ShopResult> BuySkin(IExecutionContext ctx, string skinId)
        {
            return await CloudDi.WithScope(_logger, ctx, sp =>
                sp.GetRequiredService<ShopService>()
                  .BuySkinItem(ctx.PlayerId, skinId));
        }
    }
}
