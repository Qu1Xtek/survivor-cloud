using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CloudLogic.Repositories
{
    public class QuestsManagementRepo : AchievementRepo
    {
        public QuestsManagementRepo(ILogger logger) : base("DailyQuests", logger)
        {
        }

        public async Task SetActiveStates(List<string> activate, List<string> deactivate)
        {
            var batch = _firestoreDb.StartBatch();
            var col = GetCollectionRef();

            foreach (var id in activate)
            {
                batch.Update(col.Document(id), new Dictionary<string, object> { ["IsActive"] = true });
            }

            foreach (var id in deactivate)
            {
                batch.Update(col.Document(id), new Dictionary<string, object> { ["IsActive"] = false });
            }

            await batch.CommitAsync();
        }
    }
}
