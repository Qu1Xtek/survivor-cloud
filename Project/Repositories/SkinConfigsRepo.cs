using CloudLogic.Cache;
using CloudLogic.RepoModels.Skin;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CloudLogic.Repositories
{
    public class SkinConfigsRepo : BaseFireStoreService
    {
        private readonly ICloudCache _cache;

        public SkinConfigsRepo(ILogger logger, ICloudCache cache) : base("SkinConfigs", logger)
        {
            _cache = cache;
        }

        public virtual async Task<List<SkinConfig>> GetSkinConfigs()
        {
            var cached = await _cache.GetSkinConfigs();
            if (cached != null)
            {
                return cached;
            }

            var configs = await GetRecords<SkinConfig>();
            if (configs != null)
            {
                await _cache.SetSkinConfigs(configs);
            }

            return configs ?? new List<SkinConfig>();
        }

        public async Task<SkinConfig> Create(SkinConfig config)
        {
            config.Id = null;
            var created = await CreateRecord(config);
            await _cache.SetSkinConfigs(null);
            return created;
        }
    }
}
