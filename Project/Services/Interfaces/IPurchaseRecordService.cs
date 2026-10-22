using CloudLogic.Response.IAP;
using System.Threading.Tasks;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;

namespace CloudLogic.Services.Interfaces
{
    public interface IPurchaseRecordService
    {
        Task<PurchaseGrantResult> ValidatePurchase(
            IExecutionContext ctx,
            string store, string productId, string proof);
    }
}
