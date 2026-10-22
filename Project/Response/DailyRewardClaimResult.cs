using CloudLogic.RepoModels.Player;
using CloudLogic.Response.DTO;
using System.Collections.Generic;

namespace CloudLogic.Response
{
    public class DailyRewardClaimResult
    {
        public bool Success { get; set; }

        public string RewardId { get; set; }

        public RewardGrantDTO Reward { get; set; }

        public PlayerData PlayerData { get; set; }
    }
}


