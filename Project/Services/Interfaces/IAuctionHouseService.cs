using CloudLogic.RepoModels.AuctionHouse;
using CloudLogic.RepoModels.Items;
using System.Threading.Tasks;

namespace CloudLogic.Services.Trading
{
    public interface IAuctionHouseService
    {
        Task DoInit();
        Task<string> PostAHItem(string ownerId, AHItem ahItem);
        Task<Item> BuyAHItem(string buyerId, string auctionId);
        Task<bool> TransferAuctions(string fromPlayerId, string transferTo);
        Task<string> CancelAuction(string? playerId, string auctionId);
    }
}
