using CloudLogic.Response;
using System.Threading.Tasks;

namespace CloudLogic.Services.Player
{
    public interface IRunService
    {
        /// <summary>
        /// Starts a new run for a player
        /// </summary>
        Task<StartRunResponse> StartRun(string playerId, int selectedLevel);
    }
}
