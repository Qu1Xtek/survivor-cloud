using CloudLogic.Cache;
using CloudLogic.RepoModels;
using CloudLogic.RepoModels.PlayerRewards;
using CloudLogic.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudLogic.Services.Reward
{
    public class DailyLoginWeekService : IDailyLoginWeekService
    {

        private DailyLoginWeekRepo _repo;
        private QuestService _questService;
        private ICloudCache _cache;

        public DailyLoginWeekService(DailyLoginWeekRepo repo, ICloudCache cache, QuestService questService)
        {
            _repo = repo;
            _cache = cache;
            _questService = questService;
        }


        public async Task<DailyLoginWeek> GetActiveWeek()
        {

            var result = await _cache.GetDailyLoginConfig();

            if (result == null)
            {
                var activeWeek = await _repo.GetActiveWeek();
                // Fetch all quests concurrently instead of one round trip per id.
                var quests = await Task.WhenAll(activeWeek.QuestIds.Select(id => _questService.GetQuestById(id)));
                activeWeek.Quests = quests.ToList();
                result = await _cache.LoadDailyLoginConfigFromFirebase(activeWeek);
            }

            return result;
        }
    }
}
