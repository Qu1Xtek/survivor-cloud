using CloudLogic.DI;
using CloudLogic.RepoModels.Achievements;
using CloudLogic.Response.DTO;
using CloudLogic.Services.Achievements;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.CloudCode.Core;

namespace CloudLogic.Modules
{
    public class DailyQuestModule
    {
        private readonly ILogger _logger;

        public DailyQuestModule(ILogger<DailyQuestModule> logger)
        {
            _logger = logger;
        }

        [CloudCodeFunction(nameof(RefreshDailyQuests))]
        public Task RefreshDailyQuests(IExecutionContext ctx)
        {
            return CloudDi.WithScope(_logger, ctx, sp =>
                sp.GetRequiredService<PlayerQuestsService>().RefreshDailyQuests());
        }

        [CloudCodeFunction(nameof(GetDailyQuests))]
        public Task<List<Achievement>> GetDailyQuests(IExecutionContext ctx)
        {
            return CloudDi.WithScope(_logger, ctx, sp =>
                sp.GetRequiredService<PlayerQuestsService>().GetActiveQuests());
        }

        [CloudCodeFunction(nameof(GetDailyQuestProgress))]
        public Task<QuestsProgress> GetDailyQuestProgress(IExecutionContext ctx)
        {
            return CloudDi.WithScope(_logger, ctx, sp =>
                sp.GetRequiredService<PlayerQuestsService>().GetPlayerProgress(ctx.PlayerId));
        }

        [CloudCodeFunction(nameof(ClaimDailyQuestReward))]
        public async Task<RewardGrantDTO> ClaimDailyQuestReward(IExecutionContext ctx, string questKey)
        {
            var result = await CloudDi.WithScope(_logger, ctx, sp =>
                sp.GetRequiredService<PlayerQuestsService>()
                .ClaimQuestReward(ctx.PlayerId, questKey));

            return result;
        }

        [CloudCodeFunction(nameof(ClaimDailyQuestRewards))]
        public Task<BatchClaimResponse> ClaimDailyQuestRewards(IExecutionContext ctx, List<string> questKeys)
        {
            return CloudDi.WithScope(_logger, ctx, sp =>
                sp.GetRequiredService<PlayerQuestsService>()
                .ClaimQuestRewards(ctx.PlayerId, questKeys));
        }
    }
}
