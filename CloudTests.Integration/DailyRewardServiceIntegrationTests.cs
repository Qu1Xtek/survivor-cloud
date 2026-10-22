using CloudLogic.Modules;
using CloudLogic.Repositories;
using CloudLogic.Services.Items;
using CloudLogic.Services.Reward;
using Google.Cloud.Firestore;
using Microsoft.Extensions.DependencyInjection;

using Unity.Services.CloudCode.Core;

namespace CloudTests.Integration
{
    /// <summary>
    /// Integration tests for DailyRewardService.
    /// These tests use real Firestore (or emulator) and test the full flow of claiming daily rewards.
    /// </summary>
    [TestClass]
    public class DailyRewardServiceIntegrationTests : IntegrationTestBase
    {
        private DailyRewardService _dailyRewardService;
        private PlayerDataRepo _playerDataRepo;
        private IPlayerManagerService _playerManagerService;
        private QuestRepo _questRepo;
        private DailyLoginWeekRepo _dailyLoginWeekRepo;
        private DailyLoginWeekService _dailyLoginWeekService;
        private IRewardService _rewardService;
        private string _testPlayerId;

        [TestInitialize]
        public void Setup()
        {
            _dailyRewardService = ServiceProvider.GetRequiredService<DailyRewardService>();
            _playerDataRepo = ServiceProvider.GetRequiredService<PlayerDataRepo>();
            _playerManagerService = ServiceProvider.GetRequiredService<IPlayerManagerService>();
            _questRepo = ServiceProvider.GetRequiredService<QuestRepo>();
            _dailyLoginWeekRepo = ServiceProvider.GetRequiredService<DailyLoginWeekRepo>();
            _dailyLoginWeekService = ServiceProvider.GetRequiredService<DailyLoginWeekService>();
            _rewardService = ServiceProvider.GetRequiredService<IRewardService>();
            _testPlayerId = GlobalTestPlayerId;

            //EnsureActiveWeekQuestDocs();
        }              

        [TestMethod]
        [TestCategory("Integration")]
        public async Task RewardModule_IsRegistered()
        {
            using var scope = ServiceProvider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<RewarModule>();

            //var plrData = await _playerDataRepo.GetPlayerDataByPlayerId(_testPlayerId);
            //plrData.HeroLevel = 5;
            //plrData.LoginRewardInfo.ConcecutiveLoginCount = 6;
            //plrData.LoginRewardInfo.LastClaimDate = Timestamp.FromDateTime(DateTime.UtcNow.AddDays(-2));

            await service.ClaimDailyReward(new Ctx { PlayerId = "c6Z2SkyzwGEddzXkqbNUjJKAKOGn" });
        }
    }

    public class Ctx : IExecutionContext
    {
        public string PlayerId { get; init; } = "";
        public string UserId { get; init; } = "";
        public string ServiceToken => "";
        public string AccessToken => "";
        public string ProjectId => "";
        public string EnvironmentId => "";
        public string EnvironmentName => "";
        public string Issuer => "";
        public string AnalyticsUserId => "";
        public string UnityInstallationId => "";
        public string CorrelationId => "";
    }
}
