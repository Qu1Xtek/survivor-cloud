using Google.Cloud.Firestore;

namespace CloudLogic.RepoModels
{
    [FirestoreData]
    public class Feedback : FireBaseEntity
    {
        [FirestoreProperty]
        public string PlayerId { get; set; }

        [FirestoreProperty]
        public string OptionalInfo { get; set; }

        [FirestoreProperty]
        public string FeedbackText { get; set; }

        [FirestoreProperty]
        public int Type { get; set; }

        [FirestoreProperty]
        public Timestamp DateCreated { get; set; }
    }
}
