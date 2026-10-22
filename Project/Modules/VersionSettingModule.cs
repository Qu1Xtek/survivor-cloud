using CloudLogic.RepoModels;
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
    public class VersionSettingModule
    {
        private readonly ILogger _logger;

        public VersionSettingModule(ILogger<VersionSettingModule> logger)
        {
            _logger = logger;
        }

        [CloudCodeFunction(nameof(GetVersionSetting))]
        public async Task<VersionSetting> GetVersionSetting(IExecutionContext ctx)
        {
            return await CloudDi.WithScope(_logger, ctx, sp =>
                sp.GetRequiredService<VersionSettingRepo>().GetVersionSetting());
        }

        [CloudCodeFunction(nameof(GetTesterAccounts))]
        public async Task<List<string>> GetTesterAccounts(IExecutionContext ctx)
        {
            return await CloudDi.WithScope(_logger, ctx, sp =>
                sp.GetRequiredService<VersionSettingRepo>().GetTesterAccounts());
        }
    }
}
