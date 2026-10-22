using CloudLogic.RepoModels.Items;
using Google.Cloud.Firestore;

namespace CloudLogic.RepoModels.Skin
{
    [FirestoreData]
    public class SkinConfig : FireBaseEntity
    {
        [FirestoreProperty]
        public string SkinId { get; set; }

        [FirestoreProperty]
        public string Name { get; set; }

        [FirestoreProperty]
        public string Icon { get; set; }

        [FirestoreProperty]
        public ItemType ItemType { get; set; }

        [FirestoreProperty]
        public string WeaponType { get; set; }

        [FirestoreProperty]
        public string Description { get; set; }

        [FirestoreProperty]
        public long ShopCost { get; set; }
    }
}
