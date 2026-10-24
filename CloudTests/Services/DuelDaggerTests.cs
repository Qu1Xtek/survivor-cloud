using CloudLogic.RepoModels.Items;
using FluentAssertions;
using System.Collections.Generic;
using System.Linq;

namespace CloudTests.Services
{
    [TestClass]
    public sealed class DuelDaggerTests
    {
        [TestMethod]
        public void Tiers_GoByFive_ThenThree_ThenOne()
        {
            var tiers = new List<int>();

            for (int level = DuelDagger.NextTier(0); level <= 37; level = DuelDagger.NextTier(level))
            {
                tiers.Add(level);
            }

            tiers.Should().Equal(5, 10, 15, 20, 25, 28, 31, 34, 35, 36, 37);
        }

        [TestMethod]
        public void Droppable_NeverSkipsATier()
        {
            DuelDagger.Droppable(0, 30).Should().Equal(5);
            DuelDagger.Droppable(5, 30).Should().Equal(5, 10);
            DuelDagger.Droppable(25, 40).Should().Equal(5, 10, 15, 20, 25, 28);
        }

        [TestMethod]
        public void Droppable_NeverExceedsTheMap()
        {
            DuelDagger.Droppable(10, 12).Should().Equal(5, 10);
            DuelDagger.Droppable(20, 9).Should().Equal(5);
            DuelDagger.Droppable(0, 4).Should().BeEmpty();
        }

        [TestMethod]
        public void Dagger_RangesComeFromDropLevel_NotTheTemplate()
        {
            DuelDagger.At(Template(), 5, RarityType.Common).HealthRange.Should().Equal(1, 25);
            DuelDagger.At(Template(), 5, RarityType.Epic).CriticalChanceRange.Should().Equal(1, 7);
            DuelDagger.At(Template(), 10, RarityType.Common).HealthRange.Should().Equal(1, 50);
            DuelDagger.At(Template(), 15, RarityType.Rare).CriticalChanceRange.Should().Equal(1, 20);
            DuelDagger.At(Template(), 34, RarityType.Epic).HealthRange.Should().Equal(1, 170);
            DuelDagger.At(Template(), 34, RarityType.Epic).CriticalChanceRange.Should().Equal(1, 30);
        }

        [TestMethod]
        public void Dagger_DamageGrowsWithLevelAndRarity_AndHasNoArmor()
        {
            var low = ItemStatGenerator.GenerateStats(DuelDagger.At(Template(), 5, RarityType.Common), RarityType.Common);
            var high = ItemStatGenerator.GenerateStats(DuelDagger.At(Template(), 25, RarityType.Epic), RarityType.Epic);

            high.Type.Should().Be(ItemType.DuelWeapon);
            high.Level.Should().Be(25);
            high.Armor.Should().Be(0);
            high.DamageMax.Should().BeGreaterThan(low.DamageMax);
        }

        [TestMethod]
        public void Dagger_IsNamedByRarity()
        {
            var template = Template();
            template.RarityOptions = new Dictionary<int, string> { { (int)RarityType.Rare, "Duelist's Fang" } };

            ItemStatGenerator.GenerateStats(DuelDagger.At(template, 10, RarityType.Rare), RarityType.Rare).Name.Should().Be("Duelist's Fang");
            ItemStatGenerator.GenerateStats(DuelDagger.At(template, 10, RarityType.Common), RarityType.Common).Name.Should().Be("Dagger");
        }

        [TestMethod]
        public void Dagger_OnlyRollsHealthAndCrit()
        {
            var common = ItemStatGenerator.GenerateStats(DuelDagger.At(Template(), 10, RarityType.Common), RarityType.Common);
            var epic = ItemStatGenerator.GenerateStats(DuelDagger.At(Template(), 10, RarityType.Epic), RarityType.Epic);

            common.Stats.Should().HaveCount(1);
            epic.Stats.Select(s => s.Name).Should().BeEquivalentTo("Health", "CriticalChance");
        }

        private static ItemTemplate Template()
        {
            return new ItemTemplate
            {
                Id = "dagger",
                Name = "Dagger",
                Type = ItemType.DuelWeapon,
                Level = 5,
                WeaponName = "Dagger",
                Icon = "Dagger",
                HealthRange = new List<int> { 20, 30 },
                CriticalChanceRange = new List<int> { 2, 4 },
                ArmorRange = new List<int> { 50, 60 },
                DodgeRange = new List<int> { 5, 8 }
            };
        }
    }
}
