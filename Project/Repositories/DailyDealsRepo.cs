using CloudLogic.RepoModels.Shop;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace CloudLogic.Repositories
{
    public class DailyDealsRepo : BaseFireStoreService
    {
        public DailyDealsRepo(ILogger logger) : base("DailyDeals", logger)
        {
        }

        public Task<DailyDeals> GetByDay(string day)
        {
            return GetRecordById<DailyDeals>(day);
        }

        public Task Save(DailyDeals deals)
        {
            return UpsertRecord(deals);
        }
    }
}
