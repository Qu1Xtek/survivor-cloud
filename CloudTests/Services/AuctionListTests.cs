using CloudLogic.RepoModels.AuctionHouse;
using CloudLogic.Services.Trading;
using FluentAssertions;

namespace CloudTests.Services
{
    [TestClass]
    public sealed class AuctionListTests
    {
        [TestMethod]
        public void For_MarksTheCallersAuctionsAndHidesOwners()
        {
            var auctions = new List<AHItem>
            {
                new AHItem { Id = "a1", ItemId = "i1", Price = 100, OwnerId = "me" },
                new AHItem { Id = "a2", ItemId = "i2", Price = 200, OwnerId = "someone-else" },
            };

            var list = AuctionList.For(auctions, "me");

            list.Select(a => a.IsMine).Should().Equal(true, false);
            list[1].Should().BeEquivalentTo(new { Id = "a2", ItemId = "i2", Price = 200 });
            typeof(CloudLogic.Response.DTO.AuctionDTO).GetProperty("OwnerId").Should().BeNull();
        }

        [TestMethod]
        public void For_HandlesAnEmptyCache()
        {
            AuctionList.For(null!, "me").Should().BeEmpty();
        }
    }
}
