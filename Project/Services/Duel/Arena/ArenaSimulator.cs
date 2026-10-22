using CloudLogic.RepoModels.Duel;
using System;
using System.Linq;

namespace CloudLogic.Services.Duel.Arena
{
    public static class ArenaSimulator
    {
        public static DuelReplay Run(Dueler a, Dueler b, int seed)
        {
            var state = new ArenaState(a, b, seed);
            var fighters = state.Fighters.Select(Describe).ToList();

            while (!state.Over)
            {
                ArenaTurns.Take(state);
            }

            return new DuelReplay
            {
                Seed = seed,
                Duration = MathF.Round(state.Time, 2),
                Winner = DuelOutcome.Winner(state),
                Fighters = fighters,
                Events = state.Events
            };
        }

        private static DuelReplayFighter Describe(ArenaFighter fighter)
        {
            return new DuelReplayFighter
            {
                Name = fighter.Dueler.Name,
                ModelName = fighter.Dueler.ModelName,
                MainWeapon = fighter.Loadout[0].Name,
                IsNpc = fighter.Dueler.IsNpc,
                Level = fighter.Dueler.Level,
                MaxHp = (int)fighter.MaxHp,
                Spells = fighter.Loadout.Skip(1).Select(s => s.Id).ToList(),
                X = fighter.Pos.X,
                Y = fighter.Pos.Y
            };
        }
    }
}
