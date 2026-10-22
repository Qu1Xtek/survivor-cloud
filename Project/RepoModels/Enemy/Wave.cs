using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudLogic.RepoModels.Enemy
{
    [FirestoreData]
    [Serializable]
    public class Wave
    {
        [FirestoreProperty]
        public List<EnemyDefinition> Enemies {  get; set; }

        [FirestoreProperty]
        public float CamRange {  get; set; }

        [FirestoreProperty]
        public bool IsBossWave { get; set; }
    }
}
