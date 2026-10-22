using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudLogic.RepoModels.Gem
{
    [FirestoreData]
    public class GemConfig
    {
        [FirestoreProperty]
        public string GemType { get; set; }

        [FirestoreProperty]
        public float LevelIncrement { get; set; }
    }
}
