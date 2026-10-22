using CloudLogic.DI;
using CloudLogic.RepoModels.Items;
using CloudLogic.RepoModels.Player;
using CloudLogic.Services.Items;
using CloudLogic.Services.Reward;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.CloudCode.Core;

namespace CloudLogic.Modules
{
    public class AdModule
    {
        private readonly ILogger _logger;

        public AdModule(ILogger<AdModule> logger)
        {
            _logger = logger;
        }

        [CloudCodeFunction(nameof(AddBlueGemsFromAd))]
        public async Task<BaseResponse> AddBlueGemsFromAd(IExecutionContext ctx)
        {
            return await CloudDi.WithScope(_logger, ctx, sp =>
                sp.GetRequiredService<AdService>().AddBlueGemsFromAd(ctx.PlayerId));
        }

    }
}
