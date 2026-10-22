using CloudLogic.RepoModels.Shop;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudLogic.Repositories
{
    public class IapProductRepo : BaseFireStoreService
    {
        public IapProductRepo(ILogger logger) : base("IapProducts", logger)
        {
        }

        /// <summary>All configured products, smallest pack first.</summary>
        public async Task<List<IapProduct>> GetAll()
        {
            var products = await GetRecords<IapProduct>() ?? new List<IapProduct>();

            return products.OrderBy(p => p.Amount).ToList();
        }

        public async Task<IapProduct> Upsert(IapProduct item)
        {
            // The base upsert skips empty values, so a cleared Tag has to be written explicitly.
            item.ExplicitValuesUpsert(new[] { nameof(IapProduct.Tag) });
            await UpsertRecord(item);

            return item;
        }

        public Task<bool> Delete(string id)
        {
            return DeleteRecordById(id);
        }
    }
}
