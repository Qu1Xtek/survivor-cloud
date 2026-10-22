using CloudLogic.RepoModels.AuctionHouse;
using CloudLogic.Response.DTO;
using System.Collections.Generic;
using System.Linq;

namespace CloudLogic.Services.Trading
{
    public static class AuctionList
    {
        public static List<AuctionDTO> For(List<AHItem> auctions, string playerId)
        {
            return (auctions ?? new()).Select(a => new AuctionDTO
            {
                Id = a.Id,
                ItemId = a.ItemId,
                Price = a.Price,
                SoldItem = a.SoldItem,
                IsMine = a.OwnerId == playerId,
            }).ToList();
        }
    }
}
