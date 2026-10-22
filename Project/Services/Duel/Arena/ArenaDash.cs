using CloudLogic.RepoModels.Duel;
using CloudLogic.Services.Duel.Spells;
using System.Linq;
using System.Numerics;

namespace CloudLogic.Services.Duel.Arena
{
    public static class ArenaDash
    {
        public const float EscapeDistance = 2f;
        public const float MinEscapeGain = 1f;
        public const float LandGap = 1.2f;

        public static bool TryLeap(ArenaState state, ArenaFighter fighter)
        {
            var step = fighter.Loadout.FirstOrDefault(s => s.IsMobility);

            if (step == null || !fighter.Ready(step, state.Time))
            {
                return false;
            }

            var enemy = state.Enemy(fighter);
            var target = fighter.Melee ? Chase(state, fighter, enemy, step) : Escape(state, fighter, enemy);

            if (!target.HasValue)
            {
                return false;
            }

            fighter.Use(step, state.Time);
            fighter.Pos = target.Value;
            state.Record(DuelEventType.Cast, fighter, step, at: fighter.Pos);

            if (fighter.Melee)
            {
                DuelDamage.Strike(state, fighter, step);
            }

            return true;
        }

        public static Vector2 Landing(ArenaFighter fighter, ArenaFighter enemy)
        {
            return ArenaState.Clamp(enemy.Pos + ArenaState.Direction(fighter.Pos - enemy.Pos) * LandGap, 0);
        }

        private static Vector2? Chase(ArenaState state, ArenaFighter fighter, ArenaFighter enemy, DuelSpell step)
        {
            float distance = Vector2.Distance(fighter.Pos, enemy.Pos);
            bool walkable = distance - fighter.Loadout[0].Reach <= ArenaMovement.Budget(state, fighter);

            if (walkable || distance > step.Range)
            {
                return null;
            }

            return Landing(fighter, enemy);
        }

        private static Vector2? Escape(ArenaState state, ArenaFighter fighter, ArenaFighter enemy)
        {
            float distance = Vector2.Distance(fighter.Pos, enemy.Pos);
            var spot = ArenaIntent.SafeSpot(state, fighter);

            if (distance >= EscapeDistance || Vector2.Distance(spot, enemy.Pos) < distance + MinEscapeGain)
            {
                return null;
            }

            return spot;
        }
    }
}
