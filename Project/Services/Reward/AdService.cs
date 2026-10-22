using CloudLogic.RepoModels.Player;
using CloudLogic.Repositories;
using CloudLogic.Response;
using CloudLogic.Services.Items;
using CloudLogic.Util;
using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudLogic.Services.Reward
{
    public class AdService
    {
        public const string FreeGiftId = "free_gems";
        private const int DefaultGems = 10;

        private readonly IPlayerManagerService _playerManagerService;
        private readonly ShopRepo _shopRepo;
        private readonly ResourceUtil _resourceUtil;

        public AdService(IPlayerManagerService playerManagerService, ShopRepo shopRepo)
        {
            _playerManagerService = playerManagerService;
            _shopRepo = shopRepo;
            _resourceUtil = new ResourceUtil();
        }

        public async Task<BaseResponse> AddBlueGemsFromAd(string playerId)
        {
            var player = await _playerManagerService.GetPlayerDataAsync(playerId);
            var response = new BaseResponse() { Success = false };

            var nextAd = player.NextBlueGemAdAt.ToDateTime();
            var now = DateTime.UtcNow;

            if (nextAd > now)
            {
                return response;
            }

            var gift = await _shopRepo.GetById(FreeGiftId);
            var gems = gift?.Reward?.Value > 0 ? (int)gift.Reward.Value : DefaultGems;
            var next = now.AddHours(4);
            player = _resourceUtil.AddBlueGems(player, gems);
            player.NextBlueGemAdAt = Timestamp.FromDateTime(next);
            player.NextBlueGemAdAvailableAt = next;

            var upsertData = new PlayerData()
            {
                Id = player.Id,
                BlueGems = player.BlueGems,
                NextBlueGemAdAt = player.NextBlueGemAdAt

            };

            var updatedData = await _playerManagerService.UpsertPlayerData(upsertData, player);

            response.Success = true;
            response.PlayerData = new PlayerDataDTO()
            {
                BlueGems = BindableInt.Of(updatedData.BlueGems),
                NextBlueGemAdAt = BindableDateTime.Of(updatedData.NextBlueGemAdAt.ToDateTime())
            };

            return response;
        }
    }
}
