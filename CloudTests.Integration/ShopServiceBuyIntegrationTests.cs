using CloudLogic.RepoModels.Player;
using CloudLogic.RepoModels.PlayerRewards;
using CloudLogic.RepoModels.Shop;
using CloudLogic.Repositories;
using CloudLogic.Services.Items;
using CloudLogic.Services.Shop;
using FluentAssertions;
using Google.Cloud.Firestore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unity.Services.CloudCode.Core;

namespace CloudTests.Integration;

[TestClass]
public class ShopServiceBuyIntegrationTests : IntegrationTestBase
{
    private sealed class Ctx : IExecutionContext
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

    [TestMethod]
    [TestCategory("Integration")]
    public async Task BuyShopItem_MatchesCloudCodeResolution()
    {
        using var scope = ServiceProvider.CreateScope();
        var sp = scope.ServiceProvider;
        var playerId = "JvXnIePTCP3qE7vZQd85b67WNmZH";
        var before = (await sp.GetRequiredService<PlayerDataRepo>().GetPlayerDataByPlayerId(playerId))!;
        var result = await sp.GetRequiredService<ShopService>().BuyShopItem("JvXnIePTCP3qE7vZQd85b67WNmZH", "armor_pack_30");
        try
        {
            result.Success.Should().BeTrue();
            var after = (await sp.GetRequiredService<PlayerDataRepo>().GetPlayerDataByPlayerId(playerId))!;
            after.BlueGems.Should().Be(50);
            after.Gold.Should().Be(1);
        }
        finally
        {
            //await sp.GetRequiredService<IPlayerManagerService>().RemovePLAYERDATAUSEDONLYFORTESTING(playerId);
        }
    }
}
