using CloudLogic.RepoModels.Duel;
using CloudLogic.Response.DTO;
using CloudLogic.Services.Duel.Spells;
using System.Linq;

namespace CloudLogic.Services.Duel
{
    public static class DuelMapper
    {
        public static DuelNpcDTO ToNpc(Dueler npc, bool defeated)
        {
            return new DuelNpcDTO
            {
                Id = npc.Id,
                Name = npc.Name,
                Description = npc.Description,
                Level = npc.Level,
                ModelName = npc.ModelName,
                MainWeapon = npc.MainWeapon,
                Spells = npc.Spells,
                Keystones = npc.Keystones,
                Health = npc.Stats?.Health ?? 0,
                RewardGold = npc.RewardGold,
                RewardGems = npc.RewardGems,
                Defeated = defeated
            };
        }

        public static DuelLoadoutDTO ToLoadout(Dueler me, bool success)
        {
            var castable = DuelSpellBook.All.Values.Where(s => !s.IsWeapon).OrderBy(s => s.Name).ToList();

            return new DuelLoadoutDTO
            {
                Success = success,
                MainWeapon = me.MainWeapon,
                Spells = me.Spells,
                MaxHp = me.Stats.Health,
                Stats = me.Stats,
                Weapon = ToSpell(me, DuelSpellBook.WeaponFor(me.MainWeapon)),
                Book = castable.Select(s => ToSpell(me, s)).ToList(),
                Stance = me.Stance
            };
        }

        public static DuelSpellDTO ToSpell(Dueler me, DuelSpell spell)
        {
            return new DuelSpellDTO
            {
                Id = spell.Id,
                Name = spell.Name,
                IsMagical = spell.IsMagical,
                Shape = spell.Shape.ToString(),
                Effect = spell.Effect.ToString(),
                Cooldown = DuelSpellStats.Cooldown(me, spell),
                Hits = DuelSpellStats.Hits(me, spell),
                DamageMin = DuelSpellStats.Min(me, spell),
                DamageMax = DuelSpellStats.Max(me, spell),
                Range = spell.Range,
                Radius = spell.Radius,
                Speed = spell.Speed,
                Delay = spell.Delay
            };
        }
    }
}
