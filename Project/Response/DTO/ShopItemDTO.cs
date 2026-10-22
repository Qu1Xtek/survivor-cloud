using CloudLogic.RepoModels.PlayerRewards;

namespace CloudLogic.Response.DTO
{
    public class ShopItemDTO
    {
        public string Id { get; set; }
        public ShopItemType Type { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public string Tag { get; set; }
        public long Cost { get; set; }
        public long FullCost { get; set; }
        public string Price { get; set; }
        public Reward Reward { get; set; }
        public int DailyLimit { get; set; }
        public int BoughtToday { get; set; }
        public long ActiveUntil { get; set; }
    }
}
