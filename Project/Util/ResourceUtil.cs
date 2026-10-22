using CloudLogic.RepoModels.Items;
using CloudLogic.RepoModels.Player;
using CloudLogic.Repositories;
using CloudLogic.Services.Items;
using System;
using System.Threading.Tasks;
using Unity.Services.CloudCode.Core;

namespace CloudLogic.Util
{
    public class ResourceUtil
    {

        public PlayerData AddCraftingMaterials(PlayerData playerData, int amount)
        {
            playerData.CraftingMaterials += amount;

            return playerData;
        }

        public PlayerData AddGold(PlayerData playerData, long goldAmount)
        {
            playerData.Gold += goldAmount;

            return playerData;
        }

        public PlayerData AddTalentResetToken(PlayerData playerData, int amount)
        {
            playerData.TalentResetToken += amount;

            return playerData;
        }

        public PlayerData AddBlueGems(PlayerData playerData, int amount)
        {
            playerData.BlueGems += amount;

            return playerData;
        }

        public PlayerData TakeGold(PlayerData playerData, long goldAmount)
        {
            var currentGold = playerData.Gold;

            var goldDiff = currentGold - goldAmount;

            if (goldDiff < 0 || goldAmount < 0)
            {
                return playerData;
            }

            playerData.Gold = goldDiff;
            return playerData;
        }

        public long GetTotalPigGold(PlayerData playerData)
        {            
            var elapsed = DateTime.UtcNow - playerData.LastPigGoldTakenAt;

            if (elapsed.TotalMinutes < 10)
            {
                return 0;
            }

            var playerLevel = playerData.HeroLevel;
            var totalGold4Hours = (long)((playerLevel * 1.25) * 15);
            var first3HoursGold = totalGold4Hours * 0.6;
            var next1HourGold = totalGold4Hours * 0.4;

            

            if (elapsed.TotalHours >= 4)
                return totalGold4Hours;

            if (elapsed.TotalHours < 3)
            {
                var fractionOfFirstPeriod = elapsed.TotalHours / 3.0;
                return (long)(fractionOfFirstPeriod * first3HoursGold);
            }

            var fractionOfSecondPeriod = (elapsed.TotalHours - 3) / 1.0;
            return (long)(first3HoursGold + fractionOfSecondPeriod * next1HourGold);
        }

        public PlayerData TakeCraftingMatAndGold(PlayerData playerData, long goldAmount, int craftingMatAmount)
        {
            var currentGold = playerData.Gold;
            var currentCraftingMat = playerData.CraftingMaterials;

            var goldDiff = currentGold - goldAmount;
            var craftingDiff = currentCraftingMat - craftingMatAmount;

            var enoughGold = goldDiff >= 0 && goldAmount >= 0;
            var enoughCraftingMat = craftingDiff >= 0 && craftingMatAmount >= 0;

            if (!enoughGold || !enoughCraftingMat)
            {
                return null;
            }

            playerData.Gold = goldDiff;
            playerData.CraftingMaterials = craftingDiff;

            return playerData;
        }
    }
}
