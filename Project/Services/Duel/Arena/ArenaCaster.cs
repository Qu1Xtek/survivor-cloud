using CloudLogic.RepoModels.Duel;
using CloudLogic.Services.Duel.Spells;
using System;
using System.Linq;
using System.Numerics;

namespace CloudLogic.Services.Duel.Arena
{
    public static class ArenaCaster
    {
        public const float ZoneTick = 0.5f;
        public const int MaxLuckChance = 30;

        public static bool TryCast(ArenaState state, ArenaFighter caster)
        {
            var spell = Choose(state, caster);

            if (spell == null)
            {
                return false;
            }

            if (spell.IsUltimate)
            {
                caster.UltCharge = 0;
            }
            else
            {
                caster.Use(spell, state.Time);
            }

            Cast(state, caster, spell);
            return true;
        }

        public static bool HasCloseShot(ArenaState state, ArenaFighter caster)
        {
            float distance = Vector2.Distance(caster.Pos, state.Enemy(caster).Pos);
            return caster.Loadout.Any(s => s.IsClose && caster.Ready(s, state.Time) && distance <= s.Reach);
        }

        private static DuelSpell Choose(ArenaState state, ArenaFighter caster)
        {
            float distance = Vector2.Distance(caster.Pos, state.Enemy(caster).Pos);

            if (caster.UltimateReady && distance <= caster.Ultimate.Reach)
            {
                return caster.Ultimate;
            }

            return caster.Loadout.Skip(1).Append(caster.Loadout[0])
                .Where(s => !s.IsMobility && caster.Ready(s, state.Time) && distance <= s.Reach)
                .OrderBy(caster.ReadyTime)
                .FirstOrDefault();
        }

        private static void Cast(ArenaState state, ArenaFighter caster, DuelSpell spell)
        {
            var enemy = state.Enemy(caster);
            var aim = ArenaShot.Target(caster, enemy, spell);
            float flight = ArenaShot.Flight(caster, enemy, spell);
            int hits = Hits(state, caster, spell);
            state.Record(DuelEventType.Cast, caster, spell, hits, at: aim, dur: flight);

            if (spell.Shape == DuelShape.Dash)
            {
                caster.Pos = aim;
            }

            bool avoided = ArenaAvoid.Try(state, caster, spell, flight);

            for (int i = 0; i < hits && enemy.Alive; i++)
            {
                Land(state, caster, spell, avoided, aim);
            }

            if (spell.Effect == DuelEffect.Block)
            {
                caster.BlockCharges += (int)spell.EffectValue;
                state.Record(DuelEventType.Effect, enemy, spell, caster.BlockCharges);
            }
        }

        private static void Land(ArenaState state, ArenaFighter caster, DuelSpell spell, bool avoided, Vector2 aim)
        {
            if (avoided)
            {
                state.Record(DuelEventType.Miss, caster, spell, at: aim);
            }
            else if (spell.Effect == DuelEffect.Poison)
            {
                DuelEffects.AddPoison(state, caster, spell);
            }
            else
            {
                DuelDamage.Strike(state, caster, spell);
            }
        }

        private static int Hits(ArenaState state, ArenaFighter caster, DuelSpell spell)
        {
            if (spell.Shape == DuelShape.Aura || spell.Shape == DuelShape.Zone)
            {
                return (int)MathF.Ceiling(spell.Duration / ZoneTick);
            }

            int hits = DuelSpellStats.Hits(caster.Dueler, spell);
            return state.Roll() < Math.Min(MaxLuckChance, caster.Stats.Luck) ? hits + 1 : hits;
        }
    }
}
