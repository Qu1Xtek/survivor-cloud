using System.Threading.Tasks;
using Unity.Services.CloudCode.Core;

namespace CloudLogic.Services.Interfaces
{
    public interface IRefundService
    {
        Task<bool> RefundByToken(IExecutionContext ctx, string purchaseToken);
        Task<bool> RefundByOrderId(IExecutionContext ctx, string orderId);
        Task<bool> RefundByTransactionId(IExecutionContext ctx, string transactionId);
    }
}
