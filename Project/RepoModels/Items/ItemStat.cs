using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudLogic.RepoModels.Items
{
    [FirestoreData]
    public class ItemStat
    {
        [FirestoreProperty]

        public string Name { get; set; }

        [FirestoreProperty]

        public int Value { get; set; }

        [FirestoreProperty]

        public bool Altered { get; set; } = false;

        [FirestoreProperty]

        public bool Reforged { get; set; } = false;

        [FirestoreProperty] 
        public int[] Range { get; set; }
    }
}
