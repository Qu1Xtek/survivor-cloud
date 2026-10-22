using CloudLogic.RepoModels.Items;
using CloudLogic.Util;
using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudLogic.Response.DTO
{
    public class ItemMergeDTO
    {
        public Item Item { get; set; }

        public BindableInt Gold { get; set; }
        public BindableInt CraftingMaterials { get; set; }

    }
}
