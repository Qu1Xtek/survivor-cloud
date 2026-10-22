using Google.Cloud.Firestore;

namespace CloudLogic.RepoModels.TrailingModels
{
    [FirestoreData]

    public class AuditData : FireBaseEntity
    {
        [FirestoreProperty]
        public int TotalItemsDropped { get; set; }

        [FirestoreProperty]
        public string PlayerId { get; set; }

        [FirestoreProperty]
        public Timestamp DateCreated { get; set; }
    }
}
