using Google.Cloud.Firestore;

namespace CloudLogic.RepoModels.Duel
{
    [FirestoreData]
    public class DuelerStats
    {
        [FirestoreProperty] public int Health { get; set; }
        [FirestoreProperty] public float HealthPerSecond { get; set; }
        [FirestoreProperty] public int Armor { get; set; }
        [FirestoreProperty] public int Dodge { get; set; }
        [FirestoreProperty] public int DamageMin { get; set; }
        [FirestoreProperty] public int DamageMax { get; set; }
        [FirestoreProperty] public double AttackSpeed { get; set; } = 1;
        [FirestoreProperty] public int AdditionalDamage { get; set; }
        [FirestoreProperty] public int AddPhysDmg { get; set; }
        [FirestoreProperty] public int AddMagicDmg { get; set; }
        [FirestoreProperty] public int CritChance { get; set; }
        [FirestoreProperty] public float CritDamage { get; set; } = 1.5f;
        [FirestoreProperty] public float CooldownReduction { get; set; }
        [FirestoreProperty] public float MoveSpeed { get; set; }
        [FirestoreProperty] public int Luck { get; set; }
        [FirestoreProperty] public float Lifesteal { get; set; }
        [FirestoreProperty] public int AdditionalProjectiles { get; set; }
    }
}
