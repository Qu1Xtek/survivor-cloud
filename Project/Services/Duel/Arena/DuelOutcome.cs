using CloudLogic.RepoModels.Duel;
using CloudLogic.Services.Duel.Spells;
using System;

namespace CloudLogic.Services.Duel.Arena
{
    public static class DuelOutcome
    {
        public const float ReviveHp = 0.3f;
        public const float DealCharge = 200f;
        public const float TakeCharge = 270f;

        public static void Damage(ArenaState state, ArenaFighter attacker, int amount, string type, DuelSpell spell, bool crit = false)
        {
            var defender = state.Enemy(attacker);
            defender.Hp = Math.Max(0, defender.Hp - amount);
            state.Record(type, attacker, spell, amount, crit);
            Leech(state, attacker, amount);
            Charge(attacker, defender, amount, spell);

            if (defender.Alive)
            {
                return;
            }

            if (defender.Dueler.Has(DuelKeystone.Revive) && !defender.ReviveUsed)
            {
                defender.ReviveUsed = true;
                defender.Hp = defender.MaxHp * ReviveHp;
                state.Record(DuelEventType.Revive, defender, null, (int)defender.Hp);
                return;
            }

            state.Record(DuelEventType.Death, attacker, spell);
        }

        public static void Heal(ArenaFighter fighter, float amount)
        {
            if (!fighter.Alive || amount <= 0 || fighter.Hp >= fighter.MaxHp)
            {
                return;
            }

            fighter.Hp = Math.Min(fighter.MaxHp, fighter.Hp + amount);
        }

        public static int Winner(ArenaState state)
        {
            var a = state.Fighters[0];
            var b = state.Fighters[1];

            if (a.Alive != b.Alive)
            {
                return a.Alive ? 0 : 1;
            }

            return a.HpPercent >= b.HpPercent ? 0 : 1;
        }

        private static void Charge(ArenaFighter attacker, ArenaFighter defender, int amount, DuelSpell spell)
        {
            if (spell != null && spell.IsUltimate)
            {
                return;
            }

            float share = amount / defender.MaxHp;
            attacker.UltCharge = Math.Min(DuelUltimates.FullCharge, attacker.UltCharge + share * DealCharge);
            defender.UltCharge = Math.Min(DuelUltimates.FullCharge, defender.UltCharge + share * TakeCharge);
        }

        private static void Leech(ArenaState state, ArenaFighter attacker, int amount)
        {
            if (attacker.Stats.Lifesteal <= 0)
            {
                return;
            }

            float healed = amount * attacker.Stats.Lifesteal / 100f;
            Heal(attacker, healed);

            if (healed >= 1)
            {
                state.Record(DuelEventType.Heal, attacker, null, (int)healed);
            }
        }
    }
}
