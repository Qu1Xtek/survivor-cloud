using Google.Cloud.Firestore;
using System.Collections.Generic;

namespace CloudLogic.RepoModels
{
    [FirestoreData]
    public class DailyMissionSelected : FireBaseEntity
    {
        [FirestoreProperty]
        public List<string> MissionsSelected { get; set; } = new List<string>();
    }
}
