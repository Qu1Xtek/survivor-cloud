using CloudLogic.RepoModels.Duel;
using CloudLogic.Services.Duel.Arena;
using FluentAssertions;
using System.Collections.Generic;
using System.Linq;

namespace CloudTests.Services
{
    [TestClass]
    public sealed class DuelUltimateTests
    {
        private static readonly HashSet<int> UltimateIds = new HashSet<int> { 20, 21, 22 };
        private const int Fights = 20;

        [TestMethod]
        public void Ultimates_FireOnceOrTwice_AroundTwentySeconds()
        {
            var replays = Enumerable.Range(1, Fights).Select(seed => ArenaSimulator.Run(DuelArenaTests.Mage(), DuelArenaTests.Warrior(), seed)).ToList();
            var casts = replays.Select(r => r.Events.Where(IsUltimateCast).ToList()).ToList();

            casts.Average(c => c.Count).Should().BeInRange(1, 3.5);
            casts.Where(c => c.Count > 0).Average(c => c.First().T).Should().BeInRange(12, 32);
        }

        [TestMethod]
        public void Rock_HasNoUltimate()
        {
            var rock = DuelArenaTests.Mage(weapon: "Rock");

            foreach (var seed in Enumerable.Range(1, Fights))
            {
                ArenaSimulator.Run(rock, DuelArenaTests.Warrior(), seed).Events.Should().NotContain(e => e.Src == 0 && IsUltimateCast(e));
            }
        }

        [TestMethod]
        public void Ultimate_IsNeverDodgedOrMissed()
        {
            var events = Enumerable.Range(1, Fights).SelectMany(seed => ArenaSimulator.Run(DuelArenaTests.Brute(), DuelArenaTests.Warrior(dodge: 60), seed).Events).ToList();

            events.Should().Contain(e => e.Type == DuelEventType.Hit && e.Spell == 22);
            events.Should().NotContain(e => (e.Type == DuelEventType.Dodge || e.Type == DuelEventType.Miss) && e.Spell == 22);
        }

        private static bool IsUltimateCast(DuelEvent e)
        {
            return e.Type == DuelEventType.Cast && UltimateIds.Contains(e.Spell);
        }
    }
}
