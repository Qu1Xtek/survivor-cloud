using CloudLogic.RepoModels.Achievements;
using System.Collections.Generic;

namespace CloudLogic.Services.Models
{
    public class ProgressDict
    {
        public Dictionary<string, long> AchievementProgressD { get; set; }

        public Dictionary<string, long> QuestsProgressD { get; set; }
    }
}
