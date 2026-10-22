using CloudLogic.RepoModels.Player;
using CloudLogic.RepoModels.PlayerRewards;
using CloudLogic.RepoModels.Purchase;
using CloudLogic.Repositories;
using CloudLogic.Response.DTO;
using CloudLogic.Util;
using System.Threading.Tasks;

namespace CloudLogic.Services.Player
{
    public class ReviveService
    {
        private readonly PlayerDataRepo _repo;
        private readonly GemBoughtProductRepo _gemBoughtProductRepo;

        public ReviveService(PlayerDataRepo repo, GemBoughtProductRepo gemBoughtProductRepo)
        {
            _repo = repo;
            _gemBoughtProductRepo = gemBoughtProductRepo;
        }

        public async Task<ShopResult> UseRevive(string playerId)
        {
            var result = new ShopResult { Success = false };

            await _repo.RunOurTransactionAsync(async tran =>
            {
                var player = await _repo.GetPlayerDataByPlayerId(playerId, tran);
                var refusal = Refusal(player);

                if (refusal != null)
                {
                    result = new ShopResult { Success = false, AdditionalInfo = refusal };
                    return;
                }

                var update = Spend(player);
                await _repo.UpsertPlayerData(update, player, tran);

                if (update.BlueGems < player.BlueGems)
                {
                    await _gemBoughtProductRepo.Add(playerId, GemBoughtProduct.Revive, player.BlueGems - update.BlueGems, update.BlueGems, tran);
                }

                result = new ShopResult { Success = true, Type = RewardType.Revive, RewardVal = BindableLong.Of(update.Revives), BlueGems = update.BlueGems };
            });

            return result;
        }

        public static string Refusal(PlayerData player)
        {
            if (player == null)
            {
                return "no_player";
            }

            if (player.RunRevives >= GameConstants.ReviveCosts.Length)
            {
                return "limit";
            }

            if (player.Revives <= 0 && player.BlueGems < GameConstants.ReviveCosts[player.RunRevives])
            {
                return "not_enough_gems";
            }

            return null;
        }

        public static PlayerData Spend(PlayerData player)
        {
            var owned = player.Revives > 0;
            var update = new PlayerData
            {
                Id = player.Id,
                RunRevives = player.RunRevives + 1,
                Revives = owned ? player.Revives - 1 : player.Revives,
                BlueGems = owned ? player.BlueGems : player.BlueGems - GameConstants.ReviveCosts[player.RunRevives],
            };

            update.ExplicitValuesUpsert(new[] { nameof(PlayerData.Revives), nameof(PlayerData.BlueGems) });
            return update;
        }
    }
}
