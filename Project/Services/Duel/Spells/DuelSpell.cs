namespace CloudLogic.Services.Duel.Spells
{
    public enum DuelShape
    {
        Projectile,
        Melee,
        SelfArea,
        TargetArea,
        Aura,
        Zone,
        Dash
    }

    public enum DuelEffect
    {
        None,
        Stun,
        Chill,
        Burn,
        Poison,
        Block,
        Slow
    }

    public class DuelSpell
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsMagical { get; set; }
        public bool IsWeapon { get; set; }
        public bool IsUltimate { get; set; }
        public bool IsMobility { get; set; }
        public bool Homing { get; set; }
        public float ExecuteBonus { get; set; }
        public DuelShape Shape { get; set; }
        public float Cooldown { get; set; }
        public float Range { get; set; }
        public float Speed { get; set; }
        public float Radius { get; set; }
        public float DamageScale { get; set; }
        public int BaseDamage { get; set; }
        public int Hits { get; set; } = 1;
        public float Delay { get; set; }
        public float Duration { get; set; }
        public DuelEffect Effect { get; set; }
        public float EffectValue { get; set; }
        public float EffectDuration { get; set; }

        public bool IsClose => Shape == DuelShape.SelfArea || Shape == DuelShape.Aura || Shape == DuelShape.Zone;

        public float Reach => IsClose ? Radius : Range;
    }

    public static class DuelKeystone
    {
        public const string Exposed = "Exposed";
        public const string Unarmored = "Unarmored";
        public const string Verdict = "Verdict";
        public const string Revive = "Revive";
    }
}
