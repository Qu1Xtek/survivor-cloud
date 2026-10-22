using CloudLogic.RepoModels.Enemy;
using CloudLogic.RepoModels.Player;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudLogic.Repositories
{
    public class WaveDetailsRepo : BaseFireStoreService
    {
        public WaveDetailsRepo(ILogger logger) : base("WaveDetails", logger)
        {
        }

        public async Task<string> GetWaveDetails()
        {
            var result = await GetRecords<WaveDetails>();



            return JsonConvert.SerializeObject(result.OrderBy(x => x.LevelIndex).ToList());
        }

        public async Task<WaveDetails> CreateWaveDetails(WaveDetails waveDetails)
        {
           
            return await CreateRecord(waveDetails);
        }


    }
}
