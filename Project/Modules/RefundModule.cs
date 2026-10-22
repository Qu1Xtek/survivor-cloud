using System.Threading.Tasks;
using CloudLogic.DI;
using CloudLogic.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;

public class RefundModule
{
    static string _cachedBridgeToken;

    readonly ILogger<RefundModule> _logger;
    readonly IGameApiClient _gameApiClient;

    public RefundModule(ILogger<RefundModule> logger)
    {
        _logger = logger;
        _gameApiClient = GameApiClient.Create();
    }

    [CloudCodeFunction("HandleRefund")]
    public async Task<bool> HandleRefund(
        IExecutionContext ctx, string purchaseToken, string bridgeToken)
    {
        if (!await ValidateBridgeToken(ctx, bridgeToken, "HandleRefund"))
        {
            return false;
        }

        return await CloudDi.WithScope(_logger, ctx, sp =>
            sp.GetRequiredService<IRefundService>().RefundByToken(ctx, purchaseToken));
    }

    [CloudCodeFunction("HandleRefundByOrderId")]
    public async Task<bool> HandleRefundByOrderId(
        IExecutionContext ctx, string orderId, string bridgeToken)
    {
        if (!await ValidateBridgeToken(ctx, bridgeToken, "HandleRefundByOrderId"))
        {
            return false;
        }

        return await CloudDi.WithScope(_logger, ctx, sp =>
        sp.GetRequiredService<IRefundService>().RefundByOrderId(ctx, orderId));
    }

    [CloudCodeFunction("HandleRefundByTransactionId")]
    public async Task<bool> HandleRefundByTransactionId(
        IExecutionContext ctx, string transactionId, string bridgeToken)
    {
        if (!await ValidateBridgeToken(ctx, bridgeToken, "HandleRefundByTransactionId"))
        {
            return false;
        }
        return await CloudDi.WithScope(_logger, ctx, sp =>
            sp.GetRequiredService<IRefundService>().RefundByTransactionId(ctx, transactionId));
    }

    async Task<bool> ValidateBridgeToken(IExecutionContext ctx, string bridgeToken, string fn)
    {
        if (_cachedBridgeToken == null)
        {
            _cachedBridgeToken = (await _gameApiClient.SecretManager.GetSecret(ctx, "REFUND_SERVICE_TOKEN")).Value;
        }

        if (_cachedBridgeToken != bridgeToken)
        {
            _logger.LogWarning("{fn} rejected: invalid bridge token", fn);
            return false;
        }

        return true;
    }
}
