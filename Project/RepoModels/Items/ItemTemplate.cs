using Google.Cloud.Firestore;
using System.Collections.Generic;

namespace CloudLogic.RepoModels.Items
{
    [FirestoreData]
    public class ItemTemplate : FireBaseEntity
    {
        [FirestoreProperty]
        public string ItemId { get; set; }

        [FirestoreProperty]
        public string Name { get; set; }

        [FirestoreProperty]
        public int Level { get; set; }

        [FirestoreProperty]
        public ItemType Type { get; set; }

        [FirestoreProperty]
        public RarityType Rarity { get; set; }

        [FirestoreProperty]
        public List<int> MaxDamageRange { get; set; }

        [FirestoreProperty]
        public List<double> DamageDeltaRange { get; set; }

        [FirestoreProperty]
        public List<int> AdditionalDamageRange { get; set; }

        [FirestoreProperty]
        public List<int> HealthRange { get; set; }

        [FirestoreProperty]
        public List<int> ArmorRange { get; set; }

        [FirestoreProperty]
        public List<int> CriticalChanceRange { get; set; }

        [FirestoreProperty]
        public List<int> CriticalDamageRange { get; set; }

        [FirestoreProperty]
        public List<int> CritPowerRange { get; set; }

        [FirestoreProperty]
        public string Icon { get; set; }

        [FirestoreProperty]
        public string WeaponName { get; set; }

        [FirestoreProperty]
        public string ModelName { get; set; }

        [FirestoreProperty]
        public List<double> AttackSpeedRange { get; set; }

        [FirestoreProperty]
        public List<int> DodgeRange { get; set; }

        [FirestoreProperty]
        public List<int> LuckRange { get; set; }

        [FirestoreProperty]
        public List<int> PhysicalDamageRange { get; set; }

        [FirestoreProperty]
        public List<int> MagicalDamageRange { get; set; }

        [FirestoreProperty]
        public List<int> AdditionalRangeRange { get; set; }

        [FirestoreProperty]
        public List<int> HealthPerSecondRange { get; set; }

        [FirestoreProperty]
        public List<int> CooldownReductionRange { get; set; }

        [FirestoreProperty]
        public List<int> MoveSpeedRange { get; set; }

        [FirestoreProperty]
        public Dictionary<int, string> RarityOptions { get; set; }

        [FirestoreProperty]
        public Dictionary<string, List<int>>? LegendaryStatOptions { get; set; }
    }
}
