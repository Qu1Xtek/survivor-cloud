using System.Collections.Generic;

namespace CloudLogic.Response.DTO
{
    public class BatchClaimResponse
    {
        public bool Success { get; set; }

        public List<QuestClaimResult> Claimed { get; set; } = new List<QuestClaimResult>();

        public List<string> Rejected { get; set; } = new List<string>();
    }
}
