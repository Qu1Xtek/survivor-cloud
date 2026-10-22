using Google.Cloud.Firestore;

namespace CloudLogic.Response.DTO
{
    public class DailyRewardGrantDTO : RewardGrantDTO
    {        
        public int ConcecutiveLoginCount { get; set; }

        public Timestamp LastClaimDate { get; set; }
    }
}
