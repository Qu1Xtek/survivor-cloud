using Google.Cloud.Firestore;
using System.Collections.Generic;

namespace CloudLogic.RepoModels.Duel
{
    [FirestoreData]
    public class Dueler : FireBaseEntity
    {
        [FirestoreProperty] public string Name { get; set; }
        [FirestoreProperty] public string Description { get; set; }
        [FirestoreProperty] public int Level { get; set; } = 1;
        [FirestoreProperty] public string ModelName { get; set; }
        [FirestoreProperty] public string MainWeapon { get; set; } = "Sword";
        [FirestoreProperty] public List<int> Spells { get; set; } = new List<int>();
        [FirestoreProperty] public int SpellLevel { get; set; } = 1;
        [FirestoreProperty] public List<string> Keystones { get; set; } = new List<string>();
        [FirestoreProperty] public int Stance { get; set; }
        [FirestoreProperty] public DuelerStats Stats { get; set; } = new DuelerStats();
        [FirestoreProperty] public int RewardGold { get; set; }
        [FirestoreProperty] public int RewardGems { get; set; }
        [FirestoreProperty] public int Order { get; set; }
        [FirestoreProperty] public bool IsActive { get; set; }

        public bool IsNpc { get; set; } = true;

        public bool Has(string keystone)
        {
            return Keystones != null && Keystones.Contains(keystone);
        }
    }
}
