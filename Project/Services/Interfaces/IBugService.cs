using System.Threading.Tasks;

namespace CloudLogic.Services.Interfaces
{
    public interface IBugService
    {
        Task ReportBug(string playerId, string bugInfo, string bugDescription);
    }
}
