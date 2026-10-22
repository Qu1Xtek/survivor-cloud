using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;

namespace CloudLogic.RepoModels.Enemy
{
    [FirestoreData]
    [Serializable]
    public class WaveDetails : FireBaseEntity
    {
        [FirestoreProperty]
        public Wave[] Waves { get; set; }

        [FirestoreProperty]
        public int EnemyLevel { get; set; }

        [FirestoreProperty]
        public int LevelIndex { get; set; }

        [FirestoreProperty]
        public int MapIndex { get; set; }

        [FirestoreProperty]
        public int Thumbnail { get; set; }

        [FirestoreProperty]
        public string Name { get; set; }

        [FirestoreProperty]
        public Dictionary<string, BaseEnemyStats> UniqueNPCBases { get; set; }

    }
}
