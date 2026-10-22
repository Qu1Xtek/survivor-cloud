using CloudLogic.Response.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudLogic.Response
{
    public class ReforgeItemResponse : ItemResponse
    {
        public string ItemId;
        public int OldStatValue;
        public int NewStatValue;

        public Dictionary<string, int> ResourcesTaken;
    }
}
