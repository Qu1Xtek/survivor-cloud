using Google.Cloud.Firestore;
using System.Collections.Generic;

namespace CloudLogic.RepoModels.Achievements
{
    [FirestoreData]
    public class AchievementProgress : FireBaseEntity
    {
        /// <summary>
        /// Per-player progress document. Recommended doc id = PlayerId.
        /// </summary>
        [FirestoreProperty]
        public string PlayerId { get; set; }

        /// <summary>
        /// Canonical stat counters, e.g.:
        /// { "craft_uncommon_count": 7, "monsters_slain": 1234 }
        /// </summary>
        [FirestoreProperty]
        public Dictionary<string, long> Stats { get; set; } = new Dictionary<string, long>();

        /// <summary>
        /// Completed flags per achievement key (threshold definitions), e.g.:
        /// { "craft_5_uncommon": true, "craft_10_uncommon": true }
        /// </summary>
        [FirestoreProperty]
        public Dictionary<string, bool> CompletedAchievements { get; set; } = new Dictionary<string, bool>();

        // Backwards-compatible fields from the old per-achievement layout.
        // Kept to avoid breaking existing serialized data / callers abruptly.
        [FirestoreProperty]
        public string AchievementKey { get; set; }

        [FirestoreProperty]
        public long Progress { get; set; }

        [FirestoreProperty]
        public bool Completed { get; set; }

        [FirestoreProperty]
        public Timestamp CompletedAt { get; set; }

        [FirestoreProperty]
        public Timestamp UpdatedAt { get; set; }
    }
}

