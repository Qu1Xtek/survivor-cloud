using CloudLogic.RepoModels.Items;
using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;

namespace CloudLogic.RepoModels.PlayerRewards
{
    [FirestoreData]
    public class Reward
    {
        [FirestoreProperty]
        public RewardType RewardType { get; set; }

        [FirestoreProperty]
        public long Value { get; set; }

        [FirestoreProperty]
        public RarityType Rarity { get; set; }


        [FirestoreProperty]
        public Dictionary<string, long>? GuaranteedRarities { get; set; }

        public Dictionary<RarityType, long>? GetGuaranteedRaritiesTyped()
        {
            if (GuaranteedRarities == null) return null;
            var dict = new Dictionary<RarityType, long>();
            foreach (var kv in GuaranteedRarities)
            {
                if (Enum.TryParse<RarityType>(kv.Key, ignoreCase: false, out var rarity))
                    dict[rarity] = kv.Value;
            }
            return dict;
        }

        [FirestoreProperty]
        public long[] LevelRange { get; set; }

        [FirestoreProperty]
        public ItemType Slot { get; set; }

        [FirestoreProperty]
        public int ItemLevel { get; set; }

        public long GetRandomLevel()
        {
            return new Random().NextInt64(LevelRange[0], LevelRange[LevelRange.Length - 1]);
        }
    }
}
