using CloudLogic.RepoModels.Achievements;
using CloudLogic.RepoModels.Player;
using CloudLogic.Repositories;
using CloudLogic.Response;
using CloudLogic.Services.Achievements;
using CloudLogic.Services.Items;
using CloudLogic.Util;
using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CloudLogic.Services.Player.RunManagement
{
    public class RunCompleter
    {
        private readonly IPlayerManagerService _playerService;
        private readonly AuditRepo _auditRepo;
        private readonly AchievementsService _achievementsService;

        public RunCompleter(IPlayerManagerService playerService,
            AchievementsService achievementsService,
            AuditRepo auditRepo)
        {
            _playerService = playerService;
            _achievementsService = achievementsService;
            _auditRepo = auditRepo;

        }

        public async Task<BaseResponse> CompleteRun(string playerId)
        {
            var result = new BaseResponse()
            {
                Success = false,
                PlayerData = new PlayerDataDTO()
            };

            bool longerThan5Min = false;
            bool leveledUp = false;
            long oldMaxLevel = 0;
            long newMaxLevel = 0;
            long newCrystals = 0;

            RunData runDataSnapshot = null;

            await _auditRepo.RunOurTransactionAsync(async tran =>
            {
                var player = await _playerService.GetPlayerDataAsync(playerId, null, tran);

                var diff = DateTime.UtcNow - player.RunData.LastRunCompletedAt.ToDateTime();

                if (player.RunData?.LastRunCompletedAt != null && diff < TimeSpan.FromMinutes(5))
                {
                    return;
                }

                longerThan5Min = true;
                oldMaxLevel = player.MaxLevelReached;

                player.RunData.LastRunCompletedAt = Timestamp.FromDateTime(DateTime.UtcNow);
                runDataSnapshot = player.RunData;

                var playerData = new PlayerData()
                {
                    Id = player.Id,
                    RunData = player.RunData
                };

                if (player.SelectedLevel == player.MaxLevelReached)
                {
                    leveledUp = true;

                    playerData.MaxLevelReached = player.MaxLevelReached + 1;
                    playerData.Crystals = player.Crystals + 3;

                    newMaxLevel = playerData.MaxLevelReached;
                    newCrystals = playerData.Crystals;
                }

                await _playerService.UpsertPlayerData(playerData, player, tran);
            });

            if (!longerThan5Min)
            {
                return result;
            }

            if (leveledUp)
            {
                result.PlayerData.MaxLevelReached = BindableLong.Of(newMaxLevel);
                result.PlayerData.Crystals = BindableLong.Of(newCrystals);

                // Because we need to show to the user visually appealing stuff (if you are just starting you are level 0 + 1 is 1 ( user should see level 1 + 1 = 2)
                var progress = await _achievementsService.Set(playerId,
                    AchievementType.ReachMap.ToString(),
                    oldMaxLevel + 2);

                result.AddProgress(AchievementType.ReachMap.ToString(), progress);
            }

            var incrementResult = await _achievementsService.IncrementMultiple(
                playerId,
                new Dictionary<string, long>()
                {
                    [AchievementType.CompleteRuns.ToString()] = 1,
                    [AchievementType.BossKills.ToString()] = 2
                });

            result.AddProgress(AchievementType.CompleteRuns.ToString(), incrementResult);
            result.AddProgress(AchievementType.BossKills.ToString(), incrementResult);

            result.Success = true;
            return result;
        }
    }
}
