using System;
using System.Collections.Generic;

namespace CloudLogic.RepoModels.Items
{
    public static class DuelDagger
    {
        public const int FirstTier = 5;
        public const RarityType MaxRarity = RarityType.Epic;
        public const int HealthPerLevel = 5;

        public static int NextTier(int level)
        {
            if (level < FirstTier)
            {
                return FirstTier;
            }

            if (level < 25)
            {
                return level + 5;
            }

            return level < 34 ? level + 3 : level + 1;
        }

        public static List<int> Droppable(int highestOwned, int mapLevel)
        {
            var tiers = new List<int>();
            int cap = Math.Min(mapLevel, NextTier(highestOwned));

            for (int level = FirstTier; level <= cap; level = NextTier(level))
            {
                tiers.Add(level);
            }

            return tiers;
        }

        public static ItemTemplate At(ItemTemplate dagger, int level, RarityType rarity)
        {
            return new ItemTemplate
            {
                Id = dagger.Id,
                Name = dagger.Name,
                Type = ItemType.DuelWeapon,
                Level = level,
                Rarity = rarity,
                Icon = dagger.Icon,
                WeaponName = dagger.WeaponName,
                ModelName = dagger.ModelName,
                AttackSpeedRange = dagger.AttackSpeedRange,
                RarityOptions = dagger.RarityOptions,
                HealthRange = new List<int> { 1, HealthPerLevel * level },
                CriticalChanceRange = new List<int> { 1, MaxCrit(level) }
            };
        }

        private static int MaxCrit(int level)
        {
            if (level < 10)
            {
                return (int)(level * 1.5);
            }

            return level < 20 ? 20 : 30;
        }
    }
}
