using Microsoft.Extensions.Logging;

namespace CloudLogic.Modules
{
    public class ItemTemplateModule
    {
        private readonly ILogger _logger;

        public ItemTemplateModule(ILogger<ItemTemplateModule> logger)
        {
            _logger = logger;
        }

        //[CloudCodeFunction(nameof(GetItemTemplates))]
        //public async Task<List<ItemTemplate>> GetItemTemplates()
        //{
        //    try
        //    {
        //        return await CloudDi.WithScope(_logger, sp =>
        //            sp.GetRequiredService<IItemTemplateService>().GetItemTemplates());
        //    }
        //    catch (Exception)
        //    {
        //        return new List<ItemTemplate>();
        //    }

        //}
    }
}
