using CloudLogic.DI;
using CloudLogic.RepoModels.Items;
using CloudLogic.RepoModels.Player;
using CloudLogic.Repositories;
using CloudLogic.Services;
using CloudLogic.Services.Items;
using CloudLogic.Services.Player;
using CloudLogic.Util;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Unity.Services.CloudCode.Core;

namespace CloudLogic.Modules
{
    public class ResourcesModule
    {
        private readonly ILogger _logger;

        public ResourcesModule(ILogger<ResourcesModule> logger)
        {
            _logger = logger;            
        }

        [CloudCodeFunction(nameof(AddBlueGems))]
        public async Task<BaseResponse> AddBlueGems(IExecutionContext ctx, int amount)
        {
            var result = await CloudDi.WithScope(_logger, ctx, sp =>
                sp.GetRequiredService<ResourceService>().AddBlueGems(ctx.PlayerId, amount));

            return new BaseResponse
            {
                PlayerData = new Response.PlayerDataDTO
                {
                    BlueGems = BindableInt.Of((int)result)
                }
            };
        }

        [CloudCodeFunction(nameof(AddEnergy))]
        public async Task<BaseResponse> AddEnergy(IExecutionContext ctx)
        {
            var result = await CloudDi.WithScope(_logger, ctx, sp =>
                sp.GetRequiredService<IEnergyService>().AddEnergy(ctx.PlayerId, 5));

            return new BaseResponse
            {
                Success = true,
                PlayerData = new Response.PlayerDataDTO
                {
                    Energy = BindableLong.Of(result)
                }
            };
        }

    }
}
