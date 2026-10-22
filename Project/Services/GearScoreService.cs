using CloudLogic.Cache;
using CloudLogic.RepoModels;
using CloudLogic.RepoModels.Items;
using CloudLogic.Repositories;
using CloudLogic.Response.Base;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;
using Unity.Services.Leaderboards.Model;

namespace CloudLogic.Services
{
    public class GearScoreService
    {
        private readonly ILogger _logger;
        private readonly IGameApiClient _gameApiClient;
        private readonly ItemsRepo _itemsRepo;
        private ICloudCache _cache;

        private readonly List<string> _equipmentKeys = new List<string>
        {
            ItemType.Head.ToString(),
            ItemType.Bracer.ToString(),
            ItemType.Belt.ToString(),
            ItemType.Boots.ToString(),
            ItemType.Hands.ToString(),
            ItemType.Ring.ToString(),
            ItemType.Amulet.ToString(),
            ItemType.Chest.ToString(),
            ItemType.Legs.ToString(),
            ItemType.Weapon.ToString(),
        };

        public GearScoreService(ILogger logger, ItemsRepo itemsRepo, ICloudCache cache)
        {
            _logger = logger;
            _gameApiClient = GameApiClient.Create();
            _itemsRepo = itemsRepo;
            _cache = cache;
        }

        public async Task<List<GearScoreEntry>> GetGearScores()
        {
            return await _cache.GetGearScore();
        }

        public async Task<int> SubmitGearScore(GearScoreEntry gearscore)
        {
            var myRealItems = await _itemsRepo
                .GetMyOwnItems(gearscore.PlayerId, gearscore.Items.Values.Select(i => i.Id)
                .ToList());

            var score = CalculateScore(myRealItems);
            gearscore.Score = score;

            var (shouldChange, entries) = await CheckGearScore(gearscore);

            if (shouldChange)
            {
                await _cache.SaveGearScore(entries);
            }

            return score;
        }

        public async Task<(bool shouldChange, List<GearScoreEntry> entries)> CheckGearScore(GearScoreEntry gearScoreEntry)
        {

            var scores = await _cache.GetGearScore() ?? new List<GearScoreEntry>();

            if (scores.Count == 0)
            {
                scores.Add(gearScoreEntry);
                return (true, scores);
            }

            if (gearScoreEntry.Score <= scores.Min(x => x.Score))
            {
                return (false, scores);
            }

            var playerIndex = scores.FirstOrDefault(x => x.PlayerId == gearScoreEntry.PlayerId);

            if (playerIndex != null)
            {
                playerIndex.Score = gearScoreEntry.Score;
                playerIndex.Items = gearScoreEntry.Items;
            }
            else
            {
                scores.Add(gearScoreEntry);
            }

            var readyScores = scores.OrderByDescending(x => x.Score).Take(5).ToList();
            

            return (true, readyScores);
        }

        private int CalculateScore(List<Item> items)
        {
            var score = 0;

            foreach (var item in items)
            {
                float levelScore = item.Level * 10;
                // Base + rarity bonus:
                // - Common => +10%
                // - Rare (2) => +20%
                // This matches the expectations used in unit tests.
                var rarityBonusFactor = 0.1f + ((int)item.Rarity * 0.05f);
                levelScore += levelScore * rarityBonusFactor;

                if (item.Type == ItemType.Weapon)
                {
                    levelScore += item.Damage;
                }

                score += (int)levelScore;
            }

            return score;
        }
    }
}


