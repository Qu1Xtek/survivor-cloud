using CloudLogic.RepoModels.Items;
using Google.Cloud.Firestore;

namespace CloudLogic.RepoModels.Skin
{
    [FirestoreData]
    public class Skin : FireBaseEntity
    {
        [FirestoreProperty]
        public string PlayerId { get; set; }

        [FirestoreProperty]
        public string SkinId { get; set; }

        [FirestoreProperty]
        public string SkinName { get; set; }

        [FirestoreProperty]
        public ItemType ItemType { get; set; }

        [FirestoreProperty]
        public string ShopPurchaseRef { get; set; }

        [FirestoreProperty]
        public Timestamp DateCreated { get; set; }
    }
}
