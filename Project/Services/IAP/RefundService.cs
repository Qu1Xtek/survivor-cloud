using CloudLogic.Repositories;
using CloudLogic.RepoModels.Purchase;
using CloudLogic.Services.Interfaces;
using CloudLogic.Services.Player;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Unity.Services.CloudCode.Core;

namespace CloudLogic.Services.IAP
{
    public class RefundService : IRefundService
    {
        readonly ILogger _logger;
        readonly PurchaseRecordRepo _repo;
        readonly ResourceService _resources;

        public RefundService(ILogger logger, PurchaseRecordRepo repo, ResourceService resources)
        {
            _logger = logger;
            _repo = repo;
            _resources = resources;
        }

        public async Task<bool> RefundByToken(IExecutionContext ctx, string purchaseToken)
            => await Process(await _repo.GetByToken(purchaseToken));

        public async Task<bool> RefundByOrderId(IExecutionContext ctx, string orderId)
            => await Process(await _repo.GetByOrderId(orderId));

        public async Task<bool> RefundByTransactionId(IExecutionContext ctx, string transactionId)
            => await Process(await _repo.GetByToken(transactionId));

        async Task<bool> Process(PurchaseRecord record)
        {
            if (record == null)
            {
                _logger.LogWarning("RefundService: no record found");
                return false;
            }

            if (record.Refunded)
            {
                _logger.LogWarning("RefundService: already refunded for player {player}", record.PlayerId);
                return false;
            }

            await _resources.SubtractBlueGems(record.PlayerId, record.GemsGranted);
            await _repo.MarkRefunded(record.Id);

            _logger.LogInformation("refund processed: {gems} gems deducted from player {player} for {product}",
                record.GemsGranted, record.PlayerId, record.ProductId);
            return true;
        }
    }
}
