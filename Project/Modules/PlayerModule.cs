using CloudLogic.DI;
using CloudLogic.RepoModels.Achievements;
using CloudLogic.RepoModels.Items;
using CloudLogic.RepoModels.Player;
using CloudLogic.Repositories;
using CloudLogic.Response;
using CloudLogic.Services.Achievements;
using CloudLogic.Services.Interfaces;
using CloudLogic.Services.Items;
using CloudLogic.Services.Player;
using CloudLogic.Util;
using CloudLogic.Util.Map;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;

namespace CloudLogic.Modules
{
    public class PlayerModule
    {
        private readonly ILogger _logger;        

        public PlayerModule(ILogger<PlayerModule> logger)
        {
            _logger = logger;
        }

        [CloudCodeFunction(nameof(UpdatePlayerIdUsername))]
        public async Task<bool> UpdatePlayerIdUsername(IExecutionContext ctx, string username)
        {
            return await CloudDi.WithScope(_logger, ctx, sp =>
                sp.GetRequiredService<IPlayerManagerService>()
                .UpdatePlayerIdUsername(ctx.PlayerId, username));
        }

        [CloudCodeFunction(nameof(LoadPlayerData))]
        public async Task<PlayerDataResponse> LoadPlayerData(IExecutionContext ctx)
        {
            return await CloudDi.WithScope(_logger, ctx, async sp =>
            {
                var gameApiClient = GameApiClient.Create();

                var playerManager = sp.GetRequiredService<IPlayerManagerService>();
                var playerInfoRepo = sp.GetRequiredService<PlayerInfoRepo>();
                var playerGemsService = sp.GetRequiredService<IPlayerGemService>();

                var response = await gameApiClient.PlayerNamesApi.GetNameAsync(ctx, ctx.ServiceToken, ctx.PlayerId);
                var playerData = await playerManager.GetPlayerDataAsync(ctx.PlayerId, response.Data.Name);

                _ = Task.Run(() => playerInfoRepo.LogLogin(ctx.PlayerId, playerData.PlayerId));

                var playerGems = await playerGemsService.GetPlayerGems(ctx.PlayerId);

                PlayerDataResponse result = new PlayerDataResponse()
                {
                    Success = true,
                    PlayerData = Mapper.MapPlayerDataToDTO(playerData),
                    PlayerGems = playerGems,
                };

                return result;
            });
        }

        [CloudCodeFunction(nameof(GetPlayerItems))]
        public async Task<List<Item>> GetPlayerItems(IExecutionContext ctx)
        {
            return await CloudDi.WithScope(_logger, ctx, sp =>
                sp.GetRequiredService<ItemsService>().GetAllPlayerItems(ctx.PlayerId));
        }

        [CloudCodeFunction(nameof(GetPlayerVer))]
        public async Task<int> GetPlayerVer(IExecutionContext ctx, string itemId)
        {
            return await CloudDi.WithScope(_logger, ctx, async sp =>
            {
                var ver = await sp.GetRequiredService<VersionSettingRepo>().GetVersionSetting();
                return ver.AndroidVersion;
            });
        }

        //[CloudCodeFunction(nameof(GetPlayerGold))]
        //public async Task<long> GetPlayerGold(IExecutionContext ctx)
        //{
        //    return await CloudDi.WithScope(_logger, sp =>
        //        sp.GetRequiredService<PlayerDataRepo>().GetGold(ctx.PlayerId));
        //}

        [CloudCodeFunction(nameof(PlayerLevelUp))]
        public async Task<BaseResponse> PlayerLevelUp(IExecutionContext ctx)
        {
            var data = await CloudDi.WithScope(_logger, ctx, sp =>
                sp.GetRequiredService<IPlayerManagerService>().LevelUp(ctx));

            await CloudDi.WithScope(_logger, ctx, sp =>
                sp.GetRequiredService<AchievementsService>()
                .Set(ctx.PlayerId, AchievementType.ReachLevel.ToString(), data.HeroLevel));

            return new BaseResponse()
            {
                Success = true,
                PlayerData = new PlayerDataDTO() { HeroLevel = BindableLong.Of(data.HeroLevel), Crystals = BindableLong.Of(data.Crystals) },
                AchievementProgressInfo = new Dictionary<string, long>() { [AchievementType.ReachLevel.ToString()] = data.HeroLevel }
            };    
        }

        [CloudCodeFunction(nameof(PlayerResetTalents))]
        public async Task<PlayerData> PlayerResetTalents(IExecutionContext ctx)
        {
            return null;
            //return await CloudDi.WithScope(_logger, ctx, sp =>
            //    sp.GetRequiredService<IPlayerManagerService>().PlayerResetTalents(ctx));
        }

        [CloudCodeFunction(nameof(LearnTalent))]
        public async Task<PlayerData> LearnTalent(IExecutionContext ctx, int talentId)
        {
            return await CloudDi.WithScope(_logger, ctx, sp =>
                sp.GetRequiredService<TalentsService>().LearnTalent(talentId));
        }


        [CloudCodeFunction(nameof(SetPlayerDevice))]
        public async Task SetPlayerDevice(IExecutionContext ctx, string deviceId)
        {
            await CloudDi.WithScope(_logger, ctx, sp =>
                sp.GetRequiredService<IPlayerManagerService>().UpdateDeviceId(ctx, deviceId));
        }

        [CloudCodeFunction(nameof(SetPlatform))]
        public async Task SetPlatform(IExecutionContext ctx, string platform)
        {
            await CloudDi.WithScope(_logger, ctx, sp =>
                sp.GetRequiredService<IPlayerManagerService>().SetPlatform(ctx.PlayerId, platform));
        }

        [CloudCodeFunction(nameof(CheckLoginTime))]
        public async Task CheckLoginTime(IExecutionContext ctx, float time, string platform, string appver, bool addressablesDownloaded = false)
        {
            await CloudDi.WithScope(_logger, ctx, async sp =>
            {
                var mngr = sp.GetRequiredService<ClientDiagnosticsRepo>();

                await mngr.LogEvent(ctx.PlayerId, addressablesDownloaded ? "Addressables" : "Regular", "Login", platform, appver, time);
            });
        }

        [CloudCodeFunction(nameof(CheckLoginTimev2))]
        public async Task CheckLoginTimev2(IExecutionContext ctx, float time, string platform, string appver, int type = 0)
        {
            await CloudDi.WithScope(_logger, ctx, async sp =>
            {
                var mngr = sp.GetRequiredService<ClientDiagnosticsRepo>();

                var reasons = new string[]
                {
                    "Regular",
                    "CachedAddressables",
                    "AddressablesDownloaded",
                    "Failed",
                    "Guest"
                };

                await mngr.LogEvent(ctx.PlayerId, "Login", reasons[type], platform, appver, time);
            });
        }
    }
}
