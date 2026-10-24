using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace CloudLogic.RepoModels.Items
{
    public class ItemStatGenerator
    {
        public static int GetMaxWeaponDamage(List<int> statRange)
        {
            var tier = GetWeaponTier();
            var dmgDelta = (statRange[1] - statRange[0]) * tier;

            return GetRandomValue(new List<int> { statRange[0], ((int)dmgDelta + statRange[0]) });
        }

        public static float GetWeaponTier()
        {
            var tierCheck = GetRandomValue(new List<int> { 0, 1000 });

            if (tierCheck > 900)
            {
                return 1;
            }
            else if (tierCheck > 750)
            {
                return 0.8f;
            }
            else if (tierCheck > 500)
            {
                return 0.6f;
            }
            else if (tierCheck > 250)
            {
                return 0.5f;
            }
            else
            {
                return 0.35f;
            }
        }

        private static ItemStat GenerateStatFromRange(string name, List<int> statRange)
        {
            var safeRange = (statRange != null && statRange.Count >= 2)
                ? new int[] { statRange[0], statRange[1] }
                : new int[] { 0, 0 };

            return new ItemStat()
            {
                Name = name,
                Value = GetRandomValue(statRange),
                Range = safeRange
            };
        }

        private static int GetStatFromRange(List<int> statRange)
        {
            return GetRandomValue(statRange);
        }

        private static int GetRandomValue(List<int> statRange)
        {
            if (statRange == null || statRange.Count < 2)
            {
                return 0;
            }

            var min = statRange[0];
            var max = statRange[1];

            if (max < min)
            {
                (min, max) = (max, min);
            }

            return new Random().Next(min, max + 1);
        }

        private static double GetStatFromRange(List<double> statRange)
        {
            if (statRange != null && statRange.Count >= 2)
            {
                var random = new Random();
                double min = statRange[0];
                double max = statRange[1];
                return min + (random.NextDouble() * (max - min));
            }
            return 0;
        }

        public static List<Item> GenerateInitialWeapons()
        {
            var items = new List<Item>(3)
            {
                new Item
                {
                    Name = "First Sword",
                    Type = ItemType.Weapon,
                    DamageMax = 65,
                    DamageMin = 45,
                    AttackSpeed = 2.2,
                    Rarity = 0,
                    GoldValue = 1,
                    Icon = "Sword",
                    WeaponName = "Sword",
                    Level = 1,
                    Stats = new List<ItemStat>(),
                },
                new Item
                {
                    Name = "First Bow",
                    Type = ItemType.Weapon,
                    DamageMax = 65,
                    DamageMin = 45,
                    AttackSpeed = 2,
                    Rarity = 0,
                    GoldValue = 0,
                    Icon = "Bow",
                    WeaponName = "Bow",
                    Level = 1,
                    Stats = new List<ItemStat>(),
                },
                new Item
                {
                    Name = "First Staff",
                    Type = ItemType.Weapon,
                    DamageMax = 65,
                    DamageMin = 45,
                    AttackSpeed = 1.8,
                    Rarity = 0,
                    GoldValue = 1,
                    Icon = "Staff",
                    WeaponName = "Staff",
                    Level = 1,
                    Stats = new List<ItemStat>(),
                }
            };


            return items;
        }

        public static Item GenerateStats(ItemTemplate itemTemplate, RarityType rollRarity)
        {
            // Use the name for the rolled rarity if the template has one; otherwise the template's own name.
            var itemName = itemTemplate.RarityOptions != null && itemTemplate.RarityOptions.ContainsKey((int)rollRarity)
                ? itemTemplate.RarityOptions[(int)rollRarity]
                : itemTemplate.Name;

            Item item = new Item()
            { 
                Name = itemName,
                Icon = itemTemplate.Icon,
                ModelName = itemTemplate.ModelName,
                Type = itemTemplate.Type,
                Rarity = rollRarity,
                ItemTemplateId = itemTemplate.Id,
                Stats = new List<ItemStat>(),
                GoldValue = new Random().Next(itemTemplate.Level, (2 * itemTemplate.Level) + 1),
                Level = itemTemplate.Level
            };


            if (!IsWeapon(itemTemplate.Type))
            {
                item.Armor = GetStatFromRange(itemTemplate.ArmorRange);

            }
            else
            {
                AddInitialWeaponStats(item, itemTemplate, rollRarity);
            }

            switch (rollRarity)
            {
                case RarityType.Common:
                    AddStat(item, itemTemplate, 1);
                    break;
                case RarityType.Uncommon:
                    AddStat(item, itemTemplate, 2);
                    item.GoldValue = (int)(item.GoldValue * 1.25);
                    break;
                case RarityType.Rare:
                    AddStat(item, itemTemplate, 3);
                    item.GoldValue = (int)(item.GoldValue * 1.75);
                    break;
                case RarityType.Epic:
                    AddStat(item, itemTemplate, 4);
                    item.GoldValue = (int)(item.GoldValue * 2.75);
                    break;
                case RarityType.Legendary:
                    AddStat(item, itemTemplate, 5);
                    item.GoldValue = (int)(item.GoldValue * 4.25);
                    break;
            }

            var legendaryStat = TryAddLegendaryStat(itemTemplate, rollRarity);


            if (legendaryStat != null)
            {
                item.LegendaryStat = legendaryStat;
            }

            return item;
        }


        private static void AddInitialWeaponStats(Item item, ItemTemplate itemTemplate, RarityType rollRarity)
        {
            // MaxDamage and delta are computed purely from level + rolled rarity - the
            // template's MaxDamageRange / DamageDeltaRange are not used. One base weapon
            // template therefore covers every rarity, like armor.
            var maxDamageRange = GetWeaponMaxDamageRange(itemTemplate.Level, rollRarity);

            if (rollRarity > RarityType.Rare)
            {
                item.DamageMax += GetMaxWeaponDamage(maxDamageRange);
            }
            else
            {
                item.DamageMax += GetStatFromRange(maxDamageRange);
            }

            item.DamageMin += (int)(item.DamageMax * GetStatFromRange(GetWeaponDeltaRange(rollRarity)));

            item.AttackSpeed = GetStatFromRange(itemTemplate.AttackSpeedRange);

            item.WeaponName = itemTemplate.WeaponName;

            item.Icon = itemTemplate.Icon;
        }

        // Weapon MaxDamage curve (mirrors DFAdmin WeaponScalingConfig):
        //   common = level <= 20 ? 45 * 1.3^level : 45 * 1.3^20 + flat bonus per level over 20
        //   curve  = common * 1.15^rarity
        // Stored/displayed range is [curve*0.7*0.7, curve*0.7] (e.g. L30 Legendary -> [20184, 28835]).
        private static List<int> GetWeaponMaxDamageRange(int level, RarityType rollRarity)
        {
            double common = level <= 20
                ? 45.0 * Math.Pow(1.3, level)
                : (45.0 * Math.Pow(1.3, 20)) + WeaponLevelBonus(level);

            double curve = common * Math.Pow(1.15, (int)rollRarity);

            int max = (int)Math.Round(curve * 0.7);
            int min = (int)Math.Round(max * 0.7);

            return new List<int> { min, max };
        }

        // Levels above 20 add flat damage: +1000 on odd levels, +2000 on even.
        private static double WeaponLevelBonus(int level)
        {
            double bonus = 0;

            for (int lvl = 21; lvl <= level; lvl++)
            {
                bonus += lvl % 2 == 1 ? 1000 : 2000;
            }

            return bonus;
        }

        // Min/max damage ratio: min fixed at 0.5, max widens +0.1 per rarity tier (capped 0.9).
        // Common [0.5,0.6] .. Epic/Legendary [0.5,0.9].
        private static List<double> GetWeaponDeltaRange(RarityType rollRarity)
        {
            return new List<double>
            {
                0.5,
                Math.Min(0.6 + (0.1 * (int)rollRarity), 0.9)
            };
        }

        private static void AddStat(Item item, ItemTemplate itemTemplate, int statCount)
        {
            var itemStats = new Dictionary<string, List<int>>
                {
                    { StatEnum.Health.ToString(), itemTemplate.HealthRange},
                    { StatEnum.AditionalDamage.ToString(), itemTemplate.AdditionalDamageRange},
                    //{ StatEnum.CriticalDamage.ToString(), itemTemplate.CriticalDamageRange},
                    { StatEnum.HealthPerSecond.ToString(), itemTemplate.HealthPerSecondRange},
                    { StatEnum.Dodge.ToString(), itemTemplate.DodgeRange},
                    { StatEnum.Luck.ToString(), itemTemplate.LuckRange},
                    { StatEnum.AdditionalRange.ToString(), itemTemplate.AdditionalRangeRange},
                    { StatEnum.CooldownReduction.ToString(), itemTemplate.CooldownReductionRange},
                    { StatEnum.PhysicalDamage.ToString(), itemTemplate.PhysicalDamageRange},
                    { StatEnum.MagicalDamage.ToString(), itemTemplate.MagicalDamageRange},
                    { StatEnum.MoveSpeed.ToString(), itemTemplate.MoveSpeedRange},
                    { StatEnum.CritPower.ToString(), itemTemplate.CritPowerRange},
                };

            if (IsWeapon(itemTemplate.Type))
            {
                itemStats.Add(StatEnum.CriticalChance.ToString(), itemTemplate.CriticalChanceRange);
            }

            AddItemStat(item, itemStats, statCount);
        }


        public static Item AlterStat(Item item, ItemTemplate itemTemplate, int statNum, bool isSuper)
        {
            return GetAlteredItem(item, itemTemplate, statNum, ItemStatUtil.PossibleStatsPerType[itemTemplate.Type], isSuper);
        }

        public static Item GetReforgedItem(Item item, ItemTemplate itemTemplate, int statNum)
        {
            var currentStat = item.Stats[statNum];



            List<int> statRange = GetRangeByStatName(currentStat.Name, itemTemplate);

            var newValue = GetRandomValue(statRange);


            item.Stats[statNum].Value = newValue;

            return item;
        }
        private static Item GetAlteredItem(Item item, ItemTemplate itemTemplate, int statNum, List<string> possibleStats, bool super = false)
        {
            var currentStat = item.Stats[statNum];

            // Get all possible weapon stats except the current one
            string newStatName;

            if (!super)
            {
                var filteredStats = possibleStats
                    .Where(stat => stat != currentStat.Name)
                    .ToList();
                var alreadyUsedStats = item.Stats.Select(s => s.Name).ToHashSet();

                filteredStats = filteredStats.Where(stat => !alreadyUsedStats.Contains(stat)).ToList();

                if (filteredStats.Count == 0)
                    return item;

                // Pick a different stat at random
                newStatName = filteredStats[new Random().Next(filteredStats.Count)];
            }
            else
            {
                newStatName = possibleStats[new Random().Next(possibleStats.Count)];
            }

            List<int> statRange = GetRangeByStatName(newStatName, itemTemplate);


            item.Stats[statNum].Name = newStatName;
            item.Stats[statNum].Value = GetRandomValue(statRange);
            item.Stats[statNum].Altered = true;

            return item;
        }

        private static Dictionary<string, int> TryAddLegendaryStat(ItemTemplate itemTemplate, RarityType rarity)
        {
            if (rarity == RarityType.Legendary
                && itemTemplate.LegendaryStatOptions != null
                && itemTemplate.LegendaryStatOptions.Count > 0)
            {
                var rnd = new Random();
                var stat = itemTemplate
                    .LegendaryStatOptions
                    .ElementAt(rnd.Next(itemTemplate.LegendaryStatOptions.Count));

                return new Dictionary<string, int>
                {
                    { stat.Key, rnd.Next(stat.Value[0], stat.Value[1]) }
                };
            }

            return null;
        }

        private static bool IsWeapon(ItemType type)
        {
            return type == ItemType.Weapon || type == ItemType.DuelWeapon;
        }

        private static List<int> GetRangeByStatName(string statName, ItemTemplate itemTemplate)
        {
            return statName switch
            {
                nameof(StatEnum.Health) => itemTemplate.HealthRange,
                nameof(StatEnum.HealthPerSecond) => itemTemplate.HealthPerSecondRange,
                nameof(StatEnum.Dodge) => itemTemplate.DodgeRange,
                nameof(StatEnum.Luck) => itemTemplate.LuckRange,
                nameof(StatEnum.PhysicalDamage) => itemTemplate.PhysicalDamageRange,
                nameof(StatEnum.MagicalDamage) => itemTemplate.MagicalDamageRange,
                nameof(StatEnum.AdditionalRange) => itemTemplate.AdditionalRangeRange,
                nameof(StatEnum.AditionalDamage) => itemTemplate.AdditionalDamageRange,
                nameof(StatEnum.CritPower) => itemTemplate.CritPowerRange,
                nameof(StatEnum.CriticalChance) => itemTemplate.CriticalChanceRange,
                nameof(StatEnum.CooldownReduction) => itemTemplate.CooldownReductionRange,
                nameof(StatEnum.MoveSpeed) => itemTemplate.MoveSpeedRange,
                _ => null
            };
        }

        private static void AddItemStat(Item item, Dictionary<string, List<int>> itemStats, int remainingStats)
        {
            //var statKeys = itemStats.Keys;
            var statKeys = itemStats.Keys.OrderBy(x => new Random().Next()).ToList();

            foreach (var statName in statKeys)
            {
                if (remainingStats == 0)
                {
                    break;
                }

                if (itemStats[statName] == null || itemStats[statName].Count == 0 || itemStats[statName][0] == 0)
                {
                    continue;
                }


                item.Stats.Add(GenerateStatFromRange(statName, itemStats[statName]));
                remainingStats--;
            }
        }
    }
}
