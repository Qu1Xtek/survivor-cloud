using CloudLogic.RepoModels.Gem;
using CloudLogic.Services.Gem.GemPowerUpgraders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudLogic.Services.Gem
{
    public class GemUpgrader
    {
        private Dictionary<GemType, List<IGemPowerUpgrader>> GemUpgraders = new Dictionary<GemType, List<IGemPowerUpgrader>>()
        {
            [GemType.Main] = new List<IGemPowerUpgrader>() { new GemDamageUpgrader(0.3f) },
            [GemType.Health] = new List<IGemPowerUpgrader>() { new GemDamageUpgrader(0.06f) },
        };

        public PlayerGem UpgradeGem(PlayerGem gem)
        {
            gem.Level++;

            foreach (var upgrader in GemUpgraders[gem.GemType]) 
            {
                gem = upgrader.UpgradePower(gem);
            }

            return gem;
        }
    }
}
