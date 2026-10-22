using Google.Cloud.Firestore;
using System.Collections.Generic;

namespace CloudLogic.RepoModels.Items
{
    [FirestoreData]
    public class Item : FireBaseEntity
    {
        [FirestoreProperty]
        public string ItemTemplateId { get; set; }

        [FirestoreProperty]
        public string OwnerId { get; set; }

        [FirestoreProperty]
        public string Name { get; set; }

        [FirestoreProperty]
        public ItemType Type { get; set; }

        [FirestoreProperty]
        public int Level { get; set; }

        [FirestoreProperty]
        public string WeaponName { get; set; }

        [FirestoreProperty]
        public RarityType Rarity { get; set; }

        [FirestoreProperty]
        public string Icon { get; set; }

        [FirestoreProperty]
        public int GoldValue { get; set; }

        [FirestoreProperty]
        public string ModelName { get; set; }

        [FirestoreProperty]
        public Timestamp DroppedAt { get; set; }

        [FirestoreProperty]
        public List<ItemStat> Stats { get; set; }

        [FirestoreProperty]
        public int AlteredTimes { get; set; }

        [FirestoreProperty]
        public int ReforgedTimes {  get; set; }

        [FirestoreProperty]
        public int Armor { get; set; }

        [FirestoreProperty]
        public int DamageMin { get; set; }

        [FirestoreProperty]
        public int DamageMax { get; set; }

        [FirestoreProperty]
        public double AttackSpeed { get; set; }

        [FirestoreProperty]
        public int Damage => (DamageMin + DamageMax) / 2;

        [FirestoreProperty]
        public Dictionary<string, int>? LegendaryStat { get; set; }

        public string AdditionalInfo { get; set; }
    }
}
