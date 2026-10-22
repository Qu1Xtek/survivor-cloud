using CloudLogic.RepoModels;
using CloudLogic.RepoModels.Achievements;
using CloudLogic.RepoModels.Items;
using CloudLogic.RepoModels.AuctionHouse;
using CloudLogic.RepoModels.Skin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudLogic.Cache
{
    public interface ICloudCache
    {
        Task<List<Achievement>> GetDailyQuests();
        Task SetDailyQuests(List<Achievement> quests);
        public Task<List<ItemTemplate>> GetItemTemplates();
        public Task<List<ItemTemplate>> LoadTemplatesFromFirestoreAsync(List<ItemTemplate> templates);
        public Task LoadVendorItemsForPlayer(string playerId, List<Item> items);

        public Task<List<Item>> GetVendorItemsForPlayer(string playerId);

        public Task SaveGearScore(List<GearScoreEntry> entries);

        public Task<List<GearScoreEntry>> GetGearScore();

        public Task<List<AHItem>> GetAuctionHouseItems();
        public Task AddAuctionHouseItem(AHItem item);
        public Task RemoveAuctionHouseItem(string auctionId);
        public Task InvalidateAuctionHouseCache();

        public Task<DailyLoginWeek> GetDailyLoginConfig();
        public Task<DailyLoginWeek> LoadDailyLoginConfigFromFirebase(DailyLoginWeek dailyLoginWeek);

        public Task<List<SkinConfig>> GetSkinConfigs();
        public Task SetSkinConfigs(List<SkinConfig> configs);
    }
}
