using CloudLogic.RepoModels.Duel;
using CloudLogic.Services.Duel.Spells;
using System;

namespace CloudLogic.Services.Duel
{
    public static class DuelSpellStats
    {
        public static float Damage(Dueler dueler, DuelSpell spell, int weaponDamage)
        {
            var stats = dueler.Stats;
            float level = 1 + 0.15f * (Math.Max(1, dueler.SpellLevel) - 1);
            float damage = (spell.BaseDamage + weaponDamage * spell.DamageScale) * level;
            int bonus = stats.AdditionalDamage + (spell.IsMagical ? stats.AddMagicDmg : stats.AddPhysDmg);
            damage *= 1 + bonus / 100f;

            if (dueler.Has(DuelKeystone.Unarmored))
            {
                damage *= 1.3f;
            }

            if (dueler.Has(DuelKeystone.Exposed))
            {
                damage *= 1.4f;
            }

            return damage;
        }

        public static int Min(Dueler dueler, DuelSpell spell)
        {
            return (int)MathF.Ceiling(Damage(dueler, spell, Math.Max(1, dueler.Stats.DamageMin)));
        }

        public static int Max(Dueler dueler, DuelSpell spell)
        {
            return (int)MathF.Ceiling(Damage(dueler, spell, Math.Max(dueler.Stats.DamageMin, dueler.Stats.DamageMax)));
        }

        public static float Cooldown(Dueler dueler, DuelSpell spell)
        {
            return spell.IsWeapon
                ? spell.Cooldown / (float)Math.Max(0.3, dueler.Stats.AttackSpeed)
                : spell.Cooldown * (1 - Math.Min(50, dueler.Stats.CooldownReduction) / 100f);
        }

        public static int Hits(Dueler dueler, DuelSpell spell)
        {
            return spell.Hits + (spell.Shape == DuelShape.Projectile ? dueler.Stats.AdditionalProjectiles : 0);
        }
    }
}
