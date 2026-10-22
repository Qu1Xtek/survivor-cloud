using CloudLogic.DI;
using CloudLogic.RepoModels.Achievements;
using CloudLogic.RepoModels.Player;
using CloudLogic.Repositories;
using CloudLogic.Services.Achievements;
using CloudLogic.Services.Items;
using CloudLogic.Services.Trading;
using CloudLogic.Util;
using CloudLogic.Util.ComModels;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;

namespace CloudLogic.Services.Player
{
    public class PlayerManagerService : IPlayerManagerService
    {
        private readonly PlayerDataRepo _playerDataRepo;
        private readonly ILogger _logger;
        private readonly IGameApiClient _gameApiClient;
        private readonly RequestContextAccessor _contextAccessor;
        private ResourceUtil _resourceUtil;


        public PlayerManagerService(
            PlayerDataRepo goldService,
            IMailService mailService,
            ILogger logger,
            RequestContextAccessor contextAccessor)
        {
            _playerDataRepo = goldService;
            _logger = logger;
            _gameApiClient = GameApiClient.Create();
            _contextAccessor = contextAccessor;
            _resourceUtil = new ResourceUtil();
        }


        public async Task<PlayerData> AddRerollRune(string playerId)
        {
            var player = await _playerDataRepo.GetPlayerDataByPlayerId(playerId);
            player.RerollRuneCount++;

            return await _playerDataRepo.SetPlayerDataByPlayerId(playerId, player);
        }

        public async Task<ResourceUpdate> UseRerollRune(string playerId)
        {
            var player = await GetPlayerDataAsync(playerId);
            if (player.RerollRuneCount > 0)
            {
                var newPlrData = new PlayerData()
                {
                    Id = player.Id,
                    RerollRuneCount = player.RerollRuneCount - 1,
                    ExplicitUpserts = new List<string>() { nameof(PlayerData.RerollRuneCount) }
                };
                await UpsertPlayerData(newPlrData);

                return new ResourceUpdate() 
                {
                    Success = true,
                    RerollRune = newPlrData.RerollRuneCount
                };
            }

            return new ResourceUpdate() { Success = false };
        }

        public async Task<bool> SetFirstRunStart(string playerId)
        {
            var player = await GetPlayerDataAsync(playerId);

            var newPlrData = new PlayerData()
            {
                Id = player.Id,
                FirstRunStarted = true,
                ExplicitUpserts = new List<string>() { nameof(PlayerData.FirstRunStarted) }
            };

            await UpsertPlayerData(newPlrData);
           return true;
        }

        public async Task<ResourceUpdate> TakeCraftingMaterialsAndGold(string playerId, long goldAmount, int craftingAmount, Transaction tran = null)
        {
            var playerData = await GetPlayerDataAsync(playerId);

            playerData = _resourceUtil.TakeCraftingMatAndGold(playerData, goldAmount, craftingAmount);

            if (playerData != null)
            {
                var newPlrData = new PlayerData()
                {
                    Id = playerData.Id,
                    Gold = playerData.Gold,
                    CraftingMaterials = playerData.CraftingMaterials
                };

                newPlrData.ExplicitValuesUpsert(new string[] { nameof(PlayerData.CraftingMaterials), nameof(PlayerData.Gold) });

                await UpsertPlayerData(newPlrData, tran);
                return new ResourceUpdate() 
                { 
                    Success = true,
                    Gold = goldAmount,
                    CraftingMat = craftingAmount
                };
            }

            return new ResourceUpdate() { Success = false };
        }

        public async Task<PlayerData> GetPlayerDataAsync(string playerId, string? userName = null, Transaction tran = null)
        {
            try
            {
                var result = await _playerDataRepo.GetPlayerDataByPlayerId(playerId, tran);

                if (result == null)
                {
                    // Business logic: Create new player with default values
                    var newPlayerData = new PlayerData();
                    newPlayerData.Crystals = 3;
                    newPlayerData.LearnedTalents = new List<int>();
                    newPlayerData.NLCR = 3;
                    newPlayerData.LoginRewardInfo = new LoginRewardInfo
                    {
                        ConcecutiveLoginCount = 0,
                        LastClaimDate = default,
                        Week = 0,
                        Year = 0,
                    };

                    result =  await _playerDataRepo.CreatePlayerData(playerId, userName, newPlayerData, tran);
                }
                else
                {
                    var week = GetWeekNumber();
                    var year = DateTime.UtcNow.Year;


                    if (result.LoginRewardInfo.Week != week || result.LoginRewardInfo.Year != year)
                    {
                        var playerLoginData = new PlayerData
                        {
                            Id = result.Id,
                            LoginRewardInfo = result.LoginRewardInfo
                        };

                        playerLoginData.LoginRewardInfo.ConcecutiveLoginCount = 0;
                        playerLoginData.LoginRewardInfo.Week = week;
                        playerLoginData.LoginRewardInfo.Year = year;

                        result = await _playerDataRepo.UpsertPlayerData(playerLoginData, result, tran);
                    }

                    result.NextBlueGemAdAvailableAt = result.NextBlueGemAdAt.ToDateTime();
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, $"Player data for player {playerId} errored,{ex.Message} -  {ex.StackTrace}");
                return null;
            }
        }

        private int GetWeekNumber()
        {
            var date = DateTime.UtcNow;
            var cal = System.Globalization.CultureInfo.InvariantCulture.Calendar;
            var day = cal.GetDayOfWeek(date);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                date = date.AddDays(3);
            }
            return cal.GetWeekOfYear(date, System.Globalization.CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        public async Task<PlayerData> LevelUp(IExecutionContext ctx)
        {
            var playerData = await GetPlayerDataAsync(ctx.PlayerId, ctx.UserId);

            if (playerData.NLCR <= playerData.Crystals)
            {
                playerData.HeroLevel++;
                playerData.Crystals -= playerData.NLCR;
                playerData.NLCR = 3;
                playerData.TalentPoints++;

                return await _playerDataRepo.SetPlayerDataByPlayerId(ctx.PlayerId, playerData);
            }

            return playerData;
        }

        public async Task UpdateDeviceId(IExecutionContext ctx, string deviceId)
        {
            var playerData = await GetPlayerDataAsync(ctx.PlayerId, ctx.UserId);

            playerData.DeviceId = deviceId;

            await _playerDataRepo.SetPlayerDataByPlayerId(playerData.Id, playerData);
        }

        public async Task<PlayerData> UpsertPlayerData(PlayerData plrData, PlayerData oldPlrData, Transaction tran = null)
        {
            return await _playerDataRepo.UpsertPlayerData(plrData, oldPlrData, tran);
        }
        public async Task UpsertPlayerData(PlayerData plrData, Transaction tran = null)
        {
            await _playerDataRepo.UpsertPlayerData(plrData, tran);
        }

        public async Task<bool> UpdatePlayerIdUsername(string playerId, string username)
        {
            return await _playerDataRepo.UpdatePlayerIdUsername(playerId, username);
        }


        public async Task<PlayerData> IncreaseItemDropCountForId(string playerId, Transaction tran = null)
        {
            var plrData = await GetPlayerDataAsync(playerId, null, tran);
            plrData.RunData.ItemsDropped++;

            var upsertData = new PlayerData()
            {
                Id = playerId,
                RunData = plrData.RunData
            };

            return await UpsertPlayerData(upsertData, plrData, tran);
        }

        public async Task<PlayerData> IncreaseItemDropCountForData(PlayerData plrData, Transaction tran = null)
        {
            plrData.RunData.ItemsDropped++;

            var upsertData = new PlayerData()
            {
                Id = plrData.Id,
                RunData = plrData.RunData
            };

            return await UpsertPlayerData(upsertData, plrData, tran);
        }

        public async Task RemovePLAYERDATAUSEDONLYFORTESTING(string playerId, Transaction tran = null)
        {
            await _playerDataRepo.REMOVEPLAYERDATAUSEDONLYFORTESTING(playerId, tran);
        }


        public async Task SetPlatform(string playerId, string platform)
        {
            await _playerDataRepo.SetPlatform(playerId, platform);
        }
    }
}
