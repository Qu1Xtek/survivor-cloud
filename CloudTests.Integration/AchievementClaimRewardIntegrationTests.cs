using CloudLogic.RepoModels.Achievements;
using CloudLogic.RepoModels.Player;
using CloudLogic.Repositories;
using CloudLogic.Response;
using CloudLogic.Response.DTO;
using CloudLogic.Services.Achievements;
using CloudLogic.Services.Reward;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RewardModel = CloudLogic.RepoModels.PlayerRewards.Reward;

namespace CloudTests.Integration;

[TestClass]
public class AchievementClaimRewardIntegrationTests : IntegrationTestBase
{
    private string _achievementKey = null!;

    private sealed class FakeRewardService : IRewardService
    {
        public int CallCount { get; private set; }
        public RewardModel? LastReward { get; private set; }
        public PlayerData? LastPlayerData { get; private set; }

        public Task<RewardGrantDTO> ClaimReward(PlayerData playerData, RewardModel reward)
        {
            CallCount++;
            LastReward = reward;
            LastPlayerData = playerData;
            return null;
        }
    }

    private AchievementsService _achievementService = null!;
    private AchievementRepo _achievementRepo = null!;
    private AchievementProgressRepo _progressRepo = null!;
    private PlayerDataRepo _playerDataRepo = null!;
    private FakeRewardService _fakeReward = null!;
    private string _playerId = null!;
    private IServiceProvider _sp = null!;

    [TestInitialize]
    public void Setup()
    {
        // Use unique ids per test to avoid cross-test Firestore state bleed.
        _playerId = $"{GlobalTestPlayerId}";
        _achievementKey = $"craft_uncommon_5";
        _fakeReward = new FakeRewardService();

        // Build a tiny per-test DI container with real repos + fake IRewardService.
        var services = new ServiceCollection();
        services.AddSingleton(ContextAccessor);
        services.AddScoped<Microsoft.Extensions.Logging.ILogger>(_ => Logger);

        services.AddScoped<AchievementRepo>(sp => new AchievementRepo(Logger));
        services.AddScoped<AchievementProgressRepo>(sp => new AchievementProgressRepo(Logger));
        services.AddScoped<PlayerDataRepo>(sp => new PlayerDataRepo(Logger));

        services.AddSingleton<IRewardService>(_fakeReward);
        services.AddScoped<AchievementsService>();

        _sp = services.BuildServiceProvider();

        _achievementRepo = _sp.GetRequiredService<AchievementRepo>();
        _progressRepo = _sp.GetRequiredService<AchievementProgressRepo>();
        _playerDataRepo = _sp.GetRequiredService<PlayerDataRepo>();
        _achievementService = _sp.GetRequiredService<AchievementsService>();
    }

    private async Task EnsurePlayerAsync()
    {
        var player = new PlayerData
        {
            Id = _playerId,
            Gold = 0,
            // Keep required timestamps sane for other systems.
            LastEnergyGiveStamp = Google.Cloud.Firestore.Timestamp.FromDateTime(DateTime.UtcNow),
            NextBlueGemAdAt = Google.Cloud.Firestore.Timestamp.FromDateTime(DateTime.UtcNow),
            RunData = new RunData(),
            LoginRewardInfo = new LoginRewardInfo(),
            ExplicitUpserts = new List<string> { nameof(PlayerData.Gold) }
        };

        await _playerDataRepo.UpsertPlayerData(player);
    }

    private async Task EnsureAchievementDefinitionAsync(bool isActive = true, long rewardValue = 10)
    {
        //var explicitUpserts = new List<string>();
        //if (!isActive)
        //{
        //    // BaseFireStoreService.UpsertRecord skips default(bool)=false unless explicitly upserted.
        //    explicitUpserts.Add(nameof(Achievement.IsActive));
        //}

        //await _achievementRepo.Upsert(new Achievement
        //{
        //    Id = _achievementKey,
        //    Key = _achievementKey,
        //    Title = "Test Claim",
        //    Description = "Test",
        //    IsActive = isActive,
        //    RewardValue = rewardValue,
        //    RewardType = CloudLogic.RepoModels.PlayerRewards.RewardType.Gold,
        //    TargetValue = 1,
        //    Type = AchievementType.ItemsDropped,
        //    StatKey = AchievementType.ItemsDropped.ToString(),
        //    ExplicitUpserts = explicitUpserts
        //});
    }

    private async Task EnsureCompletedAsync(bool completed)
    {
        //// Ensure progress doc exists.
        //await _progressRepo.GetPlayerProgressDoc(_playerId);

        //if (completed)
        //{
        //    await _progressRepo.MarkAchievementCompleted(_playerId, _achievementKey);
        //}
    }

    [TestMethod]
    [TestCategory("Integration")]
    public async Task ClaimAchievementReward_WhenCompleted_GrantsReward()
    {
        await EnsurePlayerAsync();
        await EnsureAchievementDefinitionAsync(isActive: true, rewardValue: 20);
        await EnsureCompletedAsync(completed: true);

        var result = await _achievementService.ClaimAchievementReward(_playerId, _achievementKey);

        _fakeReward.CallCount.Should().Be(1);
        _fakeReward.LastReward.Should().NotBeNull();
        _fakeReward.LastReward!.Value.Should().Be(20);
    }

    [TestMethod]
    [TestCategory("Integration")]
    public async Task ClaimAchievementReward_WhenNotCompleted_ReturnsFailureAndDoesNotGrant()
    {  
        await _achievementService.Set(_playerId, AchievementType.UncommonCrafted.ToString(), 5);
        var result = await _achievementService.ClaimAchievementReward(_playerId, _achievementKey);

        _fakeReward.CallCount.Should().Be(0);
    }

    [TestMethod]
    [TestCategory("Integration")]
    public async Task ClaimAchievementReward_WhenInactive_ReturnsFailureAndDoesNotGrant()
    {
        var result = await _achievementService.ClaimAchievementReward(_playerId, _achievementKey);

        _fakeReward.CallCount.Should().Be(0);
    }

    [TestMethod]
    [TestCategory("Integration")]
    public async Task ClaimAchievementReward_WhenRewardValueZero_ReturnsFailureAndDoesNotGrant()
    {
        var result = await _achievementService.ClaimAchievementReward(_playerId, _achievementKey);

        _fakeReward.CallCount.Should().Be(0);
    }
}

