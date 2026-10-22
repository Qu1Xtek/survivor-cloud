using CloudLogic.RepoModels.Enemy;
using CloudLogic.RepoModels.Items;
using CloudLogic.Repositories;
using CloudLogic.Services;
using CloudLogic.Services.Items;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using CloudLogic.DI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Unity.Services.CloudCode.Core;

namespace CloudLogic.Modules
{
    public class WaveDetailsModule
    {
        private readonly ILogger _logger;

        public WaveDetailsModule(ILogger<WaveDetailsModule> logger)
        {
            _logger = logger;
        }

        [CloudCodeFunction(nameof(GetWaveDetails))]
        public async Task<string> GetWaveDetails(IExecutionContext ctx)
        { 
            return await CloudDi.WithScope(_logger, ctx, sp =>
                sp.GetRequiredService<WaveDetailsRepo>().GetWaveDetails());
        }


        [CloudCodeFunction(nameof(CreateWaveDetails))]
        public async Task<WaveDetails> CreateWaveDetails(IExecutionContext ctx, WaveDetails waveDetails)
        {
            return await CloudDi.WithScope(_logger, ctx, sp =>
                sp.GetRequiredService<WaveDetailsRepo>().CreateWaveDetails(waveDetails));
        }

    }
}
