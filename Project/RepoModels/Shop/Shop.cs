using CloudLogic.RepoModels.PlayerRewards;
using Google.Cloud.Firestore;

namespace CloudLogic.RepoModels.Shop
{
    [FirestoreData]
    public class Shop : FireBaseEntity
    {
        [FirestoreProperty]
        public Reward Reward { get; set; }

        [FirestoreProperty]
        public long Cost { get; set; }

        [FirestoreProperty]
        public string Title { get; set; }

        [FirestoreProperty]
        public string Description { get; set; }

        [FirestoreProperty]
        public string Tag { get; set; }

        [FirestoreProperty]
        public int DailyLimit { get; set; }

        [FirestoreProperty]
        public int Discount { get; set; }
    }
}

