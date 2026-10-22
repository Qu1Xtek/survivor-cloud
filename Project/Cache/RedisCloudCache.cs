using CloudLogic.RepoModels.Achievements;
using CloudLogic.RepoModels.Items;
using CloudLogic.RepoModels.AuctionHouse;
using CloudLogic.RepoModels.Skin;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StackExchange.Redis;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Unity.Services.CloudSave.Model;
using CloudLogic.RepoModels;
using System.Linq;
using CloudLogic.DI;

namespace CloudLogic.Cache
{
    public class RedisCloudCache : ICloudCache, IDisposable
    {
        private readonly ConnectionMultiplexer _redis;
        private readonly IDatabase _db;

        private const string CacheKey = "global:itemTemplates";
        private const string SkinConfigsKey = "global:skinConfigs";
        private const string DailyQuestsKey = "global:dailyQuests";
        private const string GearScoreKey = "global:gearScore";
        private const string AuctionHouseKey = "global:auctionHouse:items";
        private const string DailyLoginWeekKey = "global:dailyLoginWeek";
        private const int CacheTTLSeconds = 3600; // 1 hour
        private readonly RequestContextAccessor _ctx;
        private ILogger Logger => _ctx.Logger;

        public RedisCloudCache(string redisConnectionString, RequestContextAccessor ctx)
        {
            _ctx = ctx;


            // Set up Redis connection with custom configuration
            //var options = ConfigurationOptions.Parse(redisConnectionString);
            var config = new ConfigurationOptions
            {
                EndPoints = { "redis-15349.c328.europe-west3-1.gce.redns.redis-cloud.com:15349" },
                Password = "agQxShqz4bxVQecQzHorU9zfOJExLUzR", // <--- Required
                Ssl = false,
                AbortOnConnectFail = false,
                ConnectTimeout = 30000,
                SyncTimeout = 30000
            };

            try
            {
                _redis = ConnectionMultiplexer.Connect(config);
                _db = _redis.GetDatabase();
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to connect to Redis: {ex.Message}");
                throw;
            }
        }

        public async Task<List<Achievement>> GetDailyQuests()
        {
            try
            {
                var cached = await _db.StringGetAsync(DailyQuestsKey);
                if (cached != RedisValue.Null)
                {
                    return JsonConvert.DeserializeObject<List<Achievement>>(cached);
                }

                return null;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error in GetDailyQuests: {ex.Message}");
                return null;
            }
        }

        public async Task SetDailyQuests(List<Achievement> quests)
        {
            try
            {
                var json = JsonConvert.SerializeObject(quests);
                await _db.StringSetAsync(DailyQuestsKey, json);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error in SetDailyQuests: {ex.Message}");
            }
        }

        public async Task<List<ItemTemplate>> GetItemTemplates()
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                // Try to get from Redis
                var cachedValue = await _db.StringGetAsync(CacheKey);

                if (!cachedValue.IsNull)
                {
                    try
                    {
                        stopwatch.Stop();
                        return JsonConvert.DeserializeObject<List<ItemTemplate>>(cachedValue);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError($"Failed to deserialize cached value: {ex.Message}");
                    }
                }

                // Cache miss - load from Firestore
                return null;

            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                Logger.LogError($"Error in GetItemTemplates: {ex.Message}");
                throw;
            }
        }

        public async Task<List<ItemTemplate>> LoadTemplatesFromFirestoreAsync(List<ItemTemplate> templates)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // Update cache
                var json = JsonConvert.SerializeObject(templates);
                await _db.StringSetAsync(CacheKey, json);

                stopwatch.Stop();

                return templates;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error loading templates from Firestore: {ex.Message}");
                throw;
            }
        }

        public async Task<List<RepoModels.Items.Item>> GetVendorItemsForPlayer(string playerId)
        {
            try
            {
                // Try to get from Redis
                var cachedValue = await _db.StringGetAsync($"{playerId}:vendorItems");

                if (cachedValue != RedisValue.Null)
                {
                    try
                    {
                        return JsonConvert.DeserializeObject<List<RepoModels.Items.Item>>(cachedValue);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError($"Failed to deserialize cached value: {ex.Message}");
                    }
                }

                // Cache miss - load from Firestore
                return null;

            }
            catch (Exception ex)
            {
                Logger.LogError($"Error in GetVendorItems: {ex.Message}");
                throw;
            }
        }

        public async Task LoadVendorItemsForPlayer(string playerId, List<RepoModels.Items.Item> items)
        {
            try
            {
                string redisKey = $"{playerId}:vendorItems";

                // Always serialize and set with TTL, whether key exists or not
                var json = JsonConvert.SerializeObject(items);
                await _db.StringSetAsync(redisKey, json, TimeSpan.FromSeconds(600));
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error in LoadVendorItemsForPlayer for {playerId}: {ex.Message}");
                throw;
            }
        }


        public async Task SaveGearScore(List<GearScoreEntry> entries)
        {
            try
            {
                string redisKey = GearScoreKey;

                var dict = new Dictionary<int, GearScoreEntry>();

                for (int i = 0; i < entries.Count; i++)
                {
                    dict.Add(i + 1, entries[i]);
                }

                // Always serialize and set with TTL, whether key exists or not
                var json = JsonConvert.SerializeObject(dict);
                await _db.StringSetAsync(redisKey, json);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error in LoadGearScore: {ex.Message}");
                throw;
            }
        }

        public async Task<List<GearScoreEntry>> GetGearScore()
        {
            try
            {
                // Try to get from Redis
                var cachedValue = await _db.StringGetAsync(GearScoreKey);

                if (cachedValue != RedisValue.Null)
                {
                    try
                    {

                        var dict = JsonConvert.DeserializeObject<Dictionary<int, GearScoreEntry>>(cachedValue);
                        var list = new List<GearScoreEntry>(5);

                        var x = dict.OrderBy(x => x.Key).ToList();
                        for (int i = 0; i < 5; i++)
                        {
                            if (x.Count > i)
                            {
                                list.Add(x[i].Value);
                            }
                            else
                            {
                                list.Add(new GearScoreEntry());
                            }
                        }
                        //foreach (var key in )
                        //{
                        //    list.Add(dict[key]);
                        //}


                        return list;
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError($"Failed to deserialize cached value: {ex.Message}");
                    }
                }

                // Cache miss - load from Firestore
                return new List<GearScoreEntry>() { new GearScoreEntry(){ Player = "EMPTY_PLAYER_DATA_YES_NO_YES", Score = 0, Items = null}};

            }
            catch (Exception ex)
            {
                Logger.LogError($"Error in GetGearScore: {ex.Message}");
                throw;
            }
        }




        /// <summary>
        /// Invalidates the item templates cache, forcing a refresh on next request
        /// </summary>
        public async Task InvalidateItemTemplatesCache()
        {
            await _db.KeyDeleteAsync(CacheKey);
        }

        public async Task<List<AHItem>> GetAuctionHouseItems()
        {
            try
            {
                // Try to get from Redis
                var cachedValue = await _db.StringGetAsync(AuctionHouseKey);

                if (cachedValue != RedisValue.Null)
                {
                    try
                    {
                        return JsonConvert.DeserializeObject<List<AHItem>>(cachedValue) ?? new List<AHItem>();
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError($"Failed to deserialize auction house cache: {ex.Message}");
                        return new List<AHItem>();
                    }
                }

                // Cache miss - return empty list (should not happen if cache is properly maintained)
                return new List<AHItem>();
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error in GetAuctionHouseItems: {ex.Message}");
                return new List<AHItem>();
            }
        }

        public async Task<DailyLoginWeek> GetDailyLoginConfig()
        {
            try
            {
                var cachedValue = await _db.StringGetAsync(DailyLoginWeekKey);

                if (cachedValue != RedisValue.Null)
                {
                    try
                    {
                        return JsonConvert.DeserializeObject<DailyLoginWeek>(cachedValue);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError($"Failed to deserialize auction house cache: {ex.Message}");
                        return null;
                    }
                }

                return null;
            } catch (Exception ex)
            {
                Logger.LogError($"Error in GetDailyLoginConfig: {ex.Message}");

                return null;
            }
        }
        public async Task<DailyLoginWeek> LoadDailyLoginConfigFromFirebase(DailyLoginWeek dailyLoginWeek)
        {
            try
            {
                // Update cache
                var json = JsonConvert.SerializeObject(dailyLoginWeek);
                await _db.StringSetAsync(DailyLoginWeekKey, json);

                return dailyLoginWeek;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error loading templates from Firestore: {ex.Message}");
                throw;
            }
        }

        public async Task AddAuctionHouseItem(AHItem item)
        {
            try
            {
                // Get current list from cache
                var cachedValue = await _db.StringGetAsync(AuctionHouseKey);
                var items = new List<AHItem>();

                if (cachedValue != RedisValue.Null)
                {
                    try
                    {
                        items = JsonConvert.DeserializeObject<List<AHItem>>(cachedValue) ?? new List<AHItem>();
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError($"Failed to deserialize auction house cache when adding item: {ex.Message}");
                        items = new List<AHItem>();
                    }
                }

                // Add the new item if it doesn't already exist
                if (!items.Any(x => x.Id == item.Id))
                {
                    items.Add(item);
                }
                else
                {
                    // Update existing item
                    var existingIndex = items.FindIndex(x => x.Id == item.Id);
                    if (existingIndex >= 0)
                    {
                        items[existingIndex] = item;
                    }
                }

                // Save back to cache
                var json = JsonConvert.SerializeObject(items);
                await _db.StringSetAsync(AuctionHouseKey, json);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error in AddAuctionHouseItem: {ex.Message}");
                throw;
            }
        }

        public async Task RemoveAuctionHouseItem(string auctionId)
        {
            try
            {
                // Get current list from cache
                var cachedValue = await _db.StringGetAsync(AuctionHouseKey);
                
                if (cachedValue == RedisValue.Null)
                {
                    // Cache is empty, nothing to remove
                    return;
                }

                var items = JsonConvert.DeserializeObject<List<AHItem>>(cachedValue) ?? new List<AHItem>();
                
                // Remove the item
                items.RemoveAll(x => x.Id == auctionId);

                // Save back to cache
                var json = JsonConvert.SerializeObject(items);
                await _db.StringSetAsync(AuctionHouseKey, json);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error in RemoveAuctionHouseItem: {ex.Message}");
                throw;
            }
        }

        public async Task InvalidateAuctionHouseCache()
        {
            try
            {
                await _db.KeyDeleteAsync(AuctionHouseKey);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error in InvalidateAuctionHouseCache: {ex.Message}");
                throw;
            }
        }

        public async Task<List<SkinConfig>> GetSkinConfigs()
        {
            try
            {
                var cached = await _db.StringGetAsync(SkinConfigsKey);
                if (cached != RedisValue.Null)
                {
                    return JsonConvert.DeserializeObject<List<SkinConfig>>(cached);
                }

                return null;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error in GetSkinConfigs: {ex.Message}");
                return null;
            }
        }

        public async Task SetSkinConfigs(List<SkinConfig> configs)
        {
            try
            {
                var json = JsonConvert.SerializeObject(configs);
                await _db.StringSetAsync(SkinConfigsKey, json);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error in SetSkinConfigs: {ex.Message}");
            }
        }

        public void Dispose()
        {
            _redis?.Dispose();
        }


    }
}