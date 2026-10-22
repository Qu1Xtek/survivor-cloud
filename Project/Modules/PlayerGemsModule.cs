using CloudLogic.RepoModels.Items;
using CloudLogic.RepoModels.Player;
using CloudLogic.Repositories;
using CloudLogic.Services.Items;
using CloudLogic.Services.Player;
using CloudLogic.DI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;
using System;
using CloudLogic.RepoModels.Gem;
using CloudLogic.Services.Interfaces;
using CloudLogic.Response;

namespace CloudLogic.Modules
{
    public class PlayerGemsModule
    {
        private readonly ILogger _logger;        

        public PlayerGemsModule(ILogger<PlayerGemsModule> logger)
        {
            _logger = logger;
        }

        [CloudCodeFunction(nameof(UpgradeGem))]
        public async Task<UpgradeGemResponse> UpgradeGem(IExecutionContext ctx, GemType gemType)
        {
            return await CloudDi.WithScope(_logger, ctx, sp =>
                sp.GetRequiredService<IPlayerGemService>()
                .UpgradeGem(ctx.PlayerId, gemType));
        }

        [CloudCodeFunction(nameof(GetGemConfigs))]
        public async Task<GemConfigs> GetGemConfigs(IExecutionContext ctx)
        {
            return await CloudDi.WithScope(_logger, ctx, sp =>
                sp.GetRequiredService<GemConfigsRepo>()
                .GetGemConfigs());
        }

        //[CloudCodeFunction(nameof(SelectGemMilestonePower))]
        //public async Task<PlayerGems> SelectGemMilestonePower(IExecutionContext ctx, GemType gemType, int level, int index)
        //{
        //    return await CloudDi.WithScope(_logger, ctx, sp =>
        //        sp.GetRequiredService<IPlayerGemService>()
        //        .SelectGemMilestonePower(ctx.PlayerId, gemType, level, index));
        //}
    }
}
