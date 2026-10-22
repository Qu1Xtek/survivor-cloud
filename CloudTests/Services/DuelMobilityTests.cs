using CloudLogic.RepoModels.Duel;
using CloudLogic.Services.Duel.Arena;
using FluentAssertions;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace CloudTests.Services
{
    [TestClass]
    public sealed class DuelMobilityTests
    {
        private const int AirStep = 19;
        private const int Fireball = 5;
        private const int Sword = 12;

        [TestMethod]
        public void Armor_ReducesMagicDamage()
        {
            AverageHit(DuelArenaTests.Warrior(armor: 400)).Should().BeLessThan(AverageHit(DuelArenaTests.Warrior(armor: 0)) * 0.9);
        }

        [TestMethod]
        public void SwordHits_SlowTheTarget()
        {
            var events = DuelArenaTests.Replays(DuelArenaTests.Warrior(), DuelArenaTests.Mage()).SelectMany(r => r.Events);

            events.Should().Contain(e => e.Type == DuelEventType.Effect && e.Src == 0 && e.Spell == Sword);
        }

        [TestMethod]
        public void AirStep_ChasesToTheTarget()
        {
            var melee = DuelArenaTests.Hero("Chaser", "Sword", new List<int> { 3, 15, AirStep, 2 }, new DuelerStats { Health = 1100, DamageMin = 70, DamageMax = 90 });

            Landings(melee, DuelArenaTests.Mage()).Should().NotBeEmpty().And.OnlyContain(gap => gap.After <= ArenaDash.LandGap + 0.01f);
        }

        [TestMethod]
        public void AirStep_EscapesToSafeRange()
        {
            var mage = DuelArenaTests.Mage(new List<int> { 5, 13, AirStep, 7 });
            var landings = Landings(mage, DuelArenaTests.Warrior());

            landings.Should().NotBeEmpty().And.OnlyContain(gap => gap.After >= gap.Before + ArenaDash.MinEscapeGain);
        }

        [TestMethod]
        public void AirStep_RespectsCooldown()
        {
            var mage = DuelArenaTests.Mage(new List<int> { 5, 13, AirStep, 7 });

            foreach (var replay in DuelArenaTests.Replays(mage, DuelArenaTests.Warrior()))
            {
                var casts = replay.Events.Where(e => e.Type == DuelEventType.Cast && e.Src == 0 && e.Spell == AirStep).Select(e => e.T).ToList();

                casts.Zip(casts.Skip(1), (prev, next) => next - prev).Should().OnlyContain(gap => gap >= 7.9f);
            }
        }

        private static double AverageHit(Dueler target)
        {
            return DuelArenaTests.Replays(DuelArenaTests.Mage(), target).SelectMany(r => r.Events)
                .Where(e => e.Type == DuelEventType.Hit && e.Src == 0 && e.Spell == Fireball && !e.Crit)
                .Average(e => e.Amount);
        }

        private static List<(float Before, float After)> Landings(Dueler stepper, Dueler other)
        {
            var landings = new List<(float Before, float After)>();

            foreach (var replay in DuelArenaTests.Replays(stepper, other))
            {
                for (int i = 0; i < replay.Events.Count; i++)
                {
                    var cast = replay.Events[i];

                    if (cast.Type == DuelEventType.Cast && cast.Src == 0 && cast.Spell == AirStep)
                    {
                        var enemy = DuelArenaTests.PositionBefore(replay, i, 1);
                        float before = Vector2.Distance(DuelArenaTests.PositionBefore(replay, i, 0), enemy);
                        landings.Add((before, Vector2.Distance(new Vector2(cast.X, cast.Y), enemy)));
                    }
                }
            }

            return landings;
        }
    }
}
