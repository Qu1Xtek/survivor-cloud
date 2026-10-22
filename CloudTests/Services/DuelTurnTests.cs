using CloudLogic.RepoModels.Duel;
using FluentAssertions;
using System.Collections.Generic;
using System.Linq;

namespace CloudTests.Services
{
    [TestClass]
    public sealed class DuelTurnTests
    {
        private const int AirStep = 19;

        [TestMethod]
        public void BothFighters_OpenTheDuel()
        {
            foreach (var replay in DuelArenaTests.Replays(DuelArenaTests.Warrior(), DuelArenaTests.Mage()))
            {
                var opening = replay.Events.Where(e => e.Type == DuelEventType.Turn).Take(2).Select(e => e.Src);

                opening.Should().BeEquivalentTo(new[] { 0, 1 });
            }
        }

        [TestMethod]
        public void AfterCasting_TheOtherFighterActsNext_AtEqualSpeed()
        {
            foreach (var replay in DuelArenaTests.Replays(DuelArenaTests.Sniper(), DuelArenaTests.Brute()))
            {
                var turns = Turns(replay.Events);

                for (int i = 0; i < turns.Count - 1; i++)
                {
                    if (turns[i].Any(e => e.Type == DuelEventType.Cast))
                    {
                        turns[i + 1][0].Src.Should().NotBe(turns[i][0].Src);
                    }
                }
            }
        }

        [TestMethod]
        public void FasterAttackSpeed_EarnsMoreTurns()
        {
            var fast = DuelArenaTests.Mage();
            fast.Stats.AttackSpeed = 1.4f;
            var replays = DuelArenaTests.Replays(fast, DuelArenaTests.Mage()).ToList();

            double fastTurns = replays.Average(r => r.Events.Count(e => e.Type == DuelEventType.Turn && e.Src == 0));
            double evenTurns = replays.Average(r => r.Events.Count(e => e.Type == DuelEventType.Turn && e.Src == 1));

            fastTurns.Should().BeGreaterThan(evenTurns * 1.08);
        }

        [TestMethod]
        public void EachTurn_CastsAtMostOneSpell()
        {
            foreach (var replay in DuelArenaTests.Replays(DuelArenaTests.Warrior(), DuelArenaTests.Mage()))
            {
                foreach (var turn in Turns(replay.Events))
                {
                    turn.Count(e => e.Type == DuelEventType.Cast && e.Spell != AirStep).Should().BeLessThanOrEqualTo(1);
                }
            }
        }

        [TestMethod]
        public void OnlyTheActorWalks_TheOtherOnlySidestepsShots()
        {
            foreach (var replay in DuelArenaTests.Replays(DuelArenaTests.Warrior(), DuelArenaTests.Brute()))
            {
                foreach (var turn in Turns(replay.Events))
                {
                    int actor = turn[0].Src;

                    for (int i = 1; i < turn.Count; i++)
                    {
                        if (turn[i].Type == DuelEventType.Move && turn[i].Src != actor)
                        {
                            turn[i - 1].Type.Should().Be(DuelEventType.Cast);
                            turn[i - 1].Src.Should().Be(actor);
                        }
                    }
                }
            }
        }

        private static List<List<DuelEvent>> Turns(List<DuelEvent> events)
        {
            var turns = new List<List<DuelEvent>>();

            foreach (var e in events)
            {
                if (e.Type == DuelEventType.Turn)
                {
                    turns.Add(new List<DuelEvent>());
                }

                if (turns.Count > 0)
                {
                    turns[turns.Count - 1].Add(e);
                }
            }

            return turns;
        }
    }
}
