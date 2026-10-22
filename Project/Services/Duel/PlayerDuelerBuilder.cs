using CloudLogic.RepoModels.Duel;
using CloudLogic.RepoModels.Items;
using CloudLogic.RepoModels.Player;
using CloudLogic.Services.Duel.Spells;
using CloudLogic.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CloudLogic.Services.Duel
{
    public static class PlayerDuelerBuilder
    {
        private static readonly string[] Keystones = { DuelKeystone.Exposed, DuelKeystone.Unarmored, DuelKeystone.Verdict, DuelKeystone.Revive };

        public static Dueler Build(string name, PlayerData player, IEnumerable<Item> equipped)
        {
            var items = equipped.Where(i => i != null).ToList();
            var weapon = items.FirstOrDefault(i => i.Type == ItemType.Weapon);
            int level = (int)Math.Max(1, player.HeroLevel);
            var dueler = new Dueler
            {
                Id = player.Id,
                Name = name,
                IsNpc = false,
                Level = level,
                MainWeapon = weapon?.WeaponName ?? "Sword",
                Spells = (player.DuelSpells ?? new List<int>()).Where(DuelSpellBook.IsCastable).Distinct().Take(DuelSpellBook.MaxSpells).ToList(),
                Stance = DuelStance.Clean(player.DuelStance),
                Stats = new DuelerStats
                {
                    Health = GameConstants.DuelBaseHealth,
                    CritChance = GameConstants.DuelBaseCritChance,
                    AttackSpeed = weapon != null && weapon.AttackSpeed > 0 ? weapon.AttackSpeed : 1,
                    Armor = items.Sum(i => i.Armor)
                }
            };

            dueler.Stats.DamageMin = Math.Max(GameConstants.DuelMinDamage, weapon?.DamageMin ?? 0);
            dueler.Stats.DamageMax = Math.Max(dueler.Stats.DamageMin, weapon?.DamageMax ?? 0);
            float critPower = 0;

            foreach (var stat in items.Where(i => i.Stats != null).SelectMany(i => i.Stats))
            {
                critPower += AddStat(dueler.Stats, stat.Name, stat.Value);
            }

            foreach (var legendary in items.Where(i => i.LegendaryStat != null).SelectMany(i => i.LegendaryStat))
            {
                AddLegendary(dueler, legendary.Key, legendary.Value);
            }

            dueler.Stats.CritDamage = 1.5f + Math.Min(300f, critPower / Math.Max(1f, 100f * level) * 300f) / 100f;
            return dueler;
        }

        private static float AddStat(DuelerStats stats, string name, int value)
        {
            switch (name)
            {
                case nameof(StatEnum.Health): stats.Health += value; break;
                case nameof(StatEnum.HealthPerSecond): stats.HealthPerSecond += value; break;
                case nameof(StatEnum.AditionalDamage): stats.AdditionalDamage += value; break;
                case nameof(StatEnum.PhysicalDamage): stats.AddPhysDmg += value; break;
                case nameof(StatEnum.MagicalDamage): stats.AddMagicDmg += value; break;
                case nameof(StatEnum.CriticalChance): stats.CritChance += value; break;
                case nameof(StatEnum.Dodge): stats.Dodge += value; break;
                case nameof(StatEnum.Luck): stats.Luck += value; break;
                case nameof(StatEnum.CooldownReduction): stats.CooldownReduction += value; break;
                case nameof(StatEnum.MoveSpeed): stats.MoveSpeed += value; break;
                case nameof(StatEnum.CritPower): return value;
            }

            return 0;
        }

        private static void AddLegendary(Dueler dueler, string key, int value)
        {
            if (Keystones.Contains(key))
            {
                dueler.Keystones.Add(key);
                return;
            }

            switch (key)
            {
                case "IncreasePhysicalDamage": dueler.Stats.AddPhysDmg += value; break;
                case "IncreaseMagicalDamage": dueler.Stats.AddMagicDmg += value; break;
                case "AdditionalProjectile": dueler.Stats.AdditionalProjectiles += value; break;
                case "SpellLevel": dueler.SpellLevel += 1; break;
                case "TwoSpellLevel": dueler.SpellLevel += 2; break;
            }
        }
    }
}
