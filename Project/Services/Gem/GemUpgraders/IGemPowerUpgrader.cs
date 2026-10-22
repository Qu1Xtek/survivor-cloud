using CloudLogic.RepoModels.Gem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudLogic.Services.Gem.GemPowerUpgraders
{
    internal interface IGemPowerUpgrader
    {
        string Power { get; }
        public PlayerGem UpgradePower(PlayerGem gem);
    }
}
