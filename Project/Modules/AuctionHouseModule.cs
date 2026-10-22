using CloudLogic.RepoModels.AuctionHouse;
using CloudLogic.Repositories;
using CloudLogic.Services.CloudSave;
using CloudLogic.Services.Trading;
using CloudLogic.Cache;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using CloudLogic.DI;
using CloudLogic.Response.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.CloudCode.Core;
using CloudLogic.RepoModels.Items;

namespace CloudLogic.Modules;

public class AuctionHouseModule
{
    private readonly ILogger _logger;

    public AuctionHouseModule(ILogger<AuctionHouseModule> logger)
    {
        _logger = logger;
    }

    [CloudCodeFunction(nameof(GetAuctions))]
    public async Task<List<AuctionDTO>> GetAuctions(IExecutionContext ctx)
    {
        return await CloudDi.WithScope(_logger, ctx, async sp =>
        {
            var cache = sp.GetRequiredService<ICloudCache>();
            return AuctionList.For(await cache.GetAuctionHouseItems(), ctx.PlayerId);
        });
    }

    [CloudCodeFunction("PostAHItem")]
    public async Task<string> PostAHItem(IExecutionContext ctx, AHItem ahItem)
    {
        return await CloudDi.WithScope(_logger, ctx, sp =>
            sp.GetRequiredService<IAuctionHouseService>().PostAHItem(ctx.PlayerId, ahItem));
    }

    [CloudCodeFunction(nameof(BuyAuctions))]
    public async Task<Item> BuyAuctions(IExecutionContext ctx, string auctionId)
    {        
        return await CloudDi.WithScope(_logger, ctx, sp =>
            sp.GetRequiredService<IAuctionHouseService>().BuyAHItem(ctx.PlayerId, auctionId));
    }

    [CloudCodeFunction(nameof(CancelAuction))]
    public async Task<string> CancelAuction(IExecutionContext ctx, string auctionId)
    {
        return await CloudDi.WithScope(_logger, ctx, async sp =>
        {
            var auctionService = sp.GetRequiredService<IAuctionHouseService>();
            var playerDataRepo = sp.GetRequiredService<PlayerDataRepo>();

            var result = await auctionService.CancelAuction(ctx.PlayerId, auctionId);
            await playerDataRepo.RemoveAuction(ctx.PlayerId);
            return result;
        });
    }
}


