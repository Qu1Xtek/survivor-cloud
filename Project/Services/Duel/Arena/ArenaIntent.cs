using CloudLogic.Services.Duel.Spells;
using System;
using System.Numerics;

namespace CloudLogic.Services.Duel.Arena
{
    public static class ArenaIntent
    {
        public const float EdgeMargin = 0.6f;
        public const float CircleStep = 0.45f;
        public const float HoldChance = 0.55f;
        public const float SafeMargin = 1.5f;
        private static readonly float[] RetreatSteps = { 0, MathF.PI / 4, MathF.PI / 2, MathF.PI * 3 / 4 };

        public static Vector2? Step(ArenaState state, ArenaFighter fighter, float budget)
        {
            var target = Decide(state, fighter);

            if (!target.HasValue)
            {
                return null;
            }

            var dir = target.Value - fighter.Pos;
            return dir.Length() <= budget ? target : fighter.Pos + Vector2.Normalize(dir) * budget;
        }

        private static Vector2? Decide(ArenaState state, ArenaFighter fighter)
        {
            var enemy = state.Enemy(fighter);
            var fromEnemy = fighter.Pos - enemy.Pos;
            float distance = Math.Max(0.01f, fromEnemy.Length());
            var (min, max) = Band(fighter);
            float mid = (min + max) / 2;

            if (distance > max)
            {
                return enemy.Pos + fromEnemy / distance * mid;
            }

            if (distance < min)
            {
                return Retreat(enemy.Pos, fromEnemy / distance, mid, state);
            }

            return state.Rng.NextDouble() < HoldChance ? null : Circle(enemy.Pos, fromEnemy, state.Rng.Next(2) == 0 ? -1 : 1);
        }

        public static Vector2 SafeSpot(ArenaState state, ArenaFighter fighter)
        {
            var enemy = state.Enemy(fighter);
            var away = ArenaState.Direction(fighter.Pos - enemy.Pos);
            return Retreat(enemy.Pos, away, Band(fighter).Max + SafeMargin, state);
        }

        private static (float Min, float Max) Band(ArenaFighter fighter)
        {
            if (fighter.Melee)
            {
                return (0.9f, 1.8f);
            }

            return (Math.Max(2.5f, fighter.PreferredRange * 0.6f), fighter.PreferredRange);
        }

        private static Vector2 Retreat(Vector2 enemy, Vector2 away, float distance, ArenaState state)
        {
            float sign = state.Rng.Next(2) == 0 ? -1 : 1;

            foreach (float step in RetreatSteps)
            {
                foreach (float side in new[] { sign, -sign })
                {
                    var target = enemy + Rotate(away, side * step) * distance;

                    if (ArenaState.Inside(target, EdgeMargin))
                    {
                        return target;
                    }
                }
            }

            return Inside(enemy + away * distance);
        }

        private static Vector2 Circle(Vector2 enemy, Vector2 fromEnemy, float sign)
        {
            return Inside(enemy + Rotate(fromEnemy, sign * CircleStep));
        }

        private static Vector2 Rotate(Vector2 v, float angle)
        {
            float cos = MathF.Cos(angle);
            float sin = MathF.Sin(angle);
            return new Vector2(v.X * cos - v.Y * sin, v.X * sin + v.Y * cos);
        }

        private static Vector2 Inside(Vector2 pos)
        {
            return ArenaState.Clamp(pos, EdgeMargin);
        }
    }
}
