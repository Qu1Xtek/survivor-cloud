using Google.Cloud.Firestore;
using System.Collections.Generic;

namespace CloudLogic.RepoModels.Player
{
    [FirestoreData]
    public class RunData
    {

        [FirestoreProperty]
        public int ItemsDropped { get; set; }

        [FirestoreProperty]
        public int ItemsPicked { get; set; }

        [FirestoreProperty]
        public List<long> GoldDropped { get; set; } = new List<long>();

        [FirestoreProperty]
        public List<long> GoldPicked { get; set; } = new List<long>();

        [FirestoreProperty]
        public int MaxGoldDrops { get; set; }

        [FirestoreProperty]
        public int MaxItemDrops { get; set; }

        [FirestoreProperty]
        public int CraftingMaterialsDropped {  get; set; }

        [FirestoreProperty]
        public int CraftingMaterialsPicked {  get; set; }

        [FirestoreProperty]
        public Timestamp LastRunCompletedAt { get; set; }

        // Set once the run's rewards are granted (normal collect OR ad-double),
        // so they can never be claimed twice.
        [FirestoreProperty]
        public bool RunClaimed { get; set; }
    }
}
