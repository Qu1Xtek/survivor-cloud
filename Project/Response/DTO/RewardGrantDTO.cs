using CloudLogic.RepoModels.PlayerRewards;
using CloudLogic.Util;

namespace CloudLogic.Response.DTO
{
    public class RewardGrantDTO
    {
        public bool Success { get; set; }

        public RewardType Type { get; set; }

        public string RewardObj { get; set; }

        public BindableLong RewardVal { get; set; }

        public string AdditionalInfo { get; set; }

        public long EnergyGranted { get; set; }

        public long GoldGranted { get; set; }

        public long GemsGranted { get; set; }

        public long CraftingMatsGranted { get; set; }
    }
}
