using CloudLogic.RepoModels.Duel;
using CloudLogic.Services.Duel.Spells;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace CloudLogic.Services.Duel.Arena
{
    public class ArenaState
    {
        public const float HalfWidth = 4.5f;
        public const float HalfLength = 7.5f;
        public const float StartY = 6.5f;
        public const float SuddenDeathAt = 30f;
        public const float MaxTime = 60f;

        public ArenaFighter[] Fighters { get; }
        public Random Rng { get; }
        public float Time { get; private set; }
        public int LastActor { get; set; } = -1;
        public List<DuelEvent> Events { get; } = new List<DuelEvent>();

        public ArenaState(Dueler a, Dueler b, int seed)
        {
            Rng = new Random(seed);
            Fighters = new[]
            {
                new ArenaFighter(0, a, new Vector2(0, -StartY)),
                new ArenaFighter(1, b, new Vector2(0, StartY))
            };

            Fighters[1 - Rng.Next(2)].NextTurnAt = ArenaTurns.OpeningDelay;
        }

        public bool Over => !Fighters[0].Alive || !Fighters[1].Alive || Time >= MaxTime;

        public float DamageMultiplier => Time > SuddenDeathAt ? 1 + (Time - SuddenDeathAt) * 0.1f : 1;

        public ArenaFighter Enemy(ArenaFighter fighter)
        {
            return Fighters[1 - fighter.Index];
        }

        public void Advance(float time)
        {
            float elapsed = Math.Max(0, Math.Min(time, MaxTime) - Time);

            foreach (var fighter in Fighters)
            {
                DuelOutcome.Heal(fighter, fighter.Stats.HealthPerSecond * elapsed);
            }

            Time += elapsed;
        }

        public float Roll()
        {
            return (float)Rng.NextDouble() * 100f;
        }

        public static bool Inside(Vector2 pos, float margin)
        {
            return Math.Abs(pos.X) <= HalfWidth - margin && Math.Abs(pos.Y) <= HalfLength - margin;
        }

        public static Vector2 Clamp(Vector2 pos, float margin)
        {
            return new Vector2(Math.Clamp(pos.X, margin - HalfWidth, HalfWidth - margin), Math.Clamp(pos.Y, margin - HalfLength, HalfLength - margin));
        }

        public static Vector2 Direction(Vector2 v)
        {
            return v.LengthSquared() < 0.0001f ? new Vector2(0, 1) : Vector2.Normalize(v);
        }

        public void Record(string type, ArenaFighter src, DuelSpell spell, int amount = 0, bool crit = false, Vector2? at = null, float dur = 0)
        {
            var about = AboutSource(type) ? src : Enemy(src);
            var pos = at ?? about.Pos;

            Events.Add(new DuelEvent
            {
                T = MathF.Round(Time, 2),
                Type = type,
                Src = src.Index,
                Spell = spell?.Id ?? 0,
                Amount = amount,
                Hp = (int)MathF.Ceiling(about.Hp),
                Crit = crit,
                X = MathF.Round(pos.X, 2),
                Y = MathF.Round(pos.Y, 2),
                Dur = MathF.Round(dur, 2)
            });
        }

        private static bool AboutSource(string type)
        {
            return type == DuelEventType.Turn || type == DuelEventType.Move || type == DuelEventType.Heal || type == DuelEventType.Revive;
        }
    }
}
