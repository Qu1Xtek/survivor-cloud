using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudLogic.Services
{
    public class FCMService : IFCMService
    {
        private PushNotifier _notifier;
        public FCMService(PushNotifier notifier) 
        {
            _notifier = notifier;
        }


        public async Task SendNotif(string deviceId, string type, string title, string message, string jsonObj)
        {
            var data = new Dictionary<string, string>()
            {
                {"type" ,  type },
                {"title" , title},
                {"message" , message},
                {"objString", jsonObj}
            };

            await _notifier.SendNotificationAsync(deviceId, title, message, data);
        }
    }
}
