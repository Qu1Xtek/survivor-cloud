using CloudLogic.RepoModels.Items;
using CloudLogic.RepoModels.Player;
using CloudLogic.Repositories;
using CloudLogic.Services.Interfaces;
using CloudLogic.Util;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CloudLogic.Services.Items
{
    public class OwnedItemsService
    {
        private readonly IGearService _gearService;
        private readonly ItemsRepo _itemsRepo;
        private readonly PlayerDataRepo _playerDataRepo;

        public OwnedItemsService(IGearService gearService,
            ItemsRepo itemRepo,
            PlayerDataRepo playerDataRepo)
        {
            _gearService = gearService;
            _itemsRepo = itemRepo;
            _playerDataRepo = playerDataRepo;
        }

        public async Task<PlayerItemsResponse> GetPlayerItemsSplit(string playerId)
        {
            var playerItemsResponse = new PlayerItemsResponse();

            var equippedItems = await _gearService.GetEquipedItems();
            var equippedItemsIds = new List<string>();

            foreach (var item in equippedItems.Keys)
            {
                if (equippedItems[item] != null)
                {
                    equippedItemsIds.Add(equippedItems[item].Id);
                }
            }

            playerItemsResponse.EquippedItems = equippedItems;
            playerItemsResponse.StashItems = await _itemsRepo.GetPlayerItemsExcludingIds(playerId, equippedItemsIds);

            return playerItemsResponse;
        }

        public async Task<bool> CanPickItem(string playerId)
        {
            var equipedItemIds = await _gearService.GetEquippedItemsIds();

            var stashItems = await _itemsRepo.GetPlayerItemsExcludingIds(playerId, equipedItemIds);
            var player = await _playerDataRepo.GetPlayerDataByPlayerId(playerId);

            if (stashItems.Count >= (player?.StashLimit ?? GameConstants.DefaultStashSize))
            {
                return false;
            }


            return true;
        }
    }
}