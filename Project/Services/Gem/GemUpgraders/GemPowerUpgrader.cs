using CloudLogic.RepoModels.Gem;
using CloudLogic.Services.Gem.GemPowerUpgraders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudLogic.Services.Gem.GemUpgraders
{
    internal abstract class GemPowerUpgrader : IGemPowerUpgrader
    {
        public abstract string Power { get; }
        protected float Step;

        protected GemPowerUpgrader(float step)
        {
            Step = step;
        }

        public abstract PlayerGem UpgradePower(PlayerGem gem);
    }
}
