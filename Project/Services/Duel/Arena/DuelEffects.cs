using CloudLogic.RepoModels.Duel;
using CloudLogic.Services.Duel.Spells;
using System;

namespace CloudLogic.Services.Duel.Arena
{
    public static class DuelEffects
    {
        public const int MaxPoisonStacks = 8;
        public const float PoisonStackPower = 0.35f;
        public const int DotTurnTicks = 3;
        public const float ChillTurnDelay = 0.5f;

        public static void Apply(ArenaState state, ArenaFighter attacker, ArenaFighter defender, DuelSpell spell, int dealt)
        {
            float t = state.Time;
            float duration = spell.EffectDuration;

            switch (spell.Effect)
            {
                case DuelEffect.Stun:
                    defender.NextTurnAt += Added(defender.StunnedUntil, t, duration);
                    defender.StunnedUntil = Math.Max(defender.StunnedUntil, t + duration);
                    break;
                case DuelEffect.Chill:
                    defender.NextTurnAt += Added(defender.ChilledUntil, t, duration) * spell.EffectValue * ChillTurnDelay;
                    defender.ChilledUntil = t + duration;
                    defender.ChillValue = spell.EffectValue;
                    break;
                case DuelEffect.Slow:
                    defender.SlowedUntil = t + duration;
                    defender.SlowValue = spell.EffectValue;
                    break;
                case DuelEffect.Burn:
                    float burn = dealt * spell.EffectValue;
                    defender.BurnPerTick = defender.BurnUntil > t ? Math.Max(defender.BurnPerTick, burn) : burn;
                    defender.BurnUntil = t + duration;
                    defender.BurnSpell = spell;
                    break;
                default:
                    return;
            }

            state.Record(DuelEventType.Effect, attacker, spell, dur: duration);
        }

        public static void AddPoison(ArenaState state, ArenaFighter attacker, DuelSpell spell)
        {
            var defender = state.Enemy(attacker);
            bool active = defender.PoisonUntil > state.Time;
            float perStack = DuelDamage.Raw(state, attacker, spell) * PoisonStackPower;
            defender.PoisonStacks = active ? Math.Min(MaxPoisonStacks, defender.PoisonStacks + 1) : 1;
            defender.PoisonPerStack = active ? Math.Max(defender.PoisonPerStack, perStack) : perStack;
            defender.PoisonUntil = state.Time + spell.EffectDuration;
            defender.PoisonSpell = spell;
            state.Record(DuelEventType.Effect, attacker, spell, defender.PoisonStacks, dur: spell.EffectDuration);
        }

        public static void TurnStart(ArenaState state, ArenaFighter victim)
        {
            var source = state.Enemy(victim);
            float t = state.Time;

            if (victim.BurnUntil > t && victim.BurnPerTick > 0)
            {
                Tick(state, source, victim.BurnPerTick, victim.BurnSpell);
            }

            if (victim.PoisonUntil > t && victim.PoisonStacks > 0)
            {
                Tick(state, source, victim.PoisonStacks * victim.PoisonPerStack, victim.PoisonSpell);
            }
            else
            {
                victim.PoisonStacks = 0;
            }
        }

        private static void Tick(ArenaState state, ArenaFighter source, float perTick, DuelSpell spell)
        {
            if (state.Enemy(source).Alive)
            {
                DuelOutcome.Damage(state, source, Math.Max(1, (int)(perTick * DotTurnTicks * state.DamageMultiplier)), DuelEventType.Dot, spell);
            }
        }

        private static float Added(float until, float t, float duration)
        {
            return Math.Max(0, t + duration - Math.Max(until, t));
        }
    }
}
