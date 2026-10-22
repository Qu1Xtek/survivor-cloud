using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudLogic.RepoModels.Player
{
    [FirestoreData]
    public class EquippedSkins
    {
        [FirestoreProperty]
        public string Head { get; set; }
        [FirestoreProperty]
        public string Chest { get; set; }

        [FirestoreProperty]
        public string Belt { get; set; }

        [FirestoreProperty]
        public string Gloves { get; set; }

        [FirestoreProperty]
        public string Legs { get; set; }

        [FirestoreProperty]
        public string Boots { get; set; }

        [FirestoreProperty]
        public string Sword { get; set; }

        [FirestoreProperty]
        public string Staff { get; set; }

        [FirestoreProperty]
        public string Bow { get; set; }

    }
}
