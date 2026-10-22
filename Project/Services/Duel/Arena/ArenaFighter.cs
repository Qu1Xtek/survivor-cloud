using CloudLogic.RepoModels.Duel;
using CloudLogic.Services.Duel.Spells;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace CloudLogic.Services.Duel.Arena
{
    public class ArenaFighter
    {
        public const float RangeComfort = 0.8f;
        public const float MeleeRange = 1.5f;

        public int Index { get; }
        public Dueler Dueler { get; }
        public DuelerStats Stats => Dueler.Stats;
        public List<DuelSpell> Loadout { get; }
        public DuelSpell Ultimate { get; }
        public float PreferredRange { get; }
        public float MaxHp { get; }

        public Vector2 Pos { get; set; }
        public float Hp { get; set; }
        public float NextTurnAt { get; set; }
        public float UltCharge { get; set; }
        public int BlockCharges { get; set; }
        public bool ReviveUsed { get; set; }
        public float StunnedUntil { get; set; }
        public float ChilledUntil { get; set; }
        public float ChillValue { get; set; }
        public float SlowedUntil { get; set; }
        public float SlowValue { get; set; }
        public float BurnUntil { get; set; }
        public float BurnPerTick { get; set; }
        public DuelSpell BurnSpell { get; set; }
        public float PoisonUntil { get; set; }
        public int PoisonStacks { get; set; }
        public float PoisonPerStack { get; set; }
        public DuelSpell PoisonSpell { get; set; }

        private readonly Dictionary<int, float> _readyAt = new Dictionary<int, float>();

        public ArenaFighter(int index, Dueler dueler, Vector2 pos)
        {
            Index = index;
            Dueler = dueler;
            Pos = pos;
            MaxHp = System.Math.Max(1, dueler.Stats.Health);
            Hp = MaxHp;
            Loadout = BuildLoadout(dueler);
            Ultimate = DuelUltimates.For(Loadout[0]);
            PreferredRange = DuelStance.Distance(dueler.Stance) ?? (PrefersClose(Loadout) ? 1.4f : RangedReach(Loadout) * RangeComfort);
        }

        public bool Alive => Hp > 0;

        public float HpPercent => Hp / MaxHp;

        public bool Melee => PreferredRange <= MeleeRange;

        public bool UltimateReady => Ultimate != null && UltCharge >= DuelUltimates.FullCharge;

        public bool Stunned(float t)
        {
            return StunnedUntil > t;
        }

        public bool Chilled(float t)
        {
            return ChilledUntil > t;
        }

        public bool Slowed(float t)
        {
            return SlowedUntil > t;
        }

        public float ReadyTime(DuelSpell spell)
        {
            return _readyAt.TryGetValue(spell.Id, out var at) ? at : 0;
        }

        public bool Ready(DuelSpell spell, float t)
        {
            return ReadyTime(spell) <= t;
        }

        public void Use(DuelSpell spell, float t)
        {
            float cooldown = DuelSpellStats.Cooldown(Dueler, spell);
            _readyAt[spell.Id] = t + (Chilled(t) ? cooldown * (1 + ChillValue) : cooldown);
        }

        private static float RangedReach(List<DuelSpell> loadout)
        {
            return loadout.Where(s => s.Shape == DuelShape.Projectile || s.Shape == DuelShape.TargetArea).Select(s => s.Range).DefaultIfEmpty(loadout[0].Range).Average();
        }

        private static bool PrefersClose(List<DuelSpell> loadout)
        {
            return loadout[0].Shape == DuelShape.Melee || loadout.Count(s => s.IsClose) >= 2;
        }

        private static List<DuelSpell> BuildLoadout(Dueler dueler)
        {
            var spells = (dueler.Spells ?? new List<int>())
                .Where(DuelSpellBook.IsCastable)
                .Distinct()
                .Take(DuelSpellBook.MaxSpells)
                .Select(DuelSpellBook.Get);

            return new[] { DuelSpellBook.WeaponFor(dueler.MainWeapon) }.Concat(spells).ToList();
        }
    }
}
