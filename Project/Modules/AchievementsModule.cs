using CloudLogic.DI;
using CloudLogic.RepoModels.Achievements;
using CloudLogic.Response;
using CloudLogic.Response.DTO;
using CloudLogic.Services.Achievements;
using CloudLogic.Util;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.CloudCode.Core;

namespace CloudLogic.Modules
{
    public class AchievementsModule
    {
        private readonly ILogger _logger;

        public AchievementsModule(ILogger<AchievementsModule> logger)
        {
            _logger = logger;
        }

        [CloudCodeFunction(nameof(GetAchievements))]
        public Task<List<Achievement>> GetAchievements(IExecutionContext ctx)
        {
            return CloudDi.WithScope(_logger, ctx, sp =>
                sp.GetRequiredService<AchievementsService>().GetActiveAchievements());
        }

        [CloudCodeFunction(nameof(GetAchievementProgress))]
        public Task<AchievementProgress> GetAchievementProgress(IExecutionContext ctx)
        {
            return CloudDi.WithScope(_logger, ctx, sp =>
                sp.GetRequiredService<AchievementsService>().GetPlayerProgressDoc(ctx.PlayerId));
        }

        [CloudCodeFunction(nameof(ClaimAchievementReward))]
        public async Task<RewardGrantDTO> ClaimAchievementReward(IExecutionContext ctx, string achievementKey)
        {
            var result = await CloudDi.WithScope(_logger, ctx, sp =>
                sp.GetRequiredService<AchievementsService>()
                .ClaimAchievementReward(ctx.PlayerId, achievementKey));

            return result;
        }

        [CloudCodeFunction(nameof(ClaimAchievementRewards))]
        public Task<BatchClaimResponse> ClaimAchievementRewards(IExecutionContext ctx, List<string> achievementKeys)
        {
            return CloudDi.WithScope(_logger, ctx, sp =>
                sp.GetRequiredService<AchievementsService>()
                .ClaimAchievementRewards(ctx.PlayerId, achievementKeys));
        }
    }
}

