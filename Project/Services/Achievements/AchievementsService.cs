using CloudLogic.RepoModels;
using CloudLogic.RepoModels.Achievements;
using CloudLogic.RepoModels.Items;
using CloudLogic.RepoModels.Player;
using CloudLogic.Repositories;
using CloudLogic.Response;
using CloudLogic.Response.DTO;
using CloudLogic.Services.Items;
using CloudLogic.Services.Models;
using CloudLogic.Services.Reward;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using RewardModel = CloudLogic.RepoModels.PlayerRewards.Reward;

namespace CloudLogic.Services.Achievements
{
    public class AchievementsService
    {
        private readonly AchievementRepo _achievementRepo;
        private readonly AchievementProgressRepo _progressRepo;
        private readonly QuestProgressRepo _questProgress;
        private readonly IRewardService _rewardService;
        private readonly PlayerDataRepo _playerManagerRepo;
        private readonly ILogger _logger;

        public AchievementsService(
            AchievementRepo achievementRepo,
            AchievementProgressRepo progressRepo,
            QuestProgressRepo questProgress,
            IRewardService rewardService,
            PlayerDataRepo playerManagerRepo,
            ILogger logger)
        {
            _achievementRepo = achievementRepo;
            _progressRepo = progressRepo;
            _questProgress = questProgress;
            _rewardService = rewardService;
            _playerManagerRepo = playerManagerRepo;
            _logger = logger;
        }

        public Task<List<Achievement>> GetActiveAchievements()
        {
            return _achievementRepo.GetAllActive();
        }

        public Task<List<AchievementProgress>> GetPlayerProgress(string playerId)
        {
            return _progressRepo.GetAllForPlayer(playerId);
        }

        public Task<AchievementProgress> GetPlayerProgressDoc(string playerId)
        {
            return _progressRepo.GetPlayerProgressDoc(playerId);
        }

        public async Task<AchiAndQuest> Increment(string playerId, string achievementKey, long delta)
        {
            var result = new AchiAndQuest();

            result.AchievementProgress = await _progressRepo.IncrementStatAtomic(playerId, achievementKey, delta);
            result.QuestsProgress = await _questProgress.IncrementStatAtomic(playerId, achievementKey, delta);

            return result;
        }


        /// <summary>
        /// Transactions are being ran here!!!
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="changes"></param>
        /// <returns></returns>
        public async Task<ProgressDict> IncrementMultiple(string playerId, Dictionary<string, long> changes)
        { 
            var progress = new ProgressDict();

            progress.AchievementProgressD = await _progressRepo.IncrementStatsAtomic(playerId, changes);
            progress.QuestsProgressD = await _questProgress.IncrementStatsAtomic(playerId, changes);

            return progress;
        }

        /// <summary>
        /// We are running transactions here !
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="achievementKey"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task<AchiAndQuest> Set(string playerId, string achievementKey, long value)
        {
            var result = new AchiAndQuest();

            var dict = new Dictionary<string, long> { { achievementKey, value } };

            result.AchievementProgress = await _progressRepo.SetStatsAtomic(playerId, dict);
            result.QuestsProgress = await _questProgress.SetStatsAtomic(playerId, dict);

            return result;
        }

        public async Task<Dictionary<string,long>> SetIfMore(string playerId, string achievementKey, long value)
        {
            return await _progressRepo.SetStatIfMoreAtomic(playerId, achievementKey, value);
        }


        bool IsNotReady(AchievementProgress prog, Achievement ach)
        {
            var current = prog.Stats.ContainsKey(ach.Type.ToString()) ? prog.Stats[ach.Type.ToString()] : 0;
            return current < ach.TargetValue;
        }


        public async Task<RewardGrantDTO> ClaimAchievementReward(string playerId, string achievementKey)
        {
            var ach = await _achievementRepo.GetByKey(achievementKey);

            if (ach == null || !ach.IsActive || ach.RewardValue <= 0)
            {
                return null;
            }                

            var progress = await _progressRepo.GetPlayerProgressDoc(playerId);

            if (progress.CompletedAchievements.ContainsKey(achievementKey) || IsNotReady(progress, ach))
            {
                return null;
            }

            await _progressRepo.MarkAchievementCompleted(playerId, achievementKey);
            
            var playerData = await _playerManagerRepo.GetPlayerDataByPlayerId(playerId);            

            return await _rewardService.ClaimReward(playerData, ach.Reward);
        }

        /// <summary>
        /// "Claim all": marks every ready, unclaimed achievement in one transaction, then grants their rewards in order.
        /// </summary>
        public async Task<BatchClaimResponse> ClaimAchievementRewards(string playerId, List<string> achievementKeys)
        {
            var response = new BatchClaimResponse();
            var keys = achievementKeys ?? new List<string>();

            var claimable = (await _achievementRepo.GetAllActive())
                .Where(a => a.RewardValue > 0 && keys.Contains(a.Key))
                .GroupBy(a => a.Key)
                .ToDictionary(g => g.Key, g => g.First());

            var marked = await _progressRepo.MarkAchievementsCompleted(playerId, claimable.Keys,
                (progress, key) => !IsNotReady(progress, claimable[key]));

            if (marked.Count > 0)
            {
                var playerData = await _playerManagerRepo.GetPlayerDataByPlayerId(playerId);

                foreach (var key in marked)
                {
                    response.Claimed.Add(await Grant(playerData, key, claimable[key].Reward));
                }
            }

            response.Rejected = keys.Except(marked).Distinct().ToList();
            response.Success = response.Claimed.Count > 0;

            return response;
        }

        private async Task<QuestClaimResult> Grant(PlayerData playerData, string key, RewardModel reward)
        {
            try
            {
                return new QuestClaimResult { Key = key, Reward = await _rewardService.ClaimReward(playerData, reward) };
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"Achievement {key} was marked claimed but its reward failed for player {playerData?.Id}");
                return new QuestClaimResult { Key = key, Reward = new RewardGrantDTO { Success = false, AdditionalInfo = ex.Message } };
            }
        }

        public async Task<ProgressDict> IncrementCraft(string playerId, RarityType craftedRarity)
        {
            var statKey = craftedRarity switch
            {
                RarityType.Uncommon => AchievementType.UncommonCrafted.ToString(),
                RarityType.Rare => AchievementType.RareCrafted.ToString(),
                RarityType.Epic => AchievementType.EpicCrafted.ToString(),
                _ => null
            };

            if (string.IsNullOrWhiteSpace(statKey))
                return null;

            var result = await Increment(playerId, statKey, 1);

            var progress = new ProgressDict();

            progress.AchievementProgressD = new Dictionary<string, long>() { [statKey] = result.AchievementProgress.Stats[statKey] };
            progress.QuestsProgressD = new Dictionary<string, long>() { [statKey] = result.QuestsProgress.Stats[statKey] };

            return progress;
        }

        public async Task<ProgressDict> IncrementDrop(string playerId, RarityType craftedRarity)
        {
            var statKey = craftedRarity switch
            {
                RarityType.Uncommon => AchievementType.UncommonDropped.ToString(),
                RarityType.Rare => AchievementType.RareDropped.ToString(),
                RarityType.Epic => AchievementType.EpicsDropped.ToString(),
                RarityType.Legendary => AchievementType.LegendariesDropped.ToString(),
                _ => null
            };

            if (string.IsNullOrWhiteSpace(statKey))
                return new ProgressDict();

            var stats = new Dictionary<string, long>
            {
                {statKey, 1 },
                {AchievementType.ItemsDropped.ToString(), 1 }
            };

            var result = new ProgressDict();    

            result.AchievementProgressD = await _progressRepo.IncrementStatsAtomic(playerId, stats);
            result.QuestsProgressD = await _questProgress.IncrementStatsAtomic(playerId, stats);

            return result;
        }
    }
}

