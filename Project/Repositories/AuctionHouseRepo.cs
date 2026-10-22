using System;
using CloudLogic.RepoModels.AuctionHouse;
using CloudLogic.RepoModels.Items;
using CloudLogic.Cache;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CloudLogic.Repositories
{
    public class AuctionHouseRepo : BaseFireStoreService
    {
        private readonly ICloudCache _cache;

        public AuctionHouseRepo(ILogger logger, ICloudCache cache) : base("Auctions", logger)
        {
            _cache = cache;
        }

        internal async Task<AHItem> CreateAuction(string ownerId, AHItem ahItem, Item item)
        {
            if (ahItem.Price < 1)
            {
                return null;
            }

            ahItem.OwnerId = ownerId;
            _logger.LogInformation($"Setting the ownerId to - {ahItem.OwnerId}");
            var refs = new Dictionary<string, string>()
            {
                { "ItemRef", ahItem.ItemId }
            };

            return await StoreRecord(ahItem, refs, "Items");
        }

        internal AHItem CreateAuction(string ownerId, AHItem ahItem, Transaction tran)
        {
            ahItem.OwnerId = ownerId;
            var refs = new Dictionary<string, string>()
            {
                { "ItemRef", ahItem.ItemId }
            };

            return StoreRecord(ahItem, tran, refs, "Items");
        }

        internal async Task<List<AHItem>> GetRecentAuctions()
        {
            var auctions =  await GetRecordsWithRefs<AHItem, Item>("ItemRef", nameof(AHItem.SoldItem));

            return auctions;
            // Populate the referenced items
         
        }

        internal virtual async Task<AHItem> GetByIdAsync(string id, Transaction tran = null)
        {
            var auctions = await GetRecordByIdWithRef<AHItem, Item>(id, "ItemRef", nameof(AHItem.SoldItem), tran);

            return auctions;
            // Populate the referenced items

        }

        internal virtual async Task RemoveAuction(string auctionId, Transaction tran = null)
        {        
            await DeleteRecordById(auctionId, tran);
        }

        internal virtual async Task<int> TransferPlayerAuctionsAsync(string fromPlayerId, string toPlayerId)
        {
            if (string.IsNullOrWhiteSpace(fromPlayerId))
            {
                throw new ArgumentException("Source player id cannot be null or empty.", nameof(fromPlayerId));
            }

            if (string.IsNullOrWhiteSpace(toPlayerId))
            {
                throw new ArgumentException("Target player id cannot be null or empty.", nameof(toPlayerId));
            }

            if (string.Equals(fromPlayerId, toPlayerId, StringComparison.OrdinalIgnoreCase))
            {
                return 0;
            }

            var collection = GetCollectionRef();

                var query = collection.WhereEqualTo("OwnerId", fromPlayerId);
                var snapshot = await query.GetSnapshotAsync();
                var documents = snapshot.Documents;

                if (documents.Count == 0)
                {
                    return 0;
                }

                var batch = _firestoreDb.StartBatch();

                foreach (var document in documents)
                {
                    batch.Update(document.Reference, new Dictionary<string, object>
                    {
                        { "OwnerId", toPlayerId }
                    });
                }

                await batch.CommitAsync();

                return documents.Count;
            
        }

        /// <summary>
        /// Initializes the cache with all auction house items from the database.
        /// This method retrieves all auctions from DB (with populated SoldItem references) and stores them in cache.
        /// </summary>
        public async Task InitializeCacheFromDatabase()
        {
            try
            {
                _logger.LogInformation("Starting cache initialization from database...");

                // Clear existing cache first
                await _cache.InvalidateAuctionHouseCache();

                // Get all auctions from DB (this already populates the SoldItem reference)
                var auctions = await GetRecentAuctions();

                if (auctions == null || auctions.Count == 0)
                {
                    _logger.LogInformation("No auctions found in database to cache.");
                    return;
                }

                // Add each auction to cache
                int cachedCount = 0;
                foreach (var auction in auctions)
                {
                    try
                    {
                        await _cache.AddAuctionHouseItem(auction);
                        cachedCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Failed to cache auction {auction.Id}: {ex.Message}");
                    }
                }

                _logger.LogInformation($"Cache initialization completed. Cached {cachedCount} out of {auctions.Count} auctions.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error initializing auction house cache from database: {ex.Message}");
                throw;
            }
        }
    }
}
