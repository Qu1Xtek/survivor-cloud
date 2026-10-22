using CloudLogic.Response.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudLogic.Response.IAP
{
        public class PurchaseGrantResult
        {
            public bool Ok { get; set; }
            public string CorrelationId { get; set; }
            public int GemsGranted { get; set; }
            public List<RewardGrantDTO> Rewards { get; set; } = new();

            public static PurchaseGrantResult Success(string id, int amount)
                => new() { Ok = true, CorrelationId = id, GemsGranted = amount };
            public static PurchaseGrantResult Failure(string id)
                => new() { Ok = false, CorrelationId = id };
        }
}
