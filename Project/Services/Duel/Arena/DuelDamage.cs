using CloudLogic.RepoModels.Duel;
using CloudLogic.Services.Duel.Spells;
using System;

namespace CloudLogic.Services.Duel.Arena
{
    public static class DuelDamage
    {
        public const float BlockAbsorb = 0.5f;
        public const float MaxArmorReduction = 0.7f;
        public const int ArmorPerLevel = 40;

        public static void Strike(ArenaState state, ArenaFighter attacker, DuelSpell spell)
        {
            var defender = state.Enemy(attacker);

            if (!defender.Alive)
            {
                return;
            }

            if (!spell.IsUltimate && !defender.Dueler.Has(DuelKeystone.Exposed) && state.Roll() < defender.Stats.Dodge)
            {
                state.Record(DuelEventType.Dodge, attacker, spell);
                return;
            }

            bool crit = RollCrit(state, attacker);
            float amount = Raw(state, attacker, spell) * (crit ? attacker.Stats.CritDamage : 1);
            amount *= 1 + spell.ExecuteBonus * (1 - defender.HpPercent);
            amount = Mitigate(attacker, defender, amount);

            int dealt = Math.Max(1, (int)MathF.Ceiling(amount * state.DamageMultiplier));
            DuelOutcome.Damage(state, attacker, dealt, DuelEventType.Hit, spell, crit);
            DuelEffects.Apply(state, attacker, defender, spell, dealt);
        }

        public static float Raw(ArenaState state, ArenaFighter attacker, DuelSpell spell)
        {
            var stats = attacker.Stats;
            int weaponDamage = state.Rng.Next(Math.Max(1, stats.DamageMin), Math.Max(stats.DamageMin, stats.DamageMax) + 1);
            return DuelSpellStats.Damage(attacker.Dueler, spell, weaponDamage);
        }

        private static bool RollCrit(ArenaState state, ArenaFighter attacker)
        {
            if (attacker.Dueler.Has(DuelKeystone.Verdict))
            {
                return attacker.HpPercent < 0.2f;
            }

            return state.Roll() < attacker.Stats.CritChance;
        }

        private static float Mitigate(ArenaFighter attacker, ArenaFighter defender, float amount)
        {
            if (defender.Dueler.Has(DuelKeystone.Exposed))
            {
                amount *= 1.5f;
            }

            if (!defender.Dueler.Has(DuelKeystone.Unarmored))
            {
                amount *= 1 - ArmorReduction(defender.Stats.Armor, attacker.Dueler.Level);
            }

            if (defender.BlockCharges > 0)
            {
                defender.BlockCharges--;
                amount *= 1 - BlockAbsorb;
            }

            return amount;
        }

        public static float ArmorReduction(float armor, int attackerLevel)
        {
            if (armor <= 0)
            {
                return 0;
            }

            return Math.Min(MaxArmorReduction, armor / (armor + 10 + ArmorPerLevel * Math.Max(1, attackerLevel)));
        }
    }
}
