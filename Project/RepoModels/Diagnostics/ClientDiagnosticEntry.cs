using Google.Cloud.Firestore;

namespace CloudLogic.RepoModels.Diagnostics
{
    [FirestoreData]
    public class ClientDiagnosticEntry : FireBaseEntity
    {
        [FirestoreProperty]
        public string PlayerId { get; set; }

        [FirestoreProperty]
        public string Stage { get; set; }

        [FirestoreProperty]
        public string Reason { get; set; }

        [FirestoreProperty]
        public string Platform { get; set; }

        [FirestoreProperty]
        public string AppVersion { get; set; }

        [FirestoreProperty]
        public float Time { get; set; }

        [FirestoreProperty]
        public Timestamp CreatedAt { get; set; }
    }
}
