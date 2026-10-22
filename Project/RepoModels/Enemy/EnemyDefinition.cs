using Google.Cloud.Firestore;
using System;

namespace CloudLogic.RepoModels.Enemy
{
    [FirestoreData]
    [Serializable]
    public class BaseEnemyStats
    {
        [FirestoreProperty]
        public int Health { get; set; }

        [FirestoreProperty]
        public int TouchDamage { get; set; }

        [FirestoreProperty]
        public int Special1Damage { get; set; }

        [FirestoreProperty]
        public int Special2Damage { get; set; }

        /// <summary>
        /// Difficulty tier this enemy's Health is derived from (EasyTrash, Medium, Heavy, ...).
        /// Null/empty means no tier is assigned and Health is left untouched by tier-based tooling.
        /// </summary>
        [FirestoreProperty]
        public string Tier { get; set; }
    }
}
