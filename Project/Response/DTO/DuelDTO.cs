using CloudLogic.RepoModels.Duel;
using System.Collections.Generic;

namespace CloudLogic.Response.DTO
{
    public class DuelNpcDTO
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Level { get; set; }
        public string ModelName { get; set; }
        public string MainWeapon { get; set; }
        public List<int> Spells { get; set; }
        public List<string> Keystones { get; set; }
        public int Health { get; set; }
        public int RewardGold { get; set; }
        public int RewardGems { get; set; }
        public bool Defeated { get; set; }
    }

    public class DuelResultDTO
    {
        public bool Success { get; set; }
        public string AdditionalInfo { get; set; }
        public bool Won { get; set; }
        public bool FirstWin { get; set; }
        public int RewardGold { get; set; }
        public int RewardGems { get; set; }
        public DuelReplay Replay { get; set; }
    }

    public class DuelLoadoutDTO
    {
        public bool Success { get; set; }
        public string MainWeapon { get; set; }
        public List<int> Spells { get; set; }
        public int MaxHp { get; set; }
        public DuelerStats Stats { get; set; }
        public DuelSpellDTO Weapon { get; set; }
        public List<DuelSpellDTO> Book { get; set; }
        public int Stance { get; set; }
    }

    public class DuelSpellDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsMagical { get; set; }
        public string Shape { get; set; }
        public string Effect { get; set; }
        public float Cooldown { get; set; }
        public int Hits { get; set; }
        public int DamageMin { get; set; }
        public int DamageMax { get; set; }
        public float Range { get; set; }
        public float Radius { get; set; }
        public float Speed { get; set; }
        public float Delay { get; set; }
    }
}
