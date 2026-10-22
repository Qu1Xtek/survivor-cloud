using CloudLogic.RepoModels.Duel;
using System;
using System.Linq;

namespace CloudLogic.Services.Duel.Arena
{
    public static class ArenaTurns
    {
        public const float SecondsPerTurn = 4.8f;
        public const float WalkShare = 0.4f;
        public const float OpeningDelay = 0.7f;

        public static void Take(ArenaState state)
        {
            var actor = Next(state);
            state.Advance(actor.NextTurnAt);

            if (state.Over)
            {
                return;
            }

            state.LastActor = actor.Index;
            state.Record(DuelEventType.Turn, actor, null, (int)actor.UltCharge);
            DuelEffects.TurnStart(state, actor);

            if (state.Over)
            {
                return;
            }

            ArenaMovement.Move(state, actor);
            bool acted = !state.Over && ArenaCaster.TryCast(state, actor);
            actor.NextTurnAt = state.Time + Length(actor) * (acted ? 1 : WalkShare);
        }

        private static ArenaFighter Next(ArenaState state)
        {
            return state.Fighters
                .OrderBy(f => f.NextTurnAt)
                .ThenBy(f => f.Index == state.LastActor ? 1 : 0)
                .First();
        }

        private static float Length(ArenaFighter fighter)
        {
            return SecondsPerTurn / (float)Math.Max(0.3, fighter.Stats.AttackSpeed);
        }
    }
}
