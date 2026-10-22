using CloudLogic.RepoModels.Skin;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CloudLogic.Repositories
{
    public class SkinsRepo : BaseFireStoreService
    {
        public SkinsRepo(ILogger logger) : base("Skins", logger)
        {
        }

        public async Task<List<Skin>> GetByPlayerId(string playerId) =>
            await ReadRecordsByField<Skin>("PlayerId", playerId) ?? new List<Skin>();

        public async Task<Skin> Create(Skin purchase) =>
            await CreateRecord(purchase);
    }
}
