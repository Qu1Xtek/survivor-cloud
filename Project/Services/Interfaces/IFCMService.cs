using System.Threading.Tasks;

namespace CloudLogic.Services
{
    public interface IFCMService
    {
        Task SendNotif(string deviceId, string type, string title, string message, string jsonObj);
    }
}
