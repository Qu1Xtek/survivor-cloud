using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudLogic.Response
{
    public class AlterItemResponse : BaseResponse
    {
        public string ItemId;

        public string OldStat;
        public string NewStat;
        public int NewStatValue;

        public Dictionary<string, int> ResourcesTaken;
    }
}
