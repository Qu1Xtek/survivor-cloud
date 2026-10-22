using CloudLogic.Util;
using System;

namespace CloudLogic.Response
{
    public class PlayerDataDTO
    {
        public string Id;
        public BindableInt SelectedLevel { get; set; }

        public BindableLong Gold { get; set; }

        public BindableInt RerollRuneCount { get; set; }

        public BindableInt StashSize { get; set; }

        public bool Premium { get; set; }

        public BindableDateTime ForgeCardUntil { get; set; }

        public int Revives { get; set; }

        public BindableLong HeroLevel { get; set; }

        public BindableLong MaxLevelReached { get; set; }

        public BindableLong Energy { get; set; }

        public BindableLong Crystals { get; set; }


        public LoginRewardInfoDTO? LoginRewardInfo { get; set; }

        public BindableInt BlueGems {  get; set; }
        public BindableInt AuctionsCount {  get; set; }

        public string DeviceId { get; set; }
        public string Platform { get; set; }

        public BindableInt CraftingMaterials { get; set; }
        public BindableInt TalentResetToken { get; set; }

        public BindableDateTime NextBlueGemAdAt { get; set; }

        public BindableDateTime LastEnergyGiveStamp { get; set; }

        public BindableDateTime LastPigGoldTakenStamp { get; set; }

        public BindableDateTime NextBlueGemAdAvailableAt { get; set; }

        public bool FirstRunStarted { get; set; }
    }
}
    