using CloudLogic.DI;
using CloudLogic.RepoModels.Player;
using CloudLogic.Repositories;
using CloudLogic.Response;
using CloudLogic.Response.DTO;
using CloudLogic.Services.Items;
using CloudLogic.Services.Models;
using CloudLogic.Services.Player;
using CloudLogic.Services.Player.RunManagement;
using CloudLogic.Util;
using Google.Cloud.Firestore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;

namespace CloudLogic.Modules
{
    public class RunModule
    {
        private readonly ILogger _logger;
        private PushClient _pushClient;


        public RunModule(ILogger<RunModule> logger)
        {
            _logger = logger;
            _pushClient = PushClient.Create();
        }

        [CloudCodeFunction(nameof(StartRun))]
        public async Task<StartRunResponse> StartRun(IExecutionContext ctx, int selectedLevel)
        {
            return await CloudDi.WithScope(_logger, ctx, async sp =>
            {
                var runService = sp.GetRequiredService<IRunService>();
                var playerInfoRepo = sp.GetRequiredService<PlayerInfoRepo>();

                var runStarted = await runService.StartRun(ctx.PlayerId, selectedLevel);
                if (!runStarted.Success)
                {
                    return runStarted;
                }

                await playerInfoRepo.LogRunStart(ctx.PlayerId);
                return runStarted;
            });
        }

        [CloudCodeFunction(nameof(CompleteRun))]
        public async Task<BaseResponse> CompleteRun(IExecutionContext ctx)
        {
            return await CloudDi.WithScope(_logger, ctx, async sp =>
            {
                var runCompleter = sp.GetRequiredService<RunCompleter>();

                return await runCompleter.CompleteRun(ctx.PlayerId);
            });
        }

        [CloudCodeFunction(nameof(UseRevive))]
        public Task<ShopResult> UseRevive(IExecutionContext ctx)
        {
            return CloudDi.WithScope(_logger, ctx, sp =>
                sp.GetRequiredService<ReviveService>().UseRevive(ctx.PlayerId));
        }

        [CloudCodeFunction(nameof(DoubleRunRewards))]
        public async Task<BaseResponse> DoubleRunRewards(IExecutionContext ctx, int craftPickedCount, int craftDroppedCount)
        {
            return await CloudDi.WithScope(_logger, ctx, async sp =>
            {
                var util = new ResourceUtil();
                var playerManager = sp.GetRequiredService<IPlayerManagerService>();
                var repo = sp.GetRequiredService<PlayerDataRepo>();

                await repo.RunOurTransactionAsync(async tran =>
                {
                    var playerData = await playerManager.GetPlayerDataAsync(ctx.PlayerId, null, tran);

                    // Rewards for this run were already granted (normal or ad-double).
                    if (playerData.RunData.RunClaimed)
                    {
                        return;
                    }

                    var userCraftAmount = craftPickedCount + craftDroppedCount;
                    var craftTotal = Math.Min(userCraftAmount, 2);

                    var craftingAmount = craftTotal * 2;
                    var goldAmount = playerData.RunData.GoldPicked.Sum() + playerData.RunData.GoldDropped.Sum() * 2;

                    playerData.RunData.RunClaimed = true;

                    var newPlrData = new PlayerData()
                    {
                        Id = playerData.Id,
                        CraftingMaterials = util.AddCraftingMaterials(playerData, craftingAmount).CraftingMaterials,
                        Gold = util.AddGold(playerData, goldAmount).Gold,
                        RunData = playerData.RunData
                    };

                    await playerManager.UpsertPlayerData(newPlrData, playerData, tran);
                });

                return new BaseResponse();
            });
        }

        [CloudCodeFunction(nameof(R))]
        public async Task<RunAnalysis> R(IExecutionContext ctx, RunAnalysis analys)
        {
            return await CloudDi.WithScope(_logger, ctx, async sp =>
            {
                if (analys.RunEffectiveTime < 60)
                {
                    return analys;
                }

                var repo = sp.GetRequiredService<RunAnalysisRepo>();
                analys.PlayerId = ctx.PlayerId;
                analys.DateCreatedStamp = Timestamp.FromDateTime(DateTime.UtcNow);

                return await repo.Save(analys);
            });
        }
    }
}
