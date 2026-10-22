using CloudLogic.RepoModels.PlayerRewards;
using Google.Cloud.Firestore;

namespace CloudLogic.RepoModels.Achievements
{
    [FirestoreData]
    public class Achievement : FireBaseEntity
    {
        [FirestoreProperty]
        public string Key { get; set; }

        [FirestoreProperty]
        public string StatKey { get; set; }

        [FirestoreProperty]
        public string Title { get; set; }

        [FirestoreProperty]
        public string Description { get; set; }

        [FirestoreProperty]
        public AchievementType Type { get; set; }

        [FirestoreProperty]
        public long TargetValue { get; set; }

        [FirestoreProperty]
        public bool IsActive { get; set; } = true;

        [FirestoreProperty]
        public bool IsHidden { get; set; } = false;

        [FirestoreProperty]
        public int SortOrder { get; set; }

        [FirestoreProperty]
        public Reward Reward { get; set; }

        [FirestoreProperty]
        public long RewardValue { get; set; }

        [FirestoreProperty]
        public string IconKey { get; set; }
    }
}

