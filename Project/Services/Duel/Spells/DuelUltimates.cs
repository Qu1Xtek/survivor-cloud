using System.Collections.Generic;

namespace CloudLogic.Services.Duel.Spells
{
    public static class DuelUltimates
    {
        public const float FullCharge = 100f;

        private static readonly Dictionary<int, DuelSpell> ByWeapon = new Dictionary<int, DuelSpell>
        {
            { 12, new DuelSpell { Id = 20, Name = "Execution", IsUltimate = true, Shape = DuelShape.Dash, Range = 12, DamageScale = 1.3f, BaseDamage = 10, ExecuteBonus = 0.6f } },
            { 10, new DuelSpell { Id = 21, Name = "Arcane Storm", IsUltimate = true, IsMagical = true, Shape = DuelShape.TargetArea, Range = 7, Radius = 1.6f, Delay = 0.3f, DamageScale = 0.45f, BaseDamage = 3, Hits = 5, Effect = DuelEffect.Chill, EffectValue = 0.3f, EffectDuration = 2 } },
            { 11, new DuelSpell { Id = 22, Name = "Arrow Rain", IsUltimate = true, Shape = DuelShape.Projectile, Range = 11, Speed = 6f, DamageScale = 0.28f, BaseDamage = 2, Hits = 6 } }
        };

        public static DuelSpell For(DuelSpell weapon)
        {
            return ByWeapon.TryGetValue(weapon.Id, out var ultimate) ? ultimate : null;
        }
    }
}
