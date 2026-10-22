using Google.Cloud.Firestore;
using System;

namespace CloudLogic.RepoModels
{
    [FirestoreData]
    public class BugReport : FireBaseEntity
    {
        [FirestoreProperty]
        public string PlayerReportId { get; set; }

        [FirestoreProperty]
        public string BugInfo { get; set; }

        [FirestoreProperty]
        public string BugDescription { get; set; }

        [FirestoreProperty]
        public Timestamp DateCreated { get; set; }

        [FirestoreProperty]
        public bool Solved { get; set; } = false;
    }
}
