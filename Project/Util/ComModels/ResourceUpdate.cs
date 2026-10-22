using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudLogic.Util.ComModels
{
    public class ResourceUpdate
    {
        public bool Success { get; set; }
        public long Gold {  get; set; }
        public int CraftingMat {  get; set; }

        public int RerollRune { get; set; }
    }
}
