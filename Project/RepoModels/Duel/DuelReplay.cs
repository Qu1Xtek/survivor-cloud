using System.Collections.Generic;

namespace CloudLogic.RepoModels.Duel
{
    public class DuelReplay
    {
        public int Seed { get; set; }
        public float Duration { get; set; }
        public int Winner { get; set; }
        public List<DuelReplayFighter> Fighters { get; set; } = new List<DuelReplayFighter>();
        public List<DuelEvent> Events { get; set; } = new List<DuelEvent>();
    }

    public class DuelReplayFighter
    {
        public string Name { get; set; }
        public string ModelName { get; set; }
        public string MainWeapon { get; set; }
        public bool IsNpc { get; set; }
        public int Level { get; set; }
        public int MaxHp { get; set; }
        public List<int> Spells { get; set; } = new List<int>();
        public float X { get; set; }
        public float Y { get; set; }
    }

    public class DuelEvent
    {
        public float T { get; set; }
        public string Type { get; set; }
        public int Src { get; set; }
        public int Spell { get; set; }
        public int Amount { get; set; }
        public int Hp { get; set; }
        public bool Crit { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Dur { get; set; }
    }

    public static class DuelEventType
    {
        public const string Cast = "Cast";
        public const string Hit = "Hit";
        public const string Dot = "Dot";
        public const string Miss = "Miss";
        public const string Dodge = "Dodge";
        public const string Heal = "Heal";
        public const string Effect = "Effect";
        public const string Revive = "Revive";
        public const string Death = "Death";
        public const string Turn = "Turn";
        public const string Move = "Move";
    }
}
