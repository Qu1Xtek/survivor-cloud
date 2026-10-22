using CloudLogic.DI;
using CloudLogic.RepoModels.Shop;
using CloudLogic.Repositories;
using CloudLogic.Response;
using CloudLogic.Response.DTO;
using CloudLogic.Services.Shop;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.CloudCode.Core;

namespace CloudLogic.Modules
{
    public class ShopModule
    {
        private readonly ILogger _logger;

        public ShopModule(ILogger<ShopModule> logger)
        {
            _logger = logger;
        }

        [CloudCodeFunction(nameof(GetShopItems))]
        public Task<List<Shop>> GetShopItems(IExecutionContext ctx)
        {
            return CloudDi.WithScope(_logger, ctx, sp =>
                sp.GetRequiredService<ShopRepo>().GetAll());
        }

        [CloudCodeFunction(nameof(GetIapProducts))]
        public Task<List<IapProduct>> GetIapProducts(IExecutionContext ctx)
        {
            return CloudDi.WithScope(_logger, ctx, sp =>
                sp.GetRequiredService<IapProductRepo>().GetAll());
        }

        [CloudCodeFunction(nameof(GetShop))]
        public Task<List<ShopItemDTO>> GetShop(IExecutionContext ctx)
        {
            return CloudDi.WithScope(_logger, ctx, sp =>
                sp.GetRequiredService<ShopService>().GetShop(ctx.PlayerId));
        }

        [CloudCodeFunction(nameof(ClaimForgeCard))]
        public Task<ShopResult> ClaimForgeCard(IExecutionContext ctx)
        {
            return CloudDi.WithScope(_logger, ctx, sp =>
                sp.GetRequiredService<ShopService>().ClaimForgeCard(ctx.PlayerId));
        }

        [CloudCodeFunction(nameof(BuyShopItem))]
        public Task<ShopResult> BuyShopItem(IExecutionContext ctx, string shopId)
        {
            return CloudDi.WithScope(_logger, ctx, sp =>
                sp.GetRequiredService<ShopService>().BuyShopItem(ctx.PlayerId, shopId));
        }
    }
}

