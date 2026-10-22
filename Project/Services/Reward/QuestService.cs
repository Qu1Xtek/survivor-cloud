using CloudLogic.RepoModels.PlayerRewards;
using CloudLogic.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudLogic.Services.Reward
{
    public class QuestService
    {
        private QuestRepo _questRepo;

        public QuestService(QuestRepo questRepo)
        {
            _questRepo = questRepo;
        }



        public async Task<Quest> GetQuestById(string id)
        {
            return await _questRepo.GetQuestById(id);
        }
    }
}
