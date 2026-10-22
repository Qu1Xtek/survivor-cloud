using CloudLogic.RepoModels;
using CloudLogic.RepoModels.Enemy;
using CloudLogic.RepoModels.Player;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudLogic.Repositories
{
    public class VersionSettingRepo : BaseFireStoreService
    {
        public VersionSettingRepo(ILogger logger) : base("VersionSetting", logger)
        {
        }

        public async Task<VersionSetting> GetVersionSetting()
        {
            var result = await GetRecords<VersionSetting>();
            return result[0];
        }

        public async Task<List<string>> GetTesterAccounts()
        {
            var setting = await GetVersionSetting();
            return setting.TesterAccounts ?? new List<string>();
        }
    }
}
