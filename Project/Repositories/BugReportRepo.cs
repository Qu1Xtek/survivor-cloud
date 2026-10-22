using CloudLogic.RepoModels;
using Microsoft.Extensions.Logging;

namespace CloudLogic.Repositories
{
    public class BugReportRepo : BaseFireStoreService
    {
        public BugReportRepo(ILogger logger) : base("BugReports", logger)
        {
        }

        public async System.Threading.Tasks.Task<BugReport> CreateBugReport(BugReport report)
        {
            return await StoreRecord(report);
        }
    }
}
