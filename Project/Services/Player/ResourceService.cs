using CloudLogic.RepoModels.Player;
using CloudLogic.Repositories;
using CloudLogic.Services.Items;
using CloudLogic.Util;
using Google.Cloud.Firestore;
using System;
using System.Threading.Tasks;

namespace CloudLogic.Services.Player
{
    public class ResourceService
    {
        private ResourceUtil _resourceUtil;
        private readonly PlayerDataRepo _repo;

        public ResourceService(PlayerDataRepo repo)
        {
            _repo = repo;
            _resourceUtil = new ResourceUtil();
        }


        public async Task<long> AddBlueGems(string playerId, int blueGems)
        {
            var playerData = await _repo.GetPlayerDataByPlayerId(playerId);
         
            playerData = _resourceUtil.AddBlueGems(playerData, blueGems);

            var newPlrData = new PlayerData()
            {
                Id = playerId,
                BlueGems = playerData.BlueGems
            };

            var updatedData = await _repo.UpsertPlayerData(newPlrData, playerData);

            return updatedData.BlueGems;
        }

        public async Task SubtractBlueGems(string playerId, int amount)
        {
            var playerData = await _repo.GetPlayerDataByPlayerId(playerId);
            int newBalance = Math.Max(0, playerData.BlueGems - amount);

            var newPlrData = new PlayerData { Id = playerId, BlueGems = newBalance };
            await _repo.UpsertPlayerData(newPlrData, playerData);
        }

        public async Task<BaseResponse> AddGold(string playerId, long gold, Transaction tran = null)
        {
            var playerData = await _repo.GetPlayerDataByPlayerId(playerId, tran);

            playerData = _resourceUtil.AddGold(playerData, gold);

            var newPlrData = new PlayerData()
            {
                Id = playerId,
                Gold = playerData.Gold
            };

            var updatedData = await _repo.UpsertPlayerData(newPlrData, playerData, tran);

            return new BaseResponse()
            {
                Success = true,
                PlayerData = new Response.PlayerDataDTO() { Gold = BindableLong.Of(updatedData.Gold) },
            };
        }

        public async Task<long> AddGold(PlayerData playerData, long gold, Transaction tran = null)
        {
            playerData = _resourceUtil.AddGold(playerData, gold);

            var newPlrData = new PlayerData()
            {
                Id = playerData.Id,
                Gold = playerData.Gold
            };

            var updatedData = await _repo.UpsertPlayerData(newPlrData, playerData, tran);

            return updatedData.Gold;
        }

        public async Task<long> AddCraftingMaterials(string playerId, int craftingMat)
        {
            var playerData = await _repo.GetPlayerDataByPlayerId(playerId);

            playerData = _resourceUtil.AddCraftingMaterials(playerData, craftingMat);

            var newPlrData = new PlayerData()
            {
                Id = playerId,
                CraftingMaterials = playerData.CraftingMaterials
            };

            var updatedData = await _repo.UpsertPlayerData(newPlrData, playerData);

            return updatedData.CraftingMaterials;
        }

        public async Task<long> AddRerollRunes(string playerId, int runes)
        {
            var playerData = await _repo.GetPlayerDataByPlayerId(playerId);
            var update = new PlayerData { Id = playerId, RerollRuneCount = playerData.RerollRuneCount + runes };
            var updatedData = await _repo.UpsertPlayerData(update, playerData);
            return updatedData.RerollRuneCount;
        }

        public async Task<long> AddRevives(string playerId, int revives)
        {
            var playerData = await _repo.GetPlayerDataByPlayerId(playerId);
            var update = new PlayerData { Id = playerId, Revives = playerData.Revives + revives };
            var updatedData = await _repo.UpsertPlayerData(update, playerData);
            return updatedData.Revives;
        }

        public async Task<long> AddStashSlots(string playerId, int slots)
        {
            var playerData = await _repo.GetPlayerDataByPlayerId(playerId);
            var extra = Math.Min(GameConstants.MaxExtraStashSlots, playerData.ExtraStashSlots + slots);
            var update = new PlayerData { Id = playerId, ExtraStashSlots = extra };
            var updatedData = await _repo.UpsertPlayerData(update, playerData);
            return updatedData.StashLimit;
        }

        public async Task<DateTime> AddForgeCardDays(string playerId, int days)
        {
            var playerData = await _repo.GetPlayerDataByPlayerId(playerId);
            var now = DateTime.UtcNow;
            var from = playerData.HasForgeCard(now) ? playerData.ForgeCardUntil.ToDateTime() : now;
            var update = new PlayerData { Id = playerId, ForgeCardUntil = Timestamp.FromDateTime(from.AddDays(days)) };
            var updatedData = await _repo.UpsertPlayerData(update, playerData);
            return updatedData.ForgeCardUntil.ToDateTime();
        }

        public async Task SetPremium(string playerId)
        {
            var playerData = await _repo.GetPlayerDataByPlayerId(playerId);
            await _repo.UpsertPlayerData(new PlayerData { Id = playerId, Premium = true }, playerData);
        }

        public async Task<BaseResponse> GetGoldFromPig(string playerId)
        {
            BaseResponse response = new BaseResponse() { Success = false };

            await _repo.RunOurTransactionAsync(async tran =>
            {
                var playerData = await _repo.GetPlayerDataByPlayerId(playerId, tran);

                var gold = _resourceUtil.GetTotalPigGold(playerData);

                if (gold > 0)
                {
                    var plrDataUpsert = new PlayerData
                    {
                        Id = playerData.Id,
                        Gold = playerData.Gold + gold,
                        LastPigGoldTakenStamp = Timestamp.FromDateTime(DateTime.UtcNow),
                    };

                    var plrData = await _repo.UpsertPlayerData(plrDataUpsert, playerData, tran);

                    response.Success = true;
                    response.PlayerData = new Response.PlayerDataDTO()
                    {
                        Gold = BindableLong.Of(plrData.Gold),
                        LastPigGoldTakenStamp = BindableDateTime.Of(plrData.LastPigGoldTakenStamp.ToDateTime())
                    };
                }
            });

            return response;
        }

        public async Task<BaseResponse> AddTalentResetToken(string playerId, int token)
        {
            var playerData = await _repo.GetPlayerDataByPlayerId(playerId);

            playerData = _resourceUtil.AddTalentResetToken(playerData, token);

            var newPlrData = new PlayerData()
            {
                Id = playerId,
                TalentResetToken = playerData.TalentResetToken
            };

            var updatedData =  await _repo.UpsertPlayerData(newPlrData, playerData);


            return new BaseResponse() 
            { 
                Success = true,
                PlayerData = new Response.PlayerDataDTO() { TalentResetToken = BindableInt.Of(updatedData.TalentResetToken) },
            };

        }
    }
}
