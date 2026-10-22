using CloudLogic.RepoModels.PlayerRewards;
using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudLogic.RepoModels
{
    [FirestoreData]
    public class DailyLoginWeek : FireBaseEntity
    {
        [FirestoreProperty]
        public string Name { get; set; }

        [FirestoreProperty]
        public List<string> QuestIds { get; set; }

        public List<Quest> Quests { get; set; }

        [FirestoreProperty]
        public bool Active { get; set; }

    }
}
