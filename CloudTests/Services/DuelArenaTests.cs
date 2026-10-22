using CloudLogic.RepoModels.Duel;
using CloudLogic.Services.Duel.Arena;
using CloudLogic.Services.Duel.Spells;
using FluentAssertions;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.Json;

namespace CloudTests.Services
{
    [TestClass]
    public sealed class DuelArenaTests
    {
        private const int Fights = 40;
        private const int AirStep = 19;
        private const int Execution = 20;

        [TestMethod]
        public void SameSeed_GivesIdenticalReplay()
        {
            var first = ArenaSimulator.Run(Mage(), Warrior(), 42);
            var second = ArenaSimulator.Run(Mage(), Warrior(), 42);

            JsonSerializer.Serialize(first).Should().Be(JsonSerializer.Serialize(second));
        }

        [TestMethod]
        public void Fight_EndsWithDeathOrTimeout()
        {
            var replay = ArenaSimulator.Run(Mage(), Warrior(), 7);

            replay.Duration.Should().BeLessThanOrEqualTo(ArenaState.MaxTime);
            replay.Events.Should().Contain(e => e.Type == DuelEventType.Hit);
            (replay.Events.Any(e => e.Type == DuelEventType.Death) || replay.Duration >= ArenaState.MaxTime).Should().BeTrue();
        }

        [TestMethod]
        public void MatchingSpells_BeatMismatchedSpells_WithSameStats()
        {
            var fitted = Mage();
            var mismatched = Mage(new List<int> { 2, 14, 18, 15 }, "Sword");

            WinRate(fitted, mismatched).Should().BeGreaterThan(0.7);
        }

        [TestMethod]
        public void EveryLoadoutSpell_AndTheWeapon_GetCast()
        {
            var casts = Replays(Mage(), Warrior()).SelectMany(r => r.Events).Where(e => e.Type == DuelEventType.Cast && e.Src == 0).Select(e => e.Spell).ToHashSet();

            casts.Should().Contain(new[] { 5, 13, 6, 7, 10 });
        }

        [TestMethod]
        public void MirrorMatch_IsFair()
        {
            WinRate(Mage(), Mage()).Should().BeInRange(0.3, 0.7);
        }

        [TestMethod]
        public void Exposed_TakesMoreDamage_AndLosesToEqualHero()
        {
            var exposed = Mage();
            exposed.Keystones = new List<string> { DuelKeystone.Exposed };

            WinRate(Mage(), exposed).Should().BeGreaterThan(0.5);
        }

        internal static IEnumerable<DuelReplay> Replays(Dueler a, Dueler b, int fights = 20)
        {
            return Enumerable.Range(1, fights).Select(seed => ArenaSimulator.Run(a, b, seed));
        }

        internal static Vector2 PositionBefore(DuelReplay replay, int index, int side)
        {
            var pos = new Vector2(replay.Fighters[side].X, replay.Fighters[side].Y);

            foreach (var e in replay.Events.Take(index).Where(e => e.Src == side && Relocates(e)))
            {
                pos = new Vector2(e.X, e.Y);
            }

            return pos;
        }

        private static bool Relocates(DuelEvent e)
        {
            return e.Type == DuelEventType.Move || (e.Type == DuelEventType.Cast && (e.Spell == AirStep || e.Spell == Execution));
        }

        private static double WinRate(Dueler a, Dueler b)
        {
            return Replays(a, b, Fights).Count(r => r.Winner == 0) / (double)Fights;
        }

        internal static Dueler Mage(List<int> spells = null, string weapon = "Staff")
        {
            return Hero("Mage", weapon, spells ?? new List<int> { 5, 13, 6, 7 }, new DuelerStats { Health = 900, DamageMin = 80, DamageMax = 100, AddMagicDmg = 40, CritChance = 10 });
        }

        internal static Dueler Brute()
        {
            return Hero("Brute", "Bow", new List<int> { 2, 14, 18, 16 }, new DuelerStats { Health = 900, DamageMin = 80, DamageMax = 100, AddPhysDmg = 40, CritChance = 25, CritDamage = 2f });
        }

        internal static Dueler Warrior(int armor = 100, int dodge = 0)
        {
            return Hero("Warrior", "Sword", new List<int> { 3, 15, 16, 2 }, new DuelerStats { Health = 1100, DamageMin = 70, DamageMax = 90, AddPhysDmg = 20, Armor = armor, Dodge = dodge });
        }

        internal static Dueler Sniper()
        {
            return Hero("Sniper", "Bow", new List<int> { 8, 17, 6, 18 }, new DuelerStats { Health = 900, DamageMin = 80, DamageMax = 100, AddPhysDmg = 20, AddMagicDmg = 20 });
        }

        internal static Dueler Hero(string name, string weapon, List<int> spells, DuelerStats stats)
        {
            return new Dueler { Name = name, MainWeapon = weapon, Spells = spells, Stats = stats, Level = 10 };
        }
    }
}
