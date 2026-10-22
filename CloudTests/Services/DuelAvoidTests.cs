using CloudLogic.RepoModels.Duel;
using CloudLogic.Services.Duel.Spells;
using FluentAssertions;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace CloudTests.Services
{
    [TestClass]
    public sealed class DuelAvoidTests
    {
        private const float LongShot = 1.1f;
        private const float QuickShot = 0.6f;

        [TestMethod]
        public void OnlyStraightShotsAndBombs_AreAvoided()
        {
            var misses = Events(DuelArenaTests.Brute(), DuelArenaTests.Mage()).Where(e => e.Type == DuelEventType.Miss).ToList();

            misses.Should().NotBeEmpty();
            misses.Select(e => DuelSpellBook.Get(e.Spell)).Should().OnlyContain(s => !s.Homing && (s.Shape == DuelShape.Projectile || s.Shape == DuelShape.TargetArea));
        }

        [TestMethod]
        public void LongShots_AreAvoided_QuickShotsAreNot()
        {
            foreach (var replay in DuelArenaTests.Replays(DuelArenaTests.Sniper(), DuelArenaTests.Mage()))
            {
                var events = replay.Events;

                for (int i = 0; i < events.Count; i++)
                {
                    var cast = events[i];

                    if (cast.Type != DuelEventType.Cast || cast.Src != 0 || !IsStraight(cast.Spell))
                    {
                        continue;
                    }

                    bool missed = Outcomes(events, i).Any(e => e.Type == DuelEventType.Miss);

                    if (cast.Dur >= LongShot)
                    {
                        missed.Should().BeTrue("a {0}s shot gives time to step aside", cast.Dur);
                    }

                    if (cast.Dur < QuickShot)
                    {
                        missed.Should().BeFalse("a {0}s shot is too quick to step aside", cast.Dur);
                    }
                }
            }
        }

        [TestMethod]
        public void Sidesteps_NeverCloseTheDistance()
        {
            foreach (var replay in DuelArenaTests.Replays(DuelArenaTests.Sniper(), DuelArenaTests.Mage()))
            {
                for (int i = 1; i < replay.Events.Count; i++)
                {
                    var move = replay.Events[i];

                    if (move.Type != DuelEventType.Move || replay.Events[i - 1].Type != DuelEventType.Cast || replay.Events[i - 1].Src == move.Src)
                    {
                        continue;
                    }

                    var shooter = DuelArenaTests.PositionBefore(replay, i, 1 - move.Src);
                    var before = DuelArenaTests.PositionBefore(replay, i, move.Src);

                    Vector2.Distance(new Vector2(move.X, move.Y), shooter).Should().BeGreaterThanOrEqualTo(Vector2.Distance(before, shooter) - 0.01f);
                }
            }
        }

        [TestMethod]
        public void FasterFighters_AvoidMore()
        {
            var quick = DuelArenaTests.Mage();
            quick.Stats.MoveSpeed = 60;

            MissRate(DuelArenaTests.Sniper(), quick).Should().BeGreaterThan(MissRate(DuelArenaTests.Sniper(), DuelArenaTests.Mage()) + 0.1);
        }

        [TestMethod]
        public void ExposedFighters_StillAvoid_ButNeverDodge()
        {
            var exposed = DuelArenaTests.Mage();
            exposed.Keystones = new List<string> { DuelKeystone.Exposed };
            var events = Events(DuelArenaTests.Sniper(), exposed).ToList();

            events.Should().Contain(e => e.Type == DuelEventType.Miss && e.Src == 0);
            events.Should().NotContain(e => e.Type == DuelEventType.Dodge && e.Src == 0);
        }

        private static double MissRate(Dueler shooter, Dueler target)
        {
            var results = Events(shooter, target).Where(e => e.Src == 0 && IsStraight(e.Spell) && (e.Type == DuelEventType.Hit || e.Type == DuelEventType.Miss)).ToList();
            return results.Count(e => e.Type == DuelEventType.Miss) / (double)results.Count;
        }

        private static IEnumerable<DuelEvent> Outcomes(List<DuelEvent> events, int cast)
        {
            return events.Skip(cast + 1).TakeWhile(e => e.Type != DuelEventType.Turn && e.Type != DuelEventType.Cast);
        }

        private static IEnumerable<DuelEvent> Events(Dueler a, Dueler b)
        {
            return DuelArenaTests.Replays(a, b, 30).SelectMany(r => r.Events);
        }

        private static bool IsStraight(int spellId)
        {
            var spell = DuelSpellBook.Get(spellId);
            return spell != null && spell.Shape == DuelShape.Projectile && !spell.Homing;
        }
    }
}
