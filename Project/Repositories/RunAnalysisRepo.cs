using CloudLogic.Services.Models;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace CloudLogic.Repositories
{
    public class RunAnalysisRepo : BaseFireStoreService
    {
        public RunAnalysisRepo(ILogger logger) : base("RunAnalysis", logger)
        {
        }

        public async Task<RunAnalysis> Save(RunAnalysis analysis)
        {
            await UpsertRecord(analysis);
            return analysis;
        }

        public async Task<RunAnalysis> Get(string documentId)
        {
            return await GetRecordById<RunAnalysis>(documentId);
        }
    }
}
