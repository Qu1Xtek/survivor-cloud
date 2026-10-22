using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudLogic.RepoModels.Achievements
{
    [FirestoreData]
    public class QuestsProgress : AchievementProgress
    {
        [FirestoreProperty]
        public string DayKey { get; set; }
    }
}
