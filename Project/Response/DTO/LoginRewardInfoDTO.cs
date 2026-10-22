
using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudLogic.Response
{
    public class LoginRewardInfoDTO
    {
        public int ConcecutiveLoginCount { get; set; }
        
        public int Week {  get; set; }
        public int Year { get; set; }

        public Timestamp LastClaimDate {  get; set; }

        public DateTime LastClaimTime => LastClaimDate.ToDateTime();
    }
}
