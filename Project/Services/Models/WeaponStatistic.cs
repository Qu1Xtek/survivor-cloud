using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;

namespace CloudLogic.Services.Models
{
    [FirestoreData]
    [Serializable]
    public class WeaponStatistic
    {
        [FirestoreProperty]
        public string WeaponName { get; set; }

        [FirestoreProperty]
        public long TotalDamageDone { get; set; }

        [FirestoreProperty]
        public long TotalHitsMade { get; set; }

        [FirestoreProperty]
        public long MaxDamage { get; set; }

        [FirestoreProperty]
        public long MinDamage { get; set; }

        [FirestoreProperty]
        public long EvolvedAt { get; set; }

        [FirestoreProperty]
        public long TakenAt { get; set; }

        [FirestoreProperty]
        public int LevelReached { get; set; }

        [FirestoreProperty]
        public Dictionary<long, string> WeaponUpgradesTaken { get; set; } = new Dictionary<long, string>();
    }
}
