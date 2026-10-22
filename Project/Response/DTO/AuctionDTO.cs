using CloudLogic.RepoModels.Items;

namespace CloudLogic.Response.DTO
{
    public class AuctionDTO
    {
        public string Id { get; set; }
        public string ItemId { get; set; }
        public int Price { get; set; }
        public Item SoldItem { get; set; }
        public bool IsMine { get; set; }
    }
}
