using Google.Cloud.Firestore;

namespace CloudLogic.RepoModels.PlayerRewards
{
    [FirestoreData]
    public class ClaimedReward : FireBaseEntity
    {
        [FirestoreProperty]
        public string PlayerId { get; set; }

        [FirestoreProperty]
        public long ClaimedAtLevel { get; set; }

        [FirestoreProperty]
        public Reward Value { get; set; }        

        [FirestoreProperty]
        public string SpecificInfo { get; set; }

        [FirestoreProperty]
        public Timestamp ClaimedAt { get; set; }
    }
}
