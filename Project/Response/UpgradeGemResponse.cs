using CloudLogic.RepoModels.Gem;
using CloudLogic.RepoModels.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudLogic.Response
{
    public class UpgradeGemResponse
    {

        public bool Success;

        public long Gold;
        public PlayerGem PlayerGem;
        public PlayerGem MainGem;
    }
}
