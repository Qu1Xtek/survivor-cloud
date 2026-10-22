using CloudLogic.RepoModels.Items;
using System.Collections.Generic;

namespace CloudLogic.RepoModels
{
    public class GearScoreEntry
    {
        public int Score;
        public Dictionary<ItemType, Item> Items;
        public string? Player;

        public string PlayerId;
    }
}
