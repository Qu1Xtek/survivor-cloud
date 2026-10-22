using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudLogic
{
    public class MailNotifDto
    {
        public string PlayerId;
        public string DeviceId;
        public string Title;
        public string Message;
        public string Type;
        public Dictionary<string, int> Claim;
        public string MailData;
        public long Gold;
    }
}
