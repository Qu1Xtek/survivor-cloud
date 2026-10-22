using CloudLogic.RepoModels.Items;
using CloudLogic.Response.DTO;
using CloudLogic.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudLogic.Response.Base
{
    public class ItemResponse : BaseResponse
    {
        public Item Item;

        public BindableInt GoldCost;
        public BindableInt CraftingCost;
    }
}
