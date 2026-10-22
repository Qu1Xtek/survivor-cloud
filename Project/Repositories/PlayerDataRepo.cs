using CloudLogic.RepoModels.Player;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace CloudLogic.Repositories
{
    public class PlayerDataRepo : BaseFireStoreService
    {
        public PlayerDataRepo(ILogger logger) : base("PlayerData", logger)
        {
        }

        public virtual async Task<PlayerData> GetPlayerDataByPlayerId(string playerId, Transaction tran = null)
        {
            var result = await GetRecordById<PlayerData>(playerId, tran);

            return result;
        }
    
        public virtual async Task<PlayerData> SetPlayerDataByPlayerId(string playerId, PlayerData playerData, Transaction tran = null)
        {
            return await UpdateRecord(playerData, tran);
        }

        public async Task UpsertPlayerData(
            PlayerData playerData,
            Transaction tran = null)
        {
            await UpsertRecord(playerData, tran);
        }

        public async Task<PlayerData> UpsertPlayerData(
            PlayerData playerData,
            PlayerData oldplayerData,
            Transaction tran = null)
        {
            return await UpsertRecord(playerData, oldplayerData, tran);
        }

        public virtual async Task<PlayerData> CreatePlayerData(string playerId, string username, PlayerData playerData, Transaction tran = null)
        {
            playerData.Id = playerId;
            playerData.PlayerId = username;
            playerData.CreatedAt = Timestamp.GetCurrentTimestamp();
            playerData.Crystals = 3;
            playerData.LearnedTalents = new System.Collections.Generic.List<int>();
            playerData.NLCR = 3;
            playerData.LoginRewardInfo = new LoginRewardInfo { ConcecutiveLoginCount = 0, LastClaimDate = default, Week = 0, Year = 0};
            playerData.RunData = new RunData {  GoldPicked = new List<long>(), GoldDropped = new List<long>()  };


            return await CreateUniqueRecord(playerData, tran);
        }

        public async Task SetPlatform(string playerId, string platfrom)
        {
            var plrData = await GetPlayerDataByPlayerId(playerId);

            var newplrData = new PlayerData() { Id = plrData.Id, Platform  = platfrom };


            await UpsertRecord(newplrData);
        }

        internal async Task AddAuction(string playerId)
        {
            var plrData = await GetPlayerDataByPlayerId(playerId);
            plrData.AuctionsCount += 1;

            await SetPlayerDataByPlayerId(playerId, plrData);
        }

        internal async Task RemoveAuction(string playerId)
        {
            var plrData = await GetPlayerDataByPlayerId(playerId);
            plrData.AuctionsCount -= 1;

            await SetPlayerDataByPlayerId(playerId, plrData);
        }

        internal virtual async Task<bool> UpdatePlayerIdUsername(string playerId, string username)
        {
            var plrData = await GetPlayerDataByPlayerId(playerId);

            var newplrData = new PlayerData() {  Id = plrData.Id, PlayerId = username};


            await UpsertRecord(newplrData);

            return true;
        }

        internal async Task REMOVEPLAYERDATAUSEDONLYFORTESTING(string playerId, Transaction tran)
        {
            await DeleteRecordById(playerId);
        }
    }
}
