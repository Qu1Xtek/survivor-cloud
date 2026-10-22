using CloudLogic.Repositories;
using CloudLogic.Services.Shop;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CloudTests.Integration;

[TestClass]
public class SkinShopIntegrationTests : IntegrationTestBase
{
    private const string TestPlayerId = "c6Z2SkyzwGEddzXkqbNUjJKAKOGn";
    private const string TestSkinShopId = "necro_sword";

    [TestMethod]
    [TestCategory("Integration")]
    public async Task BuySkin_Success_DeductsGemsAndCreatesPurchase()
    {
        using var scope = ServiceProvider.CreateScope();
        var sp = scope.ServiceProvider;

        var shopService = sp.GetRequiredService<ShopService>();
        var skinsRepo = sp.GetRequiredService<SkinsRepo>();
        var playerDataRepo = sp.GetRequiredService<PlayerDataRepo>();

        var before = await playerDataRepo.GetPlayerDataByPlayerId(TestPlayerId);
        before.Should().NotBeNull();

        var result = await shopService.BuySkinItem(TestPlayerId, TestSkinShopId);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();

        var after = await playerDataRepo.GetPlayerDataByPlayerId(TestPlayerId);
        after!.BlueGems.Should().BeLessThan(before!.BlueGems);

        var owned = await skinsRepo.GetByPlayerId(TestPlayerId);
        owned.Should().Contain(s => s.SkinId != null);
    }

    [TestMethod]
    [TestCategory("Integration")]
    public async Task BuySkin_AlreadyOwned_ReturnsFail()
    {
        using var scope = ServiceProvider.CreateScope();
        var sp = scope.ServiceProvider;

        var shopService = sp.GetRequiredService<ShopService>();

        var result = await shopService.BuySkinItem(TestPlayerId, TestSkinShopId);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.AdditionalInfo.Should().Be("already_owned");
    }
}
