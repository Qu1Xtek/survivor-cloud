using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;

namespace CloudLogic.RepoModels.Enemy
{
    [FirestoreData]
    [Serializable]
    public class EnemyDefinition
    {
        [FirestoreProperty]
        public string AName { get; set; }

        [FirestoreProperty]
        public Dictionary<string, int> WeaponDamages {  get; set; }

        [FirestoreProperty]
        public int TouchDamage { get; set; }

        [FirestoreProperty]
        public int Special1Damage { get; set; }

        [FirestoreProperty]
        public int Special2Damage { get; set; }

        [FirestoreProperty]
        public int Health { get; set; }

        [FirestoreProperty]
        public int TotalNumber {  get; set; }

        [FirestoreProperty]
        public int MaxNumber {  get; set; }

        [FirestoreProperty]
        public int BatchSize {  get; set; }

        [FirestoreProperty]
        public float SpawnTime {  get; set; }

        [FirestoreProperty]
        public float SpawnAfter {  get; set; }

        [FirestoreProperty]
        public bool Boss { get; set; }

        public int AliveEnemies { get; set; }
        public int EnemiesSpawned { get; set; }
    }
}
