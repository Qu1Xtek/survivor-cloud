using Google.Cloud.Firestore;
using System.Collections.Generic;

namespace CloudLogic.RepoModels.Player
{
    [FirestoreData]
    public class PlayerInfo : FireBaseEntity
    {
        [FirestoreProperty] 
        public Timestamp LastLoginAt { get; set; }

        [FirestoreProperty]
        public Timestamp LastRunStartedAt { get; set; }

        [FirestoreProperty]
        public string PlayerName { get; set; }
    }
}
