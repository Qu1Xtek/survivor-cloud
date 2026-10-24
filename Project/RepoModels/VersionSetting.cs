using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudLogic.RepoModels
{
    [FirestoreData]
    public class VersionSetting
    {

        [FirestoreProperty]
        public string EnemiesVersion { get; set; }

        [FirestoreProperty]
        public int AndroidVersion { get; set; }

        [FirestoreProperty]
        public int iOSVersion { get; set; }

        [FirestoreProperty]
        public string GemConfigVersion { get; set; }

        [FirestoreProperty]
        public int MinutesPerEnergy { get; set; }

        [FirestoreProperty]
        public List<string> TesterAccounts { get; set; }

        [FirestoreProperty]
        public bool IsIAPShopFeatureActiveiOS { get; set; }

        [FirestoreProperty]
        public bool IsIAPShopFeatureActiveAndroid { get; set; }

        [FirestoreProperty]
        public int DefaultStashSize { get; set; }

        [FirestoreProperty]
        public int DaggerMinVersion { get; set; }

        [FirestoreProperty]
        public int DaggerDropChance { get; set; }

        [FirestoreProperty]
        public bool IsSkinsFeatureActiveiOS { get; set; }

        [FirestoreProperty]
        public bool IsSkinsFeatureActiveAndroid { get; set; }
    }
}
