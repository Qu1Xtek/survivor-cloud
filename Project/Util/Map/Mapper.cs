using CloudLogic.RepoModels.Items;
using CloudLogic.RepoModels.Player;
using CloudLogic.Response;
using CloudLogic.Response.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudLogic.Util.Map
{
    public class Mapper
    {

        public static PlayerDataDTO MapPlayerDataToDTO(PlayerData data)
        {
            return new PlayerDataDTO()
            {
                BlueGems = BindableInt.Of(data.BlueGems),
                CraftingMaterials = BindableInt.Of(data.CraftingMaterials),
                Energy = BindableLong.Of(data.Energy),
                LastEnergyGiveStamp = BindableDateTime.Of(data.LastEnergyGiveStamp.ToDateTime()),
                Gold = BindableLong.Of(data.Gold),
                DeviceId = data.DeviceId,
                Platform = data.Platform,
                Crystals = BindableLong.Of(data.Crystals),
                HeroLevel = BindableLong.Of(data.HeroLevel),
                SelectedLevel = BindableInt.Of(data.SelectedLevel),
                MaxLevelReached = BindableLong.Of(data.MaxLevelReached),
                RerollRuneCount = BindableInt.Of(data.RerollRuneCount),
                StashSize = BindableInt.Of(data.StashLimit),
                Premium = data.Premium,
                ForgeCardUntil = BindableDateTime.Of(data.ForgeCardUntil.ToDateTime()),
                Revives = data.Revives,
                TalentResetToken = BindableInt.Of(data.TalentResetToken),
                LastPigGoldTakenStamp = BindableDateTime.Of(data.LastPigGoldTakenStamp.ToDateTime()),
                NextBlueGemAdAt = BindableDateTime.Of(data.NextBlueGemAdAt.ToDateTime()),
                AuctionsCount = BindableInt.Of(data.AuctionsCount),
                LoginRewardInfo = new LoginRewardInfoDTO()
                {
                    ConcecutiveLoginCount = data.LoginRewardInfo.ConcecutiveLoginCount,
                    LastClaimDate = data.LoginRewardInfo.LastClaimDate,
                },
                FirstRunStarted = data.FirstRunStarted
            };
        }
    }
}
