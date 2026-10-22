using CloudLogic.RepoModels.Items;
using CloudLogic.RepoModels.Player;
using CloudLogic.Services.Duel;
using FluentAssertions;
using System.Collections.Generic;

namespace CloudTests.Services
{
    [TestClass]
    public sealed class PlayerDuelerBuilderTests
    {
        [TestMethod]
        public void Build_SumsGearStats_AndKeepsWeapon()
        {
            var items = new List<Item>
            {
                new Item { Type = ItemType.Weapon, WeaponName = "Bow", DamageMin = 40, DamageMax = 60, AttackSpeed = 1.2, Stats = new List<ItemStat> { new ItemStat { Name = "CriticalChance", Value = 10 } } },
                new Item { Type = ItemType.Chest, Armor = 50, Stats = new List<ItemStat> { new ItemStat { Name = "Health", Value = 200 }, new ItemStat { Name = "Dodge", Value = 5 } } },
                new Item { Type = ItemType.Ring, LegendaryStat = new Dictionary<string, int> { { "Exposed", 1 } }, Stats = new List<ItemStat> { new ItemStat { Name = "PhysicalDamage", Value = 15 } } }
            };

            var dueler = PlayerDuelerBuilder.Build("Me", new PlayerData { Id = "p1", HeroLevel = 5, DuelSpells = new List<int> { 2, 14, 9, 99 } }, items);

            dueler.MainWeapon.Should().Be("Bow");
            dueler.IsNpc.Should().BeFalse();
            dueler.Stats.Health.Should().Be(300);
            dueler.Stats.Armor.Should().Be(50);
            dueler.Stats.Dodge.Should().Be(5);
            dueler.Stats.CritChance.Should().Be(11);
            dueler.Stats.AddPhysDmg.Should().Be(15);
            dueler.Stats.DamageMin.Should().Be(40);
            dueler.Keystones.Should().Contain("Exposed");
            dueler.Spells.Should().Equal(2, 14);
        }

        [TestMethod]
        public void Build_WithNoGear_UsesBaseHero()
        {
            var dueler = PlayerDuelerBuilder.Build("Me", new PlayerData { Id = "p1", HeroLevel = 1 }, new List<Item>());

            dueler.MainWeapon.Should().Be("Sword");
            dueler.Stats.Health.Should().Be(100);
            dueler.Stats.DamageMin.Should().Be(15);
        }
    }
}
