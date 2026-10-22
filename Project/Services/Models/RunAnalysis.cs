using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;

namespace CloudLogic.Services.Models
{
    [FirestoreData]
    [Serializable]
    public class RunAnalysis : FireBaseEntity
    {
        [FirestoreProperty]
        public string PlayerId { get; set; }

        [FirestoreProperty]
        public int RunLength { get; set; }

        [FirestoreProperty]
        public float RunEffectiveTime { get; set; }

        [FirestoreProperty]
        public Dictionary<string, WeaponStatistic> Weapons { get; set; } = new Dictionary<string, WeaponStatistic>();

        [FirestoreProperty]
        public int HeroLevelReached { get; set; }

        [FirestoreProperty]
        public long TotalRunDamage { get; set; }

        [FirestoreProperty]
        public int TotalGoldEarned { get; set; }

        [FirestoreProperty]
        public int TotalItemsDropped { get; set; }

        [FirestoreProperty]
        public int TotalItemsPickedUp { get; set; }

        [FirestoreProperty]
        public int TotalEpicsDropped { get; set; }

        [FirestoreProperty]
        public int TotalLegendariesDropped { get; set; }

        [FirestoreProperty]
        public int SelectedLevel { get; set; }

        [FirestoreProperty]
        public int TotalDeaths { get; set; }

        [FirestoreProperty]
        public float StartedTime { get; set; }

        [FirestoreProperty]
        public Timestamp DateCreatedStamp { get; set; }

        public DateTime CreationDate => DateCreatedStamp.ToDateTime();
    }
}
