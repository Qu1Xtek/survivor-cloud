using System.Threading.Tasks;
using CloudLogic.DI;
using CloudLogic.Response.IAP;
using CloudLogic.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Unity.Services.CloudCode.Core;

public class PurchaseModule
{
    readonly ILogger<PurchaseModule> _logger;

    public PurchaseModule(ILogger<PurchaseModule> logger)
    {
        _logger = logger;
    }

    [CloudCodeFunction("ValidatePurchase")]
    public async Task<PurchaseGrantResult> ValidatePurchase(
        IExecutionContext ctx,
        string store, string productId, string proof)
    {
        return await CloudDi.WithScope(_logger, ctx, sp =>
            sp.GetRequiredService<IPurchaseRecordService>()
              .ValidatePurchase(ctx, store, productId, proof));
    }
}


