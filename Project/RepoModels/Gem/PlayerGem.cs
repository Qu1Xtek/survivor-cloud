using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudLogic.RepoModels.Gem
{
    [FirestoreData]
    public class PlayerGem
    {
        [FirestoreProperty]
        public GemType GemType { get; set; }

        [FirestoreProperty]
        public int Level {  get; set; }

        [FirestoreProperty]
        public List<GemPower> PowerValues { get; set; } = new List<GemPower>();

        [FirestoreProperty]
        public int EquipSlot { get; set; }
    }
}
