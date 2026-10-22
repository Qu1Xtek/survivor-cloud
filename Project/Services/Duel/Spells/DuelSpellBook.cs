using System.Collections.Generic;
using System.Linq;

namespace CloudLogic.Services.Duel.Spells
{
    public static class DuelSpellBook
    {
        public const int MaxSpells = 4;

        public static readonly Dictionary<string, int> WeaponIds = new Dictionary<string, int>
        {
            { "Rock", 9 },
            { "Staff", 10 },
            { "Bow", 11 },
            { "Sword", 12 }
        };

        public static readonly Dictionary<int, DuelSpell> All = new List<DuelSpell>
        {
            Weapon(9, "Rock", false, DuelShape.Projectile, 5, 0.3f, 0, 6),
            Weapon(10, "Staff", true, DuelShape.Projectile, 5, 0.28f, 5, 6),
            Weapon(11, "Bow", false, DuelShape.Projectile, 10, 0.25f, 6, 8),
            new DuelSpell { Id = 12, Name = "Sword", IsWeapon = true, Shape = DuelShape.Melee, Cooldown = 1.5f, Range = 1.8f, DamageScale = 0.45f, Effect = DuelEffect.Slow, EffectValue = 0.4f, EffectDuration = 2 },
            new DuelSpell { Id = 1, Name = "Meteor Shards", IsMagical = true, Homing = true, Shape = DuelShape.Projectile, Cooldown = 3, Range = 6, Speed = 6f, DamageScale = 0.35f, Hits = 2 },
            new DuelSpell { Id = 2, Name = "Bomb", Shape = DuelShape.TargetArea, Cooldown = 4, Range = 5, Radius = 2.2f, Delay = 1.2f, DamageScale = 1f },
            new DuelSpell { Id = 3, Name = "Shockwave", Shape = DuelShape.SelfArea, Cooldown = 4, Radius = 2.2f, DamageScale = 0.4f, Effect = DuelEffect.Stun, EffectDuration = 0.6f },
            new DuelSpell { Id = 4, Name = "Fire Storm", IsMagical = true, Shape = DuelShape.Aura, Cooldown = 6, Radius = 2, Duration = 1.2f, DamageScale = 0.32f, BaseDamage = 4 },
            new DuelSpell { Id = 5, Name = "Fireball", IsMagical = true, Shape = DuelShape.Projectile, Cooldown = 7, Range = 6, Speed = 4.5f, DamageScale = 1.1f, Effect = DuelEffect.Burn, EffectValue = 0.1f, EffectDuration = 4 },
            new DuelSpell { Id = 6, Name = "Chain Lightning", IsMagical = true, Shape = DuelShape.Projectile, Cooldown = 9, Range = 10, Speed = 9f, DamageScale = 0.33f, Hits = 3 },
            new DuelSpell { Id = 7, Name = "Ice Shards", IsMagical = true, Shape = DuelShape.Projectile, Cooldown = 2.5f, Range = 6, Speed = 7f, DamageScale = 0.2f, Effect = DuelEffect.Chill, EffectValue = 0.3f, EffectDuration = 1 },
            new DuelSpell { Id = 8, Name = "Tornado", IsMagical = true, Shape = DuelShape.Projectile, Cooldown = 2.4f, Range = 10, Speed = 3.5f, DamageScale = 0.25f, Hits = 2 },
            new DuelSpell { Id = 13, Name = "Poison Gas", IsMagical = true, Shape = DuelShape.Aura, Cooldown = 5, Radius = 1.8f, Duration = 1.2f, DamageScale = 0.08f, BaseDamage = 2, Effect = DuelEffect.Poison, EffectDuration = 6 },
            new DuelSpell { Id = 14, Name = "Flamespike", Homing = true, Shape = DuelShape.Projectile, Cooldown = 3, Range = 5, Speed = 6f, DamageScale = 0.35f, Hits = 2, Effect = DuelEffect.Burn, EffectValue = 0.08f, EffectDuration = 3 },
            new DuelSpell { Id = 15, Name = "Hammer Throw", Shape = DuelShape.Zone, Cooldown = 5, Radius = 2.5f, Duration = 1.2f, DamageScale = 0.4f },
            new DuelSpell { Id = 16, Name = "Avenger's Shield", Shape = DuelShape.Projectile, Cooldown = 7, Range = 5, Speed = 5.5f, DamageScale = 0.3f, Effect = DuelEffect.Block, EffectValue = 2 },
            new DuelSpell { Id = 17, Name = "Plasma Snake", IsMagical = true, Shape = DuelShape.Projectile, Cooldown = 5, Range = 11, Speed = 4f, DamageScale = 0.15f, Hits = 4 },
            new DuelSpell { Id = 18, Name = "Rock Shards", Shape = DuelShape.Projectile, Cooldown = 3.5f, Range = 6, Speed = 6f, DamageScale = 0.3f, Hits = 2 },
            new DuelSpell { Id = 19, Name = "Air Step", IsMagical = true, IsMobility = true, Shape = DuelShape.Dash, Cooldown = 8, Range = 8, DamageScale = 0.5f }
        }.ToDictionary(s => s.Id);

        public static DuelSpell Get(int id)
        {
            return All.TryGetValue(id, out var spell) ? spell : null;
        }

        public static DuelSpell WeaponFor(string weaponName)
        {
            return Get(weaponName != null && WeaponIds.TryGetValue(weaponName, out var id) ? id : 12);
        }

        public static bool IsCastable(int id)
        {
            return All.TryGetValue(id, out var spell) && !spell.IsWeapon;
        }

        private static DuelSpell Weapon(int id, string name, bool magical, DuelShape shape, float range, float scale, int baseDamage, float speed)
        {
            return new DuelSpell { Id = id, Name = name, IsMagical = magical, IsWeapon = true, Shape = shape, Cooldown = 1.5f, Range = range, Speed = speed, DamageScale = scale, BaseDamage = baseDamage };
        }
    }
}
