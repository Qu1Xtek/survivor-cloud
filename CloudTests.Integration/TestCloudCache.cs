using CloudLogic.Cache;
using CloudLogic.RepoModels;
using CloudLogic.RepoModels.Achievements;
using CloudLogic.RepoModels.AuctionHouse;
using CloudLogic.RepoModels.Items;
using CloudLogic.RepoModels.Skin;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudTests.Integration
{
    /// <summary>
    /// Test cache implementation to avoid external Redis dependency.
    /// Keeps data in-memory for the current test process.
    /// </summary>
    internal sealed class TestCloudCache : ICloudCache
    {
        private List<ItemTemplate>? _templates;
        private Dictionary<string, List<Item>> _vendorItems = new();
        private List<GearScoreEntry>? _gearScore;
        private List<AHItem>? _auctionHouseItems;

        // We intentionally do NOT persist daily login config to avoid stale configs across runs.
        public Task<DailyLoginWeek> GetDailyLoginConfig() => Task.FromResult<DailyLoginWeek>(null);
        public Task<DailyLoginWeek> LoadDailyLoginConfigFromFirebase(DailyLoginWeek dailyLoginWeek) => Task.FromResult(dailyLoginWeek);

        public Task<List<ItemTemplate>> GetItemTemplates() => Task.FromResult(_templates);

        public Task<List<ItemTemplate>> LoadTemplatesFromFirestoreAsync(List<ItemTemplate> templates)
        {
            _templates = templates;
            return Task.FromResult(templates);
        }

        public Task LoadVendorItemsForPlayer(string playerId, List<Item> items)
        {
            _vendorItems[playerId] = items;
            return Task.CompletedTask;
        }

        public Task<List<Item>> GetVendorItemsForPlayer(string playerId)
        {
            _vendorItems.TryGetValue(playerId, out var items);
            return Task.FromResult(items);
        }

        public Task SaveGearScore(List<GearScoreEntry> entries)
        {
            _gearScore = entries;
            return Task.CompletedTask;
        }

        public Task<List<GearScoreEntry>> GetGearScore()
        {
            return Task.FromResult(_gearScore ?? new List<GearScoreEntry>());
        }

        public Task<List<AHItem>> GetAuctionHouseItems()
        {
            return Task.FromResult(_auctionHouseItems ?? new List<AHItem>());
        }

        public Task AddAuctionHouseItem(AHItem item)
        {
            _auctionHouseItems ??= new List<AHItem>();
            _auctionHouseItems.Add(item);
            return Task.CompletedTask;
        }

        public Task RemoveAuctionHouseItem(string auctionId)
        {
            if (_auctionHouseItems != null)
                _auctionHouseItems = _auctionHouseItems.Where(i => i.Id != auctionId).ToList();
            return Task.CompletedTask;
        }

        public Task InvalidateAuctionHouseCache()
        {
            _auctionHouseItems = null;
            return Task.CompletedTask;
        }

        public Task<List<Achievement>> GetDailyQuests()
        {
            throw new NotImplementedException();
        }

        public Task SetDailyQuests(List<Achievement> quests)
        {
            throw new NotImplementedException();
        }

        public Task<List<SkinConfig>> GetSkinConfigs()
        {
            throw new NotImplementedException();
        }

        public Task SetSkinConfigs(List<SkinConfig> configs)
        {
            throw new NotImplementedException();
        }

        public Task InvalidateSkinConfigs()
        {
            throw new NotImplementedException();
        }
    }
}

