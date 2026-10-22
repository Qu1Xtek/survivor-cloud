using CloudLogic.DI;
using CloudLogic.RepoModels.Player;
using CloudLogic.Repositories;
using CloudLogic.Services.Items;
using Google.Cloud.Firestore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;

namespace CloudLogic.Services.Player
{
    public class TalentsService
    {
        private IPlayerManagerService _playerManagerService;
        private readonly IGameApiClient _gameApiClient;
        private readonly IExecutionContext? _ctx;

        public TalentsService(IPlayerManagerService plrManager, RequestContextAccessor context)
        {
            _playerManagerService = plrManager;
            _gameApiClient = GameApiClient.Create();
            _ctx = context.ExecutionContext;
        }

        public async Task<PlayerData> LearnTalent(int talentId)
        {
            var playerData = await _playerManagerService
                .GetPlayerDataAsync(_ctx.PlayerId, _ctx.UserId);

            var key = "Talents";

            var settings = await _gameApiClient.RemoteConfigSettings
                .AssignSettingsGetAsync(_ctx,
                _ctx.AccessToken,
                _ctx.ProjectId,
                key: new List<string>() { key });

            var stng = settings.Data.Configs.Settings[key].ToString();
            var nlcrDict = JsonConvert.DeserializeObject<Dictionary<string, int[]>>(stng);

            var dependencies = nlcrDict[talentId.ToString()];

            if (!playerData.LearnedTalents.Contains(talentId) && dependencies != null
                && (dependencies.Contains(0) || dependencies.Any(d => playerData.LearnedTalents.Contains(d))))
            {
                playerData.LearnedTalents.Add(talentId);
                playerData.TalentPoints -= 1;

                var newPlrData = new PlayerData()
                {
                    Id = playerData.Id,
                    LearnedTalents = playerData.LearnedTalents,
                    TalentPoints = playerData.TalentPoints,
                };

                return await _playerManagerService.UpsertPlayerData(newPlrData, playerData);

            }

            return playerData;
        }
        public async Task<PlayerData> AddCraftingMaterials(string playerId, int amount)
        {
            return null;
            //return await _playerDataRepo.AddCraftingMaterial(playerId, amount);
        }

        public async Task<PlayerData> AddTalentResetToken(string playerId, int amount)
        {
            return null;

            //return await _playerDataRepo.AddTalentResetToken(playerId, amount);
        }

        public async Task<PlayerData> PlayerResetTalents(IExecutionContext ctx)
        {
            return null;
            //var playerData = await _playerDataRepo.GetPlayerDataByPlayerId(ctx.PlayerId);

            //if (playerData.TalentResetToken > 0 && playerData.LearnedTalents != null && playerData.LearnedTalents.Count > 0)
            //{
            //    playerData.TalentPoints += playerData.LearnedTalents.Count;
            //    playerData.LearnedTalents = new List<int>();
            //    playerData.TalentResetToken -= 1;

            //    await _playerDataRepo.SetPlayerDataByPlayerId(ctx.PlayerId, playerData);
            //}

            //return playerData;
        }
    }
}
