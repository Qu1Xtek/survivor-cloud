using CloudLogic.DI;
using CloudLogic.RepoModels;
using CloudLogic.RepoModels.PlayerRewards;
using CloudLogic.Response.DTO;
using CloudLogic.Services.Player;
using CloudLogic.Services.Reward;
using CloudLogic.Util;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Unity.Services.CloudCode.Core;

namespace CloudLogic.Modules
{
    public class RewarModule
    {
        private readonly ILogger _logger;

        public RewarModule(ILogger<RewarModule> logger)
        {
            _logger = logger;
        }

        [CloudCodeFunction(nameof(ClaimDailyReward))]
        public async Task<DailyRewardGrantDTO> ClaimDailyReward(IExecutionContext ctx)
        {
            var result = await CloudDi.WithScope(_logger, ctx, sp =>
                sp.GetRequiredService<DailyRewardService>().ClaimDailyReward(ctx.PlayerId));

            RewardType? type = result.Reward.Type == RepoModels.PlayerRewards.RewardType.DailyLoginChest
                ? RepoModels.PlayerRewards.RewardType.Chest
                : result?.Reward?.Type;

            return new DailyRewardGrantDTO()
            {
                Success = result?.Success ?? false,
                Type = type ?? RewardType.Gold,
                RewardObj = result?.Reward?.RewardObj,
                RewardVal = result.Reward.RewardVal,
                ConcecutiveLoginCount = result.PlayerData?.LoginRewardInfo?.ConcecutiveLoginCount ?? -1,
                LastClaimDate = result?.PlayerData?.LoginRewardInfo?.LastClaimDate ?? default
            };
        }


        [CloudCodeFunction(nameof(GetActiveWeek))]
        public async Task<DailyLoginWeek> GetActiveWeek(IExecutionContext ctx)
        {
            var result = await CloudDi.WithScope(_logger, ctx, sp =>
                sp.GetRequiredService<IDailyLoginWeekService>().GetActiveWeek());

            foreach (var item in result.Quests)
            {
                item.Reward.RewardType =
                    item.Reward.RewardType == RepoModels.PlayerRewards.RewardType.DailyLoginChest
                    ? RepoModels.PlayerRewards.RewardType.Chest
                    : item.Reward.RewardType;
            }

            return result;
        }

        [CloudCodeFunction(nameof(GetPigGold))]
        public async Task<BaseResponse> GetPigGold(IExecutionContext ctx)
        {
            return await CloudDi.WithScope(_logger, ctx, sp =>
                sp.GetRequiredService<ResourceService>().GetGoldFromPig(ctx.PlayerId));
        }
    }
}
