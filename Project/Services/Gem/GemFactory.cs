using CloudLogic.RepoModels.Gem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudLogic.Services.Gem
{
    public class GemFactory
    {
        private GemUpgrader _gemUpgrader;

        public GemFactory()
        {
            _gemUpgrader = new GemUpgrader();
        }
        public PlayerGem CreateGem(GemType gemType)
        {
            //PlayerGem gem = gemType switch
            //{
            //    GemType.Main => CreateMainGem(),
            //    _ => CreateSecondaryGem(gemType),
            //};

            //gem.MilestonePowers = new Dictionary<string, int>()
            //{
            //    { "5", -1 },
            //    { "10", -1 },
            //    { "15", -1 },
            //    { "20", -1 },
            //    { "25", -1 },
            //    { "30", -1 },
            //};

            var gem = new PlayerGem()
            {
                GemType = gemType,
                Level = 1
            };

            //_gemUpgrader.UpgradeGem(gem);
            
            return gem;
        }

        private PlayerGem CreateSecondaryGem(GemType gemType)
        {
            var gem = new PlayerGem()
            {
                EquipSlot = 0,
                GemType = gemType,
            };

            return _gemUpgrader.UpgradeGem(gem);
        }

        private PlayerGem CreateMainGem()
        {
            var gem = new PlayerGem()
            {
                EquipSlot = 1,
                GemType = GemType.Main,
            };

            return _gemUpgrader.UpgradeGem(gem);
        }
    }
}
