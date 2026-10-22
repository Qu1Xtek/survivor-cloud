using System;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CloudLogic.RepoModels.Items;

namespace CloudLogic.Repositories
{
    public class ItemsRepo : BaseFireStoreService
    {
        public ItemsRepo(ILogger logger) : base("Items", logger)
        {
        }

        public virtual async Task<Item> StoreItem(Item item)
        {
            return await StoreRecord(item);
        }     

        public virtual async Task<List<Item>> StoreItems(List<Item> items)
        {
            return await StoreRecords(items);
        }

        public virtual async Task<List<Item>> GetPlayerItems(string playerId)
        {
            if (string.IsNullOrEmpty(playerId))
            {
                _logger.LogWarning("GetPlayerItems called with null or empty playerId");
                return new List<Item>();
            }

            var itemsRef = GetCollectionRef();
            Query query = itemsRef.WhereEqualTo("OwnerId", playerId);

            return await GetRecordsByQuery<Item>(query);
        }

        public virtual async Task<List<Item>> GetMyOwnItems(string playerId, List<string> itemIds)
        {
            if (string.IsNullOrEmpty(playerId))
            {
                _logger.LogWarning("GetPlayerItems called with null or empty playerId");
                return new List<Item>();
            }

            if (itemIds == null || itemIds.Count == 0)
            {
                _logger.LogWarning("GetMyOwnItems called with null or empty itemIds list");
                return new List<Item>();
            }

            var filteredIds = itemIds
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList();

            if (filteredIds.Count == 0)
            {
                return new List<Item>();
            }

            var itemsRef = GetCollectionRef();
            var results = new List<Item>();

            foreach (var chunk in filteredIds.Chunk(30))
            {
                Query query = itemsRef
                    .WhereEqualTo("OwnerId", playerId)
                    .WhereIn("Id", chunk.ToList());

                var chunkResults = await GetRecordsByQuery<Item>(query);

                if (chunkResults != null && chunkResults.Count > 0)
                {
                    results.AddRange(chunkResults);
                }
            }

            return results;
        }

        public virtual async Task<Item> GetPlayerItem(string playerId, string itemId)
        {
            if (string.IsNullOrEmpty(playerId))
            {
                _logger.LogWarning("GetPlayerItems called with null or empty playerId");
                return new Item();
            }

            var itemsRef = GetCollectionRef();
            Query query = itemsRef
                .WhereEqualTo("OwnerId", playerId)
                .WhereEqualTo("Id", itemId);

            var items = (await GetRecordsByQuery<Item>(query)).FirstOrDefault();

            return items;
        }

        public async Task<List<Item>> GetPlayerItemsExcludingIds(string ownerId, IEnumerable<string> excludedItemIds)
        {
            if (string.IsNullOrWhiteSpace(ownerId))
            {
                _logger.LogWarning("GetPlayerItemsExcludingIds called with null or empty ownerId");
                return new List<Item>();
            }

            var items = await GetPlayerItems(ownerId) ?? new List<Item>();

            if (items.Count == 0)
            {
                return items;
            }

            if (excludedItemIds == null)
            {
                return items;
            }

            var exclusionSet = new HashSet<string>(excludedItemIds.Where(id => !string.IsNullOrWhiteSpace(id)));

            if (exclusionSet.Count == 0)
            {
                return items;
            }

            return items.Where(item => item != null && !string.IsNullOrWhiteSpace(item.Id) && !exclusionSet.Contains(item.Id)).ToList();
        }

        internal virtual async Task<Item> GetItemById(string itemId, Transaction tran = null)
        {
            var document = await ReadRecordById(itemId, tran);

            return ConvertDocument<Item>(document);
        }

        public async Task<List<Item>> GetItemsByIds(List<string> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                _logger.LogWarning("GetItemsByIds called with null or empty ids list");
                return new List<Item>();
            }

            var documents = await ReadRecordsByIds(ids);

            return ConvertDocuments<Item>(documents) ?? new List<Item>();
        }


        public virtual async Task<bool> DeleteItemById(string itemId, Transaction tran = null)
        {
            return await DeleteRecordById(itemId, tran);
        }

        public async Task<int> TransferPlayerItemsAsync(string fromPlayerId, string toPlayerId)
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

        public virtual async Task<bool> DeleteItemsByIds(List<string> itemIds, Transaction tran = null)
        {
            return await DeleteRecordsByIds(itemIds, tran);
        }

        public async Task RemoveItemOwnerAsync(Item item)
        {
            item.OwnerId = null;
            
            item.ExplicitUpserts = new List<string> { nameof(Item.OwnerId) };

            await UpsertRecord(item);
        }

        internal virtual async Task SetItemOwner(Item item, string ownerId, Transaction tran = null)
        {
            item.OwnerId = ownerId;
            await UpdateRecord(item, tran);
        }

        internal virtual async Task<Item> UpdateItem(Item item)
        {
            return await UpdateRecord(item);
        }
    }
}
