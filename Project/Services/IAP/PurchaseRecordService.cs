using CloudLogic.RepoModels.Purchase;
using CloudLogic.Repositories;
using CloudLogic.Response.IAP;
using CloudLogic.Services.Interfaces;
using CloudLogic.Services.Player;
using CloudLogic.Services.Reward;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;

namespace CloudLogic.Services.IAP
{
    public class PurchaseRecordService : IPurchaseRecordService
    {

        readonly ILogger _logger;
        readonly IGameApiClient _gameApiClient;
        readonly PurchaseRecordRepo _repo;
        readonly ResourceService _resources;
        private readonly IapProductRepo _productRepo;
        private readonly PlayerDataRepo _playerDataRepo;
        private readonly IRewardService _rewardService;

        public PurchaseRecordService(ILogger logger, PurchaseRecordRepo repo, ResourceService resources, IapProductRepo productRepo,
            PlayerDataRepo playerDataRepo, IRewardService rewardService)
        {
            _logger = logger;
            _gameApiClient = GameApiClient.Create();
            _repo = repo;
            _resources = resources;
            _productRepo = productRepo;
            _playerDataRepo = playerDataRepo;
            _rewardService = rewardService;
        }

        public async Task<PurchaseGrantResult> ValidatePurchase(
            IExecutionContext ctx,
            string store, string productId, string proof)
        {
            var products = await _productRepo.GetAll();
            var amount = 0;
            var product = products.Where(x => x.Id == productId).FirstOrDefault();

            if (product == null)
            {
                _logger.LogWarning("unknown product {productId}", productId);
                return PurchaseGrantResult.Failure(ctx.CorrelationId);
            }

            amount = product.Amount;
            string orderId = await VerifyWithStore(ctx, store, productId, proof);

            if (orderId == null)
            {
                _logger.LogWarning("verification failed for {productId}", productId);
                return PurchaseGrantResult.Failure(ctx.CorrelationId);
            }

            try
            {
                var created = await _repo.TryCreate(proof, new PurchaseRecord
                {
                    PlayerId = ctx.PlayerId,
                    ProductId = productId,
                    Store = store,
                    OrderId = orderId,
                    GemsGranted = amount,
                    GrantedAt = DateTime.UtcNow,
                });

                if (!created)
                {
                    _logger.LogWarning("replay detected for {productId} token={proof}",
                        productId, proof[..Math.Min(20, proof.Length)]);
                    return PurchaseGrantResult.Failure(ctx.CorrelationId);
                }
            }
            catch (Exception e)
            {
                _logger.LogError("replay guard failed for {productId}: {err}", productId, e.Message);
                return PurchaseGrantResult.Failure(ctx.CorrelationId);
            }

            try
            {
                if (amount > 0)
                {
                    var newBalance = await _resources.AddBlueGems(ctx.PlayerId, amount);
                    _logger.LogInformation("granted {amount} blue_gems for {productId}, new balance {balance}",
                        amount, productId, newBalance);
                }

                var result = PurchaseGrantResult.Success(ctx.CorrelationId, amount);

                foreach (var reward in product.Rewards ?? new List<RepoModels.PlayerRewards.Reward>())
                {
                    var player = await _playerDataRepo.GetPlayerDataByPlayerId(ctx.PlayerId);
                    result.Rewards.Add(await _rewardService.ClaimReward(player, reward));
                }

                return result;
            }
            catch (Exception e)
            {
                _logger.LogError("grant failed for {productId}: {err}", productId, e.Message);
                return PurchaseGrantResult.Failure(ctx.CorrelationId);
            }
        }

        async Task<string> VerifyWithStore(
            IExecutionContext ctx, string store, string productId, string proof)
        {
            if (store == "GooglePlay")
            {
                return await GoogleReceiptValidator.Verify(_gameApiClient, ctx, productId, proof, _logger);
            }

            if (store == "AppleAppStore")
            {
                return await AppleReceiptValidator.Verify(_gameApiClient, ctx, proof, productId, _logger);
            }

            return null;
        }
    }
}
