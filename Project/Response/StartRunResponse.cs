using CloudLogic.RepoModels.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudLogic.Response
{
    public class StartRunResponse : BaseResponse
    {
        public int MaxGoldDrops { get; set; }
        public int MaxItemDrops { get; set; }
    }
}
