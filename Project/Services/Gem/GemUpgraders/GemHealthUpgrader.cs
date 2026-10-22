using CloudLogic.RepoModels.Gem;
using CloudLogic.RepoModels.Items;
using CloudLogic.Services.Gem.GemUpgraders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudLogic.Services.Gem.GemPowerUpgraders
{
    internal class GemHealthUpgrader : GemPowerUpgrader
    {
        public GemHealthUpgrader(float step) : base(step)
        {
        }

        public override string Power => "Damage";

        public override PlayerGem UpgradePower(PlayerGem gem)
        { 
                

            var damageUpgrade = GetWeaponDamageForLevel(gem.Level / 5f, RarityType.Epic);

            GemPower damagePower = gem.PowerValues.FirstOrDefault(x => x.Name == Power);

            if (damagePower == null)
            {
                gem.PowerValues.Add(new GemPower()
                {
                    Name = Power,
                    Value = damageUpgrade * Step
                });
            } else
            {
                damagePower.Value = damageUpgrade * Step;
            }
            
            return gem;
        }

        public static float GetWeaponDamageForLevel(float weaponLevel, RarityType rarity)
        {
            double common;

            if (weaponLevel <= 20)
            {
                // Standard formula for levels 1-20
                common = 45.0 * Math.Pow(1.3, weaponLevel);
            }
            else
            {
                // For levels > 20: use level 20 as base, then add flat damage
                // Odd levels (21, 23, 25...): add 1000
                // Even levels (22, 24, 26...): add 2000
                double baseLevel20 = 45.0 * Math.Pow(1.3, 20);

                // Calculate additional damage: 1000 for odd levels, 2000 for even levels
                double additionalDamage = 0;
                float levelsAbove20 = weaponLevel - 20;

                for (int i = 1; i <= levelsAbove20; i++)
                {
                    int levelNumber = 20 + i;
                    if (levelNumber % 2 == 1) // Odd level number (21, 23, 25...)
                    {
                        additionalDamage += 1000;
                    }
                    else // Even level number (22, 24, 26...)
                    {
                        additionalDamage += 2000;
                    }
                }

                common = baseLevel20 + additionalDamage;
            }

            double rare = common * (Math.Pow(1.15, (int)rarity));
            return (float)rare;
        }

    }
}
