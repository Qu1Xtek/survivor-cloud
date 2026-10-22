using CloudLogic.RepoModels.Duel;
using System.Numerics;

namespace CloudLogic.Services.Duel.Arena
{
    public static class ArenaMovement
    {
        public const float WalkSpeed = 2.5f;
        public const float MeleeStep = 3f;
        public const float RangedStep = 2f;
        public const float ChillSlow = 0.7f;
        public const float PersonalSpace = 0.8f;
        public const float MinMove = 0.05f;

        public static void Move(ArenaState state, ArenaFighter fighter)
        {
            if (ArenaDash.TryLeap(state, fighter) || ArenaCaster.HasCloseShot(state, fighter))
            {
                return;
            }

            var target = ArenaIntent.Step(state, fighter, Budget(state, fighter));

            if (target.HasValue)
            {
                WalkTo(state, fighter, target.Value);
            }
        }

        public static void WalkTo(ArenaState state, ArenaFighter fighter, Vector2 target)
        {
            var enemy = state.Enemy(fighter).Pos;
            target = ArenaState.Clamp(target, 0);

            if (Vector2.Distance(target, enemy) < PersonalSpace)
            {
                target = enemy + ArenaState.Direction(target - enemy) * PersonalSpace;
            }

            if (Vector2.Distance(target, fighter.Pos) < MinMove)
            {
                return;
            }

            fighter.Pos = target;
            state.Record(DuelEventType.Move, fighter, null);
        }

        public static float Budget(ArenaState state, ArenaFighter fighter)
        {
            return (fighter.Melee ? MeleeStep : RangedStep) * Multiplier(state, fighter);
        }

        public static float Speed(ArenaState state, ArenaFighter fighter)
        {
            return WalkSpeed * Multiplier(state, fighter);
        }

        private static float Multiplier(ArenaState state, ArenaFighter fighter)
        {
            float chill = fighter.Chilled(state.Time) ? ChillSlow : 1;
            float slow = fighter.Slowed(state.Time) ? 1 - fighter.SlowValue : 1;
            return (1 + fighter.Stats.MoveSpeed / 100f) * chill * slow;
        }
    }
}
