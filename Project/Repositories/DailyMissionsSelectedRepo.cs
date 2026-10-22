using CloudLogic.RepoModels;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CloudLogic.Repositories
{
    public class DailyMissionsSelectedRepo : BaseFireStoreService
    {
        public DailyMissionsSelectedRepo(ILogger logger) : base("DailyMissionSelections", logger)
        {
        }
        public async Task Create(DailyMissionSelected entry)
        {
            await CreateRecord(entry);
        }
    }
}
