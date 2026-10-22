using CloudLogic.RepoModels;
using CloudLogic.Response;
using CloudLogic.Services.Models;

namespace CloudLogic.Services.Player.RunManagement
{
    /// <summary>
    /// Copies achievement/quest progress for a key into a run response.
    /// </summary>
    public static class RunProgressExtensions
    {
        public static void AddProgress(this BaseResponse result, string key, AchiAndQuest src)
        {
            result.AchievementProgressInfo[key] = src.AchievementProgress.Stats[key];
            result.QuestsProgressInfo[key] = src.QuestsProgress.Stats[key];
        }

        public static void AddProgress(this BaseResponse result, string key, ProgressDict src)
        {
            result.AchievementProgressInfo[key] = src.AchievementProgressD[key];
            result.QuestsProgressInfo[key] = src.QuestsProgressD[key];
        }
    }
}
