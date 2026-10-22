using CloudLogic.RepoModels.Items;
using Google.Cloud.Firestore;

namespace CloudLogic.RepoModels.AuctionHouse
{
    [FirestoreData]
    public class AHItem : FireBaseEntity
    {
        [FirestoreProperty]
        public string ItemId { get; set; }

        [FirestoreProperty]
        public string OwnerId { get; set; }

        [FirestoreProperty]
        public int Price { get; set; }

        public Item SoldItem { get; set; }
    }
}
