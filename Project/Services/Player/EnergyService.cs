using CloudLogic.RepoModels.Player;
using CloudLogic.Repositories;
using CloudLogic.Services.Items;
using CloudLogic.Util;
using Google.Cloud.Firestore;
using System;
using System.Threading.Tasks;

namespace CloudLogic.Services.Player
{
    public class EnergyService : IEnergyService
    {
        private IPlayerManagerService _playerManagerService;

        public EnergyService(IPlayerManagerService plrManager)
        {
            _playerManagerService = plrManager;
        }

        private const long MaxEnergy = 20;
        private const long OverflowCap = 99;

        private long CalculateRegeneratedEnergy(PlayerData playerData)
        {
            const double minutesPerEnergy = 4.0; // 1 energy per 4 minutes

            if (playerData.Energy >= MaxEnergy)
            {
                return playerData.Energy;
            }

            if (playerData.LastEnergyGiveStamp == null || playerData.LastEnergyGiveStamp == default(Timestamp))
            {
                return MaxEnergy;
            }

            DateTime lastUpdate = playerData.LastEnergyGiveStamp.ToDateTime();
            DateTime now = DateTime.UtcNow;
            
            double minutesElapsed = (now - lastUpdate).TotalMinutes;           
            long energyToRegenerate = (long)(minutesElapsed / minutesPerEnergy);
            long newEnergy = Math.Min(MaxEnergy, playerData.Energy + energyToRegenerate);

            DateTime updatedTimestamp = energyToRegenerate > 0 ? now : lastUpdate;

            return newEnergy;
        }

        public async Task<long> TakeEnergy(PlayerData playerData, int energyToTake)
        {
            var currentEnergy = CalculateRegeneratedEnergy(playerData);
            var energyDiff = currentEnergy - energyToTake;

            if (energyToTake < 0 || energyDiff < 0)
            {
                return -1;
            }

            var upsertData = new PlayerData
            {
                Id = playerData.Id,
                Energy = energyDiff
            };

            return energyDiff;
        }

        public async Task<long> AddEnergy(string playerId, int v, bool canOverflow = false)
        {
            var playerData = await _playerManagerService.GetPlayerDataAsync(playerId);
            var current = CalculateRegeneratedEnergy(playerData);
            var cap = canOverflow ? OverflowCap : Math.Max(MaxEnergy, current);
            var newEnergy = Math.Min(cap, current + v);

            // Avoid mutating the fetched instance (keeps callers consistent)
            var upsert = new PlayerData
            {
                Id = playerData.Id,
                Energy = newEnergy,
                LastEnergyGiveStamp = Timestamp.FromDateTime(DateTime.UtcNow),
            };

            var updatedData = await _playerManagerService.UpsertPlayerData(upsert, playerData);


            return updatedData.Energy;
        }
    }
}
