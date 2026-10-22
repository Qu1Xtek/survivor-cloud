using CloudLogic.Services.Duel.Spells;
using System.Numerics;

namespace CloudLogic.Services.Duel.Arena
{
    public static class ArenaShot
    {
        public static Vector2 Target(ArenaFighter caster, ArenaFighter enemy, DuelSpell spell)
        {
            switch (spell.Shape)
            {
                case DuelShape.Dash:
                    return ArenaDash.Landing(caster, enemy);
                case DuelShape.Zone:
                    return caster.Pos;
                default:
                    return enemy.Pos;
            }
        }

        public static float Flight(ArenaFighter caster, ArenaFighter enemy, DuelSpell spell)
        {
            switch (spell.Shape)
            {
                case DuelShape.Projectile:
                    return Vector2.Distance(caster.Pos, enemy.Pos) / spell.Speed;
                case DuelShape.TargetArea:
                    return spell.Delay;
                case DuelShape.Aura:
                case DuelShape.Zone:
                    return spell.Duration;
                default:
                    return 0;
            }
        }
    }
}
