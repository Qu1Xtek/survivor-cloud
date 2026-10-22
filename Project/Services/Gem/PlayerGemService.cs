using CloudLogic.RepoModels.Gem;
using CloudLogic.Repositories;
using CloudLogic.Response;
using CloudLogic.Services.Interfaces;
using CloudLogic.Services.Items;
using CloudLogic.Util.ComModels;
using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudLogic.Services.Gem
{
    public class PlayerGemService : IPlayerGemService
    {
        private GemFactory _gemFactory;
        private PlayerGemsRepo _playerGemsRepo;
        private GemUpgrader _gemUpgrader;
        private IPlayerManagerService _playerService;
        private GemConfigsRepo _gemConfigsRepo;

        public PlayerGemService(PlayerGemsRepo repo, IPlayerManagerService playerManagerService, GemConfigsRepo gemConfigsRepo)
        {
            _gemFactory = new GemFactory();
            _gemUpgrader = new GemUpgrader();
            _playerGemsRepo = repo;
            _playerService = playerManagerService;
            _gemConfigsRepo = gemConfigsRepo;
        }
        public async Task<PlayerGems> GetPlayerGems(string playerId)
        {
            var result = await _playerGemsRepo.GetPlayerGemsByPlayerId(playerId);

            if (result == null)
            {
               return await CreatePlayerGems(playerId);
            }

            return result;
        }


        public async Task<UpgradeGemResponse> UpgradeGem(string playerId, GemType gemType, Transaction tran)
        {
            PlayerGems plGems = await _playerGemsRepo.GetPlayerGemsByPlayerId(playerId, tran);
            
            if (gemType == GemType.Main) 
            {
                return new UpgradeGemResponse() { Success = false };
            }

            PlayerGem gem = plGems.Gems[gemType.ToString()];

            var gemConfigs = await _gemConfigsRepo.GetGemConfigs();
            var costs = gemConfigs.GoldCostForLevel;

            if (costs.Count <= gem.Level)
            {
                return new UpgradeGemResponse() { Success = false };
            }

            var cost = costs[gem.Level - 1];

            ResourceUpdate result = await _playerService.TakeCraftingMaterialsAndGold(playerId, cost, 0, tran);

            if (result.Success)
            {
                plGems.Gems[gemType.ToString()].Level++;
                plGems.Gems[GemType.Main.ToString()].Level++;

                var savedGems = await _playerGemsRepo.SavePlayerGems(plGems);

                return new UpgradeGemResponse() 
                { 
                    Success = true,
                    Gold = result.Gold,
                    PlayerGem = savedGems.Gems[gemType.ToString()],
                    MainGem = savedGems.Gems[GemType.Main.ToString()]
                };
            }

            return new UpgradeGemResponse() { Success = false };
        }


        private async Task<PlayerGems> CreatePlayerGems(string playerId)
        {
            var plGems = await _playerGemsRepo.CreatePlayerGems(playerId);

            plGems.Gems = new Dictionary<string, PlayerGem>()
            {
                [GemType.Main.ToString()] = _gemFactory.CreateGem(GemType.Main),
                [GemType.Health.ToString()] = _gemFactory.CreateGem(GemType.Health),
                [GemType.Armor.ToString()] = _gemFactory.CreateGem(GemType.Armor),
                [GemType.Physical.ToString()] = _gemFactory.CreateGem(GemType.Physical),
                [GemType.Magical.ToString()] = _gemFactory.CreateGem(GemType.Magical),
                [GemType.AdditionalDamage.ToString()] = _gemFactory.CreateGem(GemType.AdditionalDamage),
            };

            plGems.Gems[GemType.Main.ToString()].Level = 5;

            return await _playerGemsRepo.SavePlayerGems(plGems);
        }


        //public async Task<PlayerGems> SelectGemMilestonePower(string playerId, GemType gemType, int level, int index)
        //{
        //    PlayerGems plGems = await _playerGemsRepo.GetPlayerGemsByPlayerId(playerId);
        //    PlayerGem gem = plGems.Gems[gemType.ToString()];


        //    if (gem.MilestonePowers[level.ToString()] != -1)
        //    {
        //        return plGems;
        //    }


        //    gem.MilestonePowers[level.ToString()] = index;

        //    return await _playerGemsRepo.SavePlayerGems(plGems);
        //}

        private long ComputeUpgradeCost(int currentLevel)
        {
            return currentLevel switch
            {
                // 1 worst case run — cost = 10 * mobLevel
                1 => 10,   // 10 * 1
                2 => 20,   // 10 * 2
                3 => 30,   // 10 * 3
                4 => 40,   // 10 * 4
                5 => 50,   // 10 * 5

                // 1.5 worst case runs — cost = 15 * mobLevel
                6 => 90,   // 15 * 6
                7 => 105,  // 15 * 7
                8 => 120,  // 15 * 8
                9 => 135,  // 15 * 9
                10 => 150,  // 15 * 10

                // 2 worst case runs — cost = 20 * mobLevel
                11 => 220,  // 20 * 11
                12 => 240,  // 20 * 12
                13 => 260,  // 20 * 13
                14 => 280,  // 20 * 14
                15 => 300,  // 20 * 15

                // 3 worst case runs — cost = 30 * mobLevel
                16 => 480,  // 30 * 16
                17 => 510,  // 30 * 17
                18 => 540,  // 30 * 18
                19 => 570,  // 30 * 19
                20 => 600,  // 30 * 20

                // 8 worst case runs — cost = 80 * mobLevel
                21 => 1680, // 80 * 21
                22 => 1760, // 80 * 22
                23 => 1840, // 80 * 23
                24 => 1920, // 80 * 24
                25 => 2000, // 80 * 25

                // 20 worst case runs — cost = 200 * mobLevel
                26 => 5200, // 200 * 26
                27 => 5400, // 200 * 27
                28 => 5600, // 200 * 28
                29 => 5800, // 200 * 29
                30 => 6000,

                _ => 9999999999999
            };
        }


    }
}
