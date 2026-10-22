using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudLogic.RepoModels.PlayerRewards
{
    [FirestoreData]
    public class Quest : FireBaseEntity
    {
        [FirestoreProperty]
        public Condition Condition { get; set; }


        [FirestoreProperty]
        public Reward Reward { get; set; }  
    }
}
