using CloudLogic.RepoModels;
using CloudLogic.RepoModels.Items;
using CloudLogic.RepoModels.PlayerRewards;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CloudLogic.Repositories
{
    public class DailyLoginWeekRepo : BaseFireStoreService
    {
        public DailyLoginWeekRepo(ILogger logger) : base("DailyLoginWeek", logger)
        {
        }

        public async Task<DailyLoginWeek> GetActiveWeek()
        {
            var collection = GetCollectionRef();
            Query query = collection.WhereEqualTo("Active", true);
            return (await GetRecordsByQuery<DailyLoginWeek>(query))[0];
        }

        public async Task<DailyLoginWeek> StoreQuest(DailyLoginWeek quest)
        {
            return await StoreRecord(quest);
        }


        public async Task DeleteQuest(string questId)
        {
            await DeleteRecordById(questId);
        }
    }
}
