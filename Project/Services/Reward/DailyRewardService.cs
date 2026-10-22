using CloudLogic.RepoModels.Player;
using CloudLogic.RepoModels.PlayerRewards;
using CloudLogic.Repositories;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Unity.Services.CloudCode.Core;
using CloudLogic.Response;
using CloudLogic.Util.Map;

namespace CloudLogic.Services.Reward
{
    public class DailyRewardService
    {

        private IDailyLoginWeekService _dailyLoginWeekService;
        private IRewardService _rewardService;
        private readonly PlayerDataRepo _playerDataService;
        private readonly ILogger _logger;

        public DailyRewardService(IDailyLoginWeekService dailyLoginWeekService, PlayerDataRepo playerDataRepo, IRewardService rewardService, ILogger logger)
        {
            _dailyLoginWeekService = dailyLoginWeekService;
            _playerDataService = playerDataRepo;
            _rewardService = rewardService;
            _logger = logger;
        }

        public async Task<DailyRewardClaimResult> ClaimDailyReward(string playerId)
        {
            var player = await _playerDataService.GetPlayerDataByPlayerId(playerId);

            var result = await ClaimRewardForPlayer(player);

            if (result.Success)
            {
                var newPlrData = new PlayerData()
                {
                    Id = player.Id,
                    LoginRewardInfo = new LoginRewardInfo()
                    {
                        ConcecutiveLoginCount = result.PlayerData.LoginRewardInfo.ConcecutiveLoginCount,
                        LastClaimDate = result.PlayerData.LoginRewardInfo.LastClaimDate,
                        Week = player.LoginRewardInfo.Week,
                        Year = player.LoginRewardInfo.Year
                    }
                };

                var data = await _playerDataService.UpsertPlayerData(newPlrData, player);                
            }

            return result;
        }

        public async Task<DailyRewardClaimResult> ClaimRewardForPlayer(PlayerData playerData)
        {
            LoginRewardInfo loginRewardInfo = playerData.LoginRewardInfo;

            if (loginRewardInfo != null && DateTime.UtcNow.Date == loginRewardInfo.LastClaimDate.ToDateTime().Date)
            {
                return new DailyRewardClaimResult
                {
                    Success = false,
                    PlayerData = playerData
                };
            }

            var quest = await GetCurrentDayQuest(loginRewardInfo.ConcecutiveLoginCount);


            var claimResult = await _rewardService.ClaimReward(playerData, quest.Reward);
            var dailyResult = new DailyRewardClaimResult
            {
                Success = claimResult.Success,
                PlayerData = playerData,
                Reward = claimResult
            };

            if (!claimResult.Success || playerData == null)
            {                
                return dailyResult;
            }

            dailyResult.PlayerData.LoginRewardInfo.ConcecutiveLoginCount++;
            dailyResult.PlayerData.LoginRewardInfo.LastClaimDate = Timestamp.FromDateTime(DateTime.UtcNow.Date);


            return dailyResult;
        }

        private async Task<Quest> GetCurrentDayQuest(int day)
        {

            var activeWeek = await _dailyLoginWeekService.GetActiveWeek();

            return activeWeek.Quests[day];
        }



    }
}
