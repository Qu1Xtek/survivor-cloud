using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudLogic.RepoModels.Items
{
    public class ItemStatUtil
    {
        public static readonly List<string> PossibleHeadStats = new List<string>
                {
                    { StatEnum.Health.ToString() },
                    { StatEnum.Dodge.ToString()},
                    { StatEnum.Luck.ToString() },
                    { StatEnum.PhysicalDamage.ToString() },
                    { StatEnum.MagicalDamage.ToString() },
                    { StatEnum.AdditionalRange.ToString() }
                };

        public static readonly List<string> PossibleChestStats = new List<string>
                {
                    { StatEnum.Health.ToString() },
                    { StatEnum.HealthPerSecond.ToString()},
                    { StatEnum.Luck.ToString() },
                    { StatEnum.Dodge.ToString() },
                    { StatEnum.AdditionalRange.ToString() }
                };

        public static readonly List<string> PossibleLegsStats = new List<string>
                {
                    { StatEnum.Health.ToString() },
                    { StatEnum.HealthPerSecond.ToString()},
                    { StatEnum.Luck.ToString() },
                    { StatEnum.Dodge.ToString() },
                    { StatEnum.AdditionalRange.ToString() }
                };

        public static readonly List<string> PossibleBeltStats = new List<string>
                {
                    { StatEnum.Health.ToString() },
                    { StatEnum.Dodge.ToString()},
                    { StatEnum.Luck.ToString() },
                    { StatEnum.PhysicalDamage.ToString() },
                    { StatEnum.MagicalDamage.ToString() },
                    { StatEnum.AdditionalRange.ToString() }
                };

        public static readonly List<string> PossibleBootsStats = new List<string>
                {
                    { StatEnum.Health.ToString() },
                    { StatEnum.Dodge.ToString()},
                    { StatEnum.Luck.ToString() },
                    { StatEnum.MoveSpeed.ToString() },
                    { StatEnum.AdditionalRange.ToString() }
                };

        public static readonly List<string> PossibleAmuletStats = new List<string>
                {
                    { StatEnum.PhysicalDamage.ToString() },
                    { StatEnum.MagicalDamage.ToString()},
                    { StatEnum.CooldownReduction.ToString() },
                    { StatEnum.CritPower.ToString() },
                    { StatEnum.AditionalDamage.ToString() },
                    { StatEnum.Luck.ToString() },
                    { StatEnum.Health.ToString() }
                };

        public static readonly List<string> PossibleRingStats = new List<string>
                {
                    { StatEnum.PhysicalDamage.ToString() },
                    { StatEnum.MagicalDamage.ToString()},
                    { StatEnum.CooldownReduction.ToString() },
                    { StatEnum.CritPower.ToString() },
                    { StatEnum.AditionalDamage.ToString() },
                    { StatEnum.Luck.ToString() },
                    { StatEnum.Health.ToString() }
                };

        public static readonly List<string> PossibleGlovesStats = new List<string>
                {
                    { StatEnum.PhysicalDamage.ToString() },
                    { StatEnum.MagicalDamage.ToString()},
                    { StatEnum.CritPower.ToString() },
                    { StatEnum.AditionalDamage.ToString() },
                    { StatEnum.Luck.ToString() },
                    { StatEnum.Health.ToString() }
                };

        public static readonly List<string> PossibleBracerStats = new List<string>
                {
                    { StatEnum.Dodge.ToString() },
                    { StatEnum.CritPower.ToString() },
                    { StatEnum.AditionalDamage.ToString() },
                    { StatEnum.Luck.ToString() },
                    { StatEnum.Health.ToString() }
                };

        public static readonly List<string> PossibleWeaponStats = new List<string>
                {
                    { StatEnum.CriticalChance.ToString() },
                    { StatEnum.CritPower.ToString() },
                    { StatEnum.Luck.ToString() },
                    { StatEnum.CooldownReduction.ToString() },
                    { StatEnum.Dodge.ToString() },
                    { StatEnum.Health.ToString() },
                    { StatEnum.HealthPerSecond.ToString() },
                    { StatEnum.PhysicalDamage.ToString() },
                    { StatEnum.MagicalDamage.ToString() },
                    { StatEnum.AdditionalRange.ToString() },
                    { StatEnum.AditionalDamage.ToString() },
                };

        public static readonly List<string> PossibleDuelWeaponStats = new List<string>
                {
                    { StatEnum.Health.ToString() },
                    { StatEnum.CriticalChance.ToString() },
                };

        public static readonly Dictionary<ItemType, List<string>> PossibleStatsPerType = new Dictionary<ItemType, List<string>>()
        {
            {ItemType.Weapon, PossibleWeaponStats},
            {ItemType.DuelWeapon, PossibleDuelWeaponStats},
            {ItemType.Head, PossibleHeadStats},
            {ItemType.Chest, PossibleChestStats},
            {ItemType.Legs, PossibleLegsStats},
            {ItemType.Belt, PossibleBeltStats},
            {ItemType.Boots, PossibleBootsStats},
            {ItemType.Amulet, PossibleAmuletStats},
            {ItemType.Ring, PossibleRingStats},
            {ItemType.Hands, PossibleGlovesStats},
            {ItemType.Bracer, PossibleBracerStats},

        };
    }
}
