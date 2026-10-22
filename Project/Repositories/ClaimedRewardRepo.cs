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
    public class ClaimedRewardRepo : BaseFireStoreService
    {
        public ClaimedRewardRepo(ILogger logger) : base("ClaimedReward", logger)
        {
        }

        public async Task<ClaimedReward> GetClaimedRewardById(string id)
        {
            return await GetRecordById<ClaimedReward>(id);
        }

        public virtual async Task<ClaimedReward> StoreClaimedReward(ClaimedReward claimedReward)
        {
            return await StoreRecord(claimedReward);
        }


        public async Task DeleteClaimedReward(string id)
        {
            await DeleteRecordById(id);
        }
    }
}
