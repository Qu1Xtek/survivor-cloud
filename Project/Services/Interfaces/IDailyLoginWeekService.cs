using CloudLogic.RepoModels;
using System.Threading.Tasks;

namespace CloudLogic.Services.Reward
{
    public interface IDailyLoginWeekService
    {
        Task<DailyLoginWeek> GetActiveWeek();
    }
}
