using CloudLogic.RepoModels.Gem;
using CloudLogic.RepoModels.Player;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace CloudLogic.Repositories
{
    public class GemConfigsRepo : BaseFireStoreService
    {
        public GemConfigsRepo(ILogger logger) : base("GemConfigs", logger)
        {
        }

        public virtual async Task<GemConfigs> GetGemConfigs()
        {
            var result = await GetRecordById<GemConfigs>("1");

            return result;
        }
    }
}
