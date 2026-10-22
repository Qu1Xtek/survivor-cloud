using CloudLogic.RepoModels.Player;
using System.Threading.Tasks;

namespace CloudLogic.Services.Player
{
    public interface IEnergyService
    {
        /// <summary>
        /// Adds energy to a player's account
        /// </summary>
        Task<long> AddEnergy(string playerId, int amount, bool canOverflow = false);

        /// <summary>
        /// Takes energy from a player's account
        /// </summary>
        Task<long> TakeEnergy(PlayerData playerData, int energyToTake);
    }
}
