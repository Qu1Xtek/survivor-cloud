using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudLogic.RepoModels.Gem
{
    [FirestoreData]
    public class GemConfigs
    {

        [FirestoreProperty]
        public List<GemConfig> GemConfig {  get; set; }

        [FirestoreProperty]
        public List<long> GoldCostForLevel { get; set; }
    }
}
