using CloudLogic.Cache;
using CloudLogic.RepoModels;
using CloudLogic.RepoModels.Achievements;
using CloudLogic.RepoModels.Items;
using CloudLogic.RepoModels.Player;
using CloudLogic.Repositories;
using System.Linq;
using RewardModel = CloudLogic.RepoModels.PlayerRewards.Reward;
using CloudLogic.Response;
using CloudLogic.Services.Reward;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudLogic.Response.DTO;

namespace CloudLogic.Services.Achievements
{
    public class PlayerQuestsService //: IPlayerQuestsService
    {
        private readonly QuestProgressRepo _playerProgressRepo;
        private readonly QuestsManagementRepo _questManageRepo;
        private readonly DailyMissionsSelectedRepo _dailyMissionsRepo;
        private readonly IRewardService _rewardService;
        private readonly PlayerDataRepo _playerManagerRepo;
        private readonly ICloudCache _cache;
        private readonly ILogger _logger;

        private const int MissionsPerDay = 3;

        public PlayerQuestsService(QuestProgressRepo progressRepo,
            IRewardService rewardService,
            QuestsManagementRepo questManageRepo,
            DailyMissionsSelectedRepo dailyMissionsRepo,
            PlayerDataRepo playerManagerRepo,
            ICloudCache cache,
            ILogger logger)
        {
            _playerProgressRepo = progressRepo;
            _rewardService = rewardService;
            _playerManagerRepo = playerManagerRepo;
            _dailyMissionsRepo = dailyMissionsRepo;
            _cache = cache;
            _logger = logger;
            _questManageRepo = questManageRepo;
        }

        public async Task<List<Achievement>> GetActiveQuests()
        {
            var cached = await _cache.GetDailyQuests();
            if (cached != null)
            {
                return cached;
            }

            var quests = await _questManageRepo.GetAllActive();
            await _cache.SetDailyQuests(quests);
            return quests;
        }

        public async Task RefreshDailyQuests()
        {
            var all = await _questManageRepo.GetAll();

            var currentlyActive = all.Where(q => q.IsActive).ToList();
            var inactive = all.Where(q => !q.IsActive).ToList();

            var pool = inactive.Count >= MissionsPerDay ? inactive : all;
            var next = pool.OrderBy(_ => Guid.NewGuid()).Take(MissionsPerDay).ToList();

            var activate = next.Select(q => q.Id).ToList();
            var deactivate = currentlyActive
                .Where(q => !activate.Contains(q.Id))
                .Select(q => q.Id)
                .ToList();

            await _questManageRepo.SetActiveStates(activate, deactivate);
            await _cache.SetDailyQuests(next);

            await _dailyMissionsRepo.Create(new DailyMissionSelected
            {
                Id = DateTime.UtcNow.ToString("dd-MM-yyyy"),
                MissionsSelected = activate
            });
        }

        public Task<QuestsProgress> GetPlayerProgress(string playerId)
        {
            return _playerProgressRepo.GetProgressForPlayer(playerId);
        }

        public async Task<QuestsProgress> Increment(string playerId, string questKey, long delta)
        {
            return await _playerProgressRepo.IncrementStatAtomic(playerId, questKey, delta);
        }

        public async Task<Dictionary<string, long>> IncrementMultiple(string playerId, Dictionary<string, long> changes)
        {
            return await _playerProgressRepo.IncrementStatsAtomic(playerId, changes);
        }

        public async Task<QuestsProgress> Set(string playerId, string questKey, long value)
        {
            var dict = new Dictionary<string, long> { { questKey, value } };

            return await _playerProgressRepo.SetStatsAtomic(playerId, dict);
        }

        public async Task<Dictionary<string, long>> SetIfMore(string playerId, string questKey, long value)
        {
            return await _playerProgressRepo.SetStatIfMoreAtomic(playerId, questKey, value);
        }

        bool IsNotReady(AchievementProgress prog, Achievement achievement)
        {
            var current = prog.Stats.ContainsKey(achievement.Type.ToString()) 
                ? prog.Stats[achievement.Type.ToString()] 
                : 0;

            return current < achievement.TargetValue;
        }

        public async Task<RewardGrantDTO> ClaimQuestReward(string playerId, string questKey)
        {
            var ach = await _questManageRepo.GetByKey(questKey);

            //Check if quest still available
            if (ach == null || !ach.IsActive || ach.RewardValue <= 0)
            {
                return null;
            }

            var progress = await _playerProgressRepo.GetPlayerProgressDoc(playerId);

            // Check if ready and not already claimed
            if (progress.CompletedAchievements.ContainsKey(questKey) || IsNotReady(progress, ach))
            {
                return null;
            }

            await _playerProgressRepo.MarkQuestCompleted(playerId, questKey);

            var playerData = await _playerManagerRepo.GetPlayerDataByPlayerId(playerId);

            return await _rewardService.ClaimReward(playerData, ach.Reward);
        }

        /// <summary>
        /// "Claim all" for daily missions: marks every ready, unclaimed quest in one transaction, then grants their rewards in order.
        /// </summary>
        public async Task<BatchClaimResponse> ClaimQuestRewards(string playerId, List<string> questKeys)
        {
            var response = new BatchClaimResponse();
            var keys = questKeys ?? new List<string>();

            var claimable = (await _questManageRepo.GetAllActive())
                .Where(q => q.RewardValue > 0 && keys.Contains(q.Key))
                .GroupBy(q => q.Key)
                .ToDictionary(g => g.Key, g => g.First());

            var marked = await _playerProgressRepo.MarkQuestsCompleted(playerId, claimable.Keys,
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
                _logger.LogError(ex, $"Quest {key} was marked claimed but its reward failed for player {playerData?.Id}");
                return new QuestClaimResult { Key = key, Reward = new RewardGrantDTO { Success = false, AdditionalInfo = ex.Message } };
            }
        }

        public async Task<Dictionary<string, long>> IncrementCraft(string playerId, RarityType craftedRarity)
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

            return new Dictionary<string, long>() { [statKey] = result.Stats[statKey] };
        }

        public async Task<Dictionary<string, long>> IncrementDrop(string playerId, RarityType craftedRarity)
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
                return new Dictionary<string, long>();

            var stats = new Dictionary<string, long>
            {
                {statKey, 1 },
                {AchievementType.ItemsDropped.ToString(), 1 }
            };

            return await _playerProgressRepo.IncrementStatsAtomic(playerId, stats);
        }
    }
}
