using CloudLogic.RepoModels.PlayerRewards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudLogic.Response
{
    public class RewardResult
    {
        public string RewardObj { get; set; }

        public long RewardVal { get; set; }

        public RewardType Type { get; set; }
    }
}
