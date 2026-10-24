using CloudLogic.RepoModels.AuctionHouse;
using CloudLogic.RepoModels.Items;
using CloudLogic.RepoModels.Player;
using CloudLogic.Repositories;
using CloudLogic.Cache;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CloudLogic.Services;
using CloudLogic.Services.Items;
using CloudLogic.Util;
using System.Diagnostics;

namespace CloudLogic.Services.Trading
{    
    public class AuctionHouseService : IAuctionHouseService
    {
        private readonly ItemsRepo _itemsRepo;
        private readonly AuctionHouseRepo _ahRepo;
        private readonly IPlayerManagerService _playerManagerService;
        private readonly IMailService _mailService;
        private readonly ICloudCache _cache;
        private readonly ILogger _logger;
        private readonly ResourceUtil _resourceUtil;

        public AuctionHouseService(
            AuctionHouseRepo ahRepo,
            IPlayerManagerService playerManagerService,
            ItemsRepo itemsRepo,
            IMailService mailService,
            ICloudCache cache,
            ILogger logger)
        {
            _ahRepo = ahRepo;
            _playerManagerService = playerManagerService;
            _itemsRepo = itemsRepo;
            _mailService = mailService;
            _cache = cache;
            _logger = logger;
            _resourceUtil = new ResourceUtil();
        }

        private const int MinAuctionPrice = 1;
        private const int MaxAuctionPrice = 1_000_000_000;

        public async Task DoInit()
        {
            await _ahRepo.InitializeCacheFromDatabase();
        }

        public async Task<string> PostAHItem(string ownerId, AHItem ahItem)
        {
            if (ahItem == null || string.IsNullOrWhiteSpace(ahItem.ItemId))
            {
                return "Invalid auction";
            }

            if (ahItem.Price < MinAuctionPrice || ahItem.Price > MaxAuctionPrice)
            {
                return "Invalid price";
            }

            AHItem createdAuction = null;
            Item item = null;

            try
            {
                await _ahRepo.RunOurTransactionAsync(async tran =>
                {
                    // STEP 1: READS FIRST
                    item = await _itemsRepo.GetItemById(ahItem.ItemId, tran);

                    // A listed item has its owner cleared, so this also blocks double-listing.
                    if (item == null || item.OwnerId != ownerId)
                    {
                        throw new Exception("Item not owned or already listed");
                    }

                    if (item.Type == ItemType.DuelWeapon)
                    {
                        throw new Exception("Daggers can't be listed");
                    }

                    var playerData = await _playerManagerService.GetPlayerDataAsync(ownerId, null, tran);
                    if (playerData.AuctionsCount >= playerData.AuctionLimit)
                    {
                        throw new Exception("Auction limit reached");
                    }

                    // STEP 2: WRITES
                    ahItem.ItemId = item.Id;
                    createdAuction = _ahRepo.CreateAuction(ownerId, ahItem, tran);

                    // Clear ownership so the item cannot be relisted, sold, or used while on auction.
                    await _itemsRepo.SetItemOwner(item, null, tran);

                    var newPlayerData = new PlayerData()
                    {
                        Id = playerData.Id,
                        AuctionsCount = playerData.AuctionsCount + 1
                    };
                    await _playerManagerService.UpsertPlayerData(newPlayerData, tran);
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"PostAHItem error: {ex.Message}");
                return "Could not post auction";
            }

            // Cache only after the DB transaction committed successfully.
            createdAuction.SoldItem = item;
            await _cache.AddAuctionHouseItem(createdAuction);

            return "Auction posted!";
        }
        public async Task<Item> BuyAHItem(string buyerId, string auctionId)
        {
            PlayerData sellerData = null;
            Item item = null;
            int price;
            try
            {
                var ahItem = await _ahRepo.GetByIdAsync(auctionId);
                if (ahItem == null)
                {
                    return null;
                }

                if (ahItem.OwnerId == buyerId)
                {
                    return null;//"Cannot buy your own auction";
                }

                price = ahItem.Price;

                try
                {
                    await _ahRepo.RunOurTransactionAsync(async tran =>
                    {
                        // STEP 1: PERFORM ALL READS FIRST
                        _logger.LogInformation("Starting transaction - performing all reads first");

                        // Read auction item
                        var currentAhItem = await _ahRepo.GetByIdAsync(auctionId, tran);
                        if (currentAhItem == null)
                        {
                            throw new Exception("Auction item was removed");
                        }

                        if (currentAhItem.OwnerId == buyerId)
                        {
                            throw new Exception("Cannot buy your own auction");
                        }

                        // Read item
                        item = await _itemsRepo.GetItemById(ahItem.ItemId, tran);
                        if (item == null)
                        {
                            throw new Exception("Item not found");
                        }

                        // Read buyer data
                        _logger.LogInformation($"Checking if {buyerId} has enough gold");
                        var buyerData = await _playerManagerService.GetPlayerDataAsync(buyerId, null, tran);
                        if (buyerData.Gold < ahItem.Price)
                        {
                            throw new Exception($"Insufficient gold: you have {buyerData.Gold}, need {ahItem.Price}");
                        }

                        // Read seller data
                        sellerData = await _playerManagerService.GetPlayerDataAsync(ahItem.OwnerId, null, tran);

                        // STEP 2: PERFORM ALL WRITES
                        _logger.LogInformation("All reads completed, now performing writes");

                        // Update buyer gold
                        _logger.LogInformation($"{buyerId} spending {ahItem.Price} gold");
                        buyerData = _resourceUtil.TakeGold(buyerData, ahItem.Price);

                        var newBuyerrData = new PlayerData()
                        {
                            Id = buyerData.Id,
                            Gold = buyerData.Gold
                        };

                        // Spending down to exactly 0 gold must still be written.
                        newBuyerrData.ExplicitUpserts = new List<string> { nameof(PlayerData.Gold) };

                        await _playerManagerService.UpsertPlayerData(newBuyerrData, tran);

                        _logger.LogInformation($"Freeing an auction slot for seller {ahItem.OwnerId}");

                        var newSellerData = new PlayerData()
                        {
                            Id = sellerData.Id,
                            AuctionsCount = Math.Max(0, sellerData.AuctionsCount - 1)
                        };

                        // The freed slot can bring the count to 0, which a partial upsert would skip.
                        newSellerData.ExplicitUpserts = new List<string> { nameof(PlayerData.AuctionsCount) };

                        await _playerManagerService.UpsertPlayerData(newSellerData, tran);

                        // Update item owner
                        _logger.LogInformation($"Setting item {item.Id} owner to {buyerId}");
                        await _itemsRepo.SetItemOwner(item, buyerId, tran);

                        // Remove auction
                        _logger.LogInformation($"Removing auction {auctionId}");
                        await _ahRepo.RemoveAuction(auctionId, tran);

                        _logger.LogInformation("All transaction operations completed successfully");
                    });

                    _logger.LogInformation("Transaction committed successfully");

                    // Remove from cache after DB transaction is completed
                    await _cache.RemoveAuctionHouseItem(auctionId);

                }
                catch (Exception ex)
                {
                    _logger.LogError($"Transaction error: {ex.Message}");
                    return null; /// $"Transaction error: {ex.Message}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"BuyAHItem error: {ex.Message}");
                return null;// $"Error processing purchase: {ex.Message}";
            }

            MailNotifDto dto = new MailNotifDto()
            {
                Title = "Auction Sold",
                Message = $"Listing of {item?.Name} sold successfully for {price} gold!",
                Claim = new System.Collections.Generic.Dictionary<string, int>() { { "gold", price } },
                Gold = price,
                Type = "Auction",
                DeviceId = sellerData.DeviceId,
                PlayerId = sellerData.Id
            };

            await _mailService.CreateMail(dto);

            return item;
        }

        public async Task<bool> TransferAuctions(string fromPlayerId, string transferTo)
        {
            if (string.IsNullOrWhiteSpace(fromPlayerId))
            {
                throw new ArgumentException("fromPlayerId cannot be null or empty.", nameof(fromPlayerId));
            }

            if (string.IsNullOrWhiteSpace(transferTo))
            {
                throw new ArgumentException("transferTo cannot be null or empty.", nameof(transferTo));
            }

            if (string.Equals(fromPlayerId, transferTo, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var transferredCount = await _ahRepo.TransferPlayerAuctionsAsync(fromPlayerId, transferTo);

            return transferredCount > 0;
        }

        public async Task<string> CancelAuction(string? playerId, string auctionId)
        {
            try
            {
                var ahItem = await _ahRepo.GetByIdAsync(auctionId);
                

                try
                {
                    await _ahRepo.RunOurTransactionAsync(async tran =>
                    {                        
                        var currentAhItem = await _ahRepo.GetByIdAsync(auctionId, tran);

                        if (currentAhItem == null || currentAhItem.OwnerId != playerId)
                        {
                            _logger.LogWarning($"CANCEL Auction item not found or doesn't belong to same player!" +
                                $" PLR: {playerId}, AUC: {currentAhItem?.OwnerId}");

                            return;
                        }

                        // Read item
                        var item = await _itemsRepo.GetItemById(ahItem.ItemId, tran);
                        if (item == null)
                        {
                            _logger.LogError($"Player {playerId} " +
                                $"couldn't cancel his auction {ahItem?.Id}," +
                                $" item: {ahItem?.ItemId}not found");

                            return;
                        }

                        await _itemsRepo.SetItemOwner(item, playerId, tran);
                        await _ahRepo.RemoveAuction(auctionId, tran);
                    });

                    // Remove from cache after DB transaction is completed
                    await _cache.RemoveAuctionHouseItem(auctionId);

                    return "Item purchased successfully!";
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Transaction error: {ex.Message}");
                    return $"Transaction error: {ex.Message}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"BuyAHItem error: {ex.Message}");
                return $"Error processing purchase: {ex.Message}";
            }
        }
    }
}
