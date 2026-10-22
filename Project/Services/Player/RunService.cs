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

namespace CloudLogic.Services.Player
{
    public class RunService : IRunService
    {
        private readonly IEnergyService _energyService;
        private readonly IPlayerManagerService _playerService;
        private readonly ItemDropService _itemDropService;        
        private readonly EventsLogger _logger;

        public RunService(IEnergyService energyService,
            IPlayerManagerService plrService,
            ItemDropService itemDropService,
            AuditRepo auditRepo,
            AchievementsService achievementsService,
            EventsLogger logger)
        {
            _energyService = energyService;
            _playerService = plrService;
            _itemDropService = itemDropService;
            _logger = logger;
        }

        public async Task<StartRunResponse> StartRun(string playerId, int selectedLevel)
        {
            var currentPlayerData = await _playerService.GetPlayerDataAsync(playerId);

            if (currentPlayerData == null)
            {
                _logger.LogWithCustom(nameof(StartRun), "Level higher than max reach");

                return new StartRunResponse() { Success = false };
            }

            var initialEnergy = currentPlayerData.Energy;
            var energyLeft = await _energyService.TakeEnergy(currentPlayerData, 5);

            // Business logic: Validate selected level and reset RunData
            if (selectedLevel > currentPlayerData.MaxLevelReached
                || energyLeft < 0)
            {
                _logger.LogWithCustom(nameof(StartRun), "Level higher than max reach");

                return new StartRunResponse() { Success = false };
            }

            await _itemDropService.PrepareNewRunDropBag(currentPlayerData.Id);

            var playerData = new PlayerData()
            {
                Id = currentPlayerData.Id,
                SelectedLevel = selectedLevel,
                Energy = energyLeft,
                LastEnergyGiveStamp = Timestamp.FromDateTime(DateTime.UtcNow),
                FirstRunStarted = true,
                RunData = new RunData
                {
                    GoldDropped = new List<long>(),
                    GoldPicked = new List<long>(),
                    CraftingMaterialsDropped = 0,
                    CraftingMaterialsPicked = 0,
                    ItemsDropped = 0,
                    ItemsPicked = 0,
                    MaxGoldDrops = new Random().Next(10, 16),
                    MaxItemDrops = new Random().Next(8, 16),
                    LastRunCompletedAt = currentPlayerData?.RunData?.LastRunCompletedAt ?? default
                }
            };

            playerData.ExplicitValuesUpsert(new string[] { nameof(PlayerData.Energy) });
            playerData.ExplicitValuesUpsert(new string[] { nameof(PlayerData.SelectedLevel), nameof(PlayerData.RunRevives) });

            var updatedPlayerData = await _playerService.UpsertPlayerData(playerData, currentPlayerData);

            return new StartRunResponse()
            {
                Success = true,
                PlayerData = new PlayerDataDTO()
                {
                    SelectedLevel = BindableInt.Of(playerData.SelectedLevel),
                    Energy = BindableLong.Of(playerData.Energy),
                    LastEnergyGiveStamp = BindableDateTime.Of(playerData.LastEnergyGiveStamp.ToDateTime()),
                },
                MaxGoldDrops = playerData.RunData.MaxGoldDrops,
                MaxItemDrops = playerData.RunData.MaxItemDrops,
            };
        }
    }
}
