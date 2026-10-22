using CloudLogic.RepoModels.Items;

namespace CloudLogic.Response.DTO
{
    public class ItemDropDTO
    {
        public string Id { get; set; }
        public RarityType Rarity { get; set; }

        public bool Success { get; set; }

        public string AdditionalInfo { get; set; }
    }
}
