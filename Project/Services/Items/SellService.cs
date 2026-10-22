using CloudLogic.RepoModels.Items;
using CloudLogic.Repositories;
using CloudLogic.Services.Interfaces;
using CloudLogic.Services.Player;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.CloudCode.Core;

namespace CloudLogic.Services.Items
{
    public class SellService
    {

        private readonly ILogger _logger;
        private ItemsRepo _itemsRepo;
        private ResourceService _resourceService;


        public SellService(ItemsRepo itemsRepo, ResourceService resourceService, ILogger logger)
        {
            _itemsRepo = itemsRepo;
            _logger = logger;
            _resourceService = resourceService;
        }

        public async Task<bool> SellPlayerItem(string itemId, string playerId)
        {
            _logger.LogInformation($"SELL ITEM {playerId}: is selling {itemId}");
            var item = await _itemsRepo.GetPlayerItem(playerId, itemId);

            if (item == null || item.OwnerId != playerId)
            {
                return false;
            }

            await _resourceService.AddGold(playerId, item.GoldValue);
            await _itemsRepo.DeleteItemById(itemId);

            return true;
        }

        public async Task<bool> SellMassItems(List<string> itemIds, string playerId)
        {
            var allItems = await _itemsRepo.GetMyOwnItems(playerId, itemIds);
            var totalGold = allItems.Sum(x => x.GoldValue);

            await _itemsRepo.RunOurTransactionAsync(async tran =>
            {                
                await _resourceService.AddGold(playerId, totalGold, tran);
                await _itemsRepo.DeleteItemsByIds(allItems.Select(i => i.Id).ToList(), tran);
            });

            return true;
        }
    }
}