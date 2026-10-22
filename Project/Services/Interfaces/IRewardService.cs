using CloudLogic.RepoModels.Player;
using CloudLogic.RepoModels.PlayerRewards;
using CloudLogic.Response;
using CloudLogic.Response.DTO;
using System.Threading.Tasks;
using RewardModel = CloudLogic.RepoModels.PlayerRewards.Reward;

namespace CloudLogic.Services.Reward
{
    public interface IRewardService
    {
        Task<RewardGrantDTO> ClaimReward(PlayerData playerData, RewardModel reward);
    }
}
