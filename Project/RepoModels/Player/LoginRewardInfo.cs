
using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudLogic.RepoModels.Player
{
    [FirestoreData]
    public class LoginRewardInfo
    {
        [FirestoreProperty]
        public int ConcecutiveLoginCount { get; set; }
        [FirestoreProperty]

        public int Week {  get; set; }
        [FirestoreProperty]

        public int Year { get; set; }
        [FirestoreProperty]

        public Timestamp LastClaimDate {  get; set; }

        public DateTime LastClaimTime => LastClaimDate.ToDateTime();
    }
}
