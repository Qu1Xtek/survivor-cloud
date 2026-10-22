using CloudLogic.RepoModels.Duel;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudLogic.Repositories
{
    public class DuelerRepo : BaseFireStoreService
    {
        public DuelerRepo(ILogger logger) : base("Duelers", logger)
        {
        }

        public async Task<List<Dueler>> GetActive()
        {
            var all = await GetRecords<Dueler>() ?? new List<Dueler>();
            return all.Where(d => d.IsActive).OrderBy(d => d.Order).ToList();
        }

        public Task<Dueler> GetById(string id)
        {
            return GetRecordById<Dueler>(id);
        }

        public async Task<List<Dueler>> GetAll()
        {
            var all = await GetRecords<Dueler>() ?? new List<Dueler>();
            return all.OrderBy(d => d.Order).ToList();
        }

        public Task<Dueler> Save(Dueler dueler)
        {
            return string.IsNullOrWhiteSpace(dueler.Id) ? CreateRecord(dueler) : UpdateRecord(dueler);
        }

        public Task<bool> Delete(string id)
        {
            return DeleteRecordById(id);
        }
    }
}
