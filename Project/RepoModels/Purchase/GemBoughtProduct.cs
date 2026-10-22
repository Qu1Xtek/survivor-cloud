using Google.Cloud.Firestore;

namespace CloudLogic.RepoModels.Purchase
{
    [FirestoreData]
    public class GemBoughtProduct : FireBaseEntity
    {
        public const string Revive = "revive";

        [FirestoreProperty] public string PlayerId { get; set; }
        [FirestoreProperty] public string ProductId { get; set; }
        [FirestoreProperty] public long Cost { get; set; }
        [FirestoreProperty] public long GemsAfter { get; set; }
        [FirestoreProperty] public Timestamp CreatedAt { get; set; }
    }
}
