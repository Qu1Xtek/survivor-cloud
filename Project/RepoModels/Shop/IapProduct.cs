using CloudLogic.RepoModels.PlayerRewards;
using Google.Cloud.Firestore;
using System.Collections.Generic;

namespace CloudLogic.RepoModels.Shop
{
    /// <summary>
    /// A real-money product shown in the IAP shop. The document id is the store product id.
    /// </summary>
    [FirestoreData]
    public class IapProduct : FireBaseEntity
    {
        [FirestoreProperty]
        public string Title { get; set; }

        [FirestoreProperty]
        public int Amount { get; set; }

        [FirestoreProperty]
        public string Price { get; set; }

        [FirestoreProperty]
        public string Icon { get; set; }

        [FirestoreProperty]
        public string Tag { get; set; }

        [FirestoreProperty]
        public string Description { get; set; }

        [FirestoreProperty]
        public List<Reward> Rewards { get; set; }

        [FirestoreProperty]
        public bool OneTime { get; set; }

        [FirestoreProperty]
        public int MinStage { get; set; }
    }
}
