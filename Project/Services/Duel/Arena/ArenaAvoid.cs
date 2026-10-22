using CloudLogic.Services.Duel.Spells;
using System;
using System.Numerics;

namespace CloudLogic.Services.Duel.Arena
{
    public static class ArenaAvoid
    {
        public const float MinReaction = 0.2f;
        public const float MaxReaction = 0.6f;
        public const float ProjectileRadius = 1.1f;
        public const float Clearance = 0.3f;

        public static bool Try(ArenaState state, ArenaFighter caster, DuelSpell spell, float flight)
        {
            var defender = state.Enemy(caster);

            if (!Avoidable(spell) || defender.Stunned(state.Time))
            {
                return false;
            }

            var aim = defender.Pos;
            float radius = Radius(spell);
            float reaction = MinReaction + (float)state.Rng.NextDouble() * (MaxReaction - MinReaction);
            float reach = Math.Max(0, flight - reaction) * ArenaMovement.Speed(state, defender);

            ArenaMovement.WalkTo(state, defender, Sidestep(caster.Pos, aim, Math.Min(reach, radius + Clearance)));
            return Vector2.Distance(defender.Pos, aim) > radius;
        }

        public static float Radius(DuelSpell spell)
        {
            return spell.Shape == DuelShape.Projectile ? ProjectileRadius : spell.Radius;
        }

        private static bool Avoidable(DuelSpell spell)
        {
            if (spell.IsUltimate || spell.Homing)
            {
                return false;
            }

            return spell.Shape == DuelShape.Projectile || spell.Shape == DuelShape.TargetArea;
        }

        private static Vector2 Sidestep(Vector2 from, Vector2 aim, float distance)
        {
            var line = ArenaState.Direction(aim - from);
            var side = new Vector2(-line.Y, line.X);

            foreach (var dir in new[] { side, -side })
            {
                var target = aim + dir * distance;

                if (ArenaState.Inside(target, 0))
                {
                    return target;
                }
            }

            return aim + side * distance;
        }
    }
}
