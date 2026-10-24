using CloudLogic.RepoModels.Duel;
using CloudLogic.RepoModels.Items;
using CloudLogic.RepoModels.Player;
using CloudLogic.Repositories;
using CloudLogic.Response.DTO;
using CloudLogic.Services.Duel.Arena;
using CloudLogic.Services.Duel.Spells;
using CloudLogic.Services.Interfaces;
using CloudLogic.Services.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudLogic.Services.Duel
{
    public class DuelService
    {
        private readonly DuelerRepo _duelers;
        private readonly PlayerDataRepo _players;
        private readonly IGearService _gear;
        private readonly ResourceService _resources;

        public DuelService(DuelerRepo duelers, PlayerDataRepo players, IGearService gear, ResourceService resources)
        {
            _duelers = duelers;
            _players = players;
            _gear = gear;
            _resources = resources;
        }

        public async Task<List<DuelNpcDTO>> GetNpcs(string playerId)
        {
            var npcs = _duelers.GetActive();
            var player = _players.GetPlayerDataByPlayerId(playerId);
            await Task.WhenAll(npcs, player);
            var defeated = player.Result?.DefeatedDuelers ?? new List<string>();
            return npcs.Result.Select(n => DuelMapper.ToNpc(n, defeated.Contains(n.Id))).ToList();
        }

        public async Task<DuelLoadoutDTO> GetLoadout(string playerId)
        {
            var player = await _players.GetPlayerDataByPlayerId(playerId);
            var equipped = (await _gear.GetEquipedItems()).Values.Where(i => i != null).ToList();
            var loadout = DuelMapper.ToLoadout(PlayerDuelerBuilder.Build(player.PlayerId ?? "You", player, equipped), true);
            loadout.Dagger = equipped.FirstOrDefault(i => i.Type == ItemType.DuelWeapon);
            return loadout;
        }

        public async Task<DuelLoadoutDTO> SetLoadout(string playerId, List<int> spells)
        {
            var clean = (spells ?? new List<int>()).Where(DuelSpellBook.IsCastable).Distinct().Take(DuelSpellBook.MaxSpells).ToList();
            var player = await _players.GetPlayerDataByPlayerId(playerId);
            var update = new PlayerData { Id = playerId, DuelSpells = clean };
            update.ExplicitValuesUpsert(new[] { nameof(PlayerData.DuelSpells) });
            await _players.UpsertPlayerData(update, player);
            return await GetLoadout(playerId);
        }

        public async Task<DuelLoadoutDTO> SetStance(string playerId, int stance)
        {
            var player = await _players.GetPlayerDataByPlayerId(playerId);
            var update = new PlayerData { Id = playerId, DuelStance = DuelStance.Clean(stance) };
            update.ExplicitValuesUpsert(new[] { nameof(PlayerData.DuelStance) });
            await _players.UpsertPlayerData(update, player);
            return await GetLoadout(playerId);
        }

        public async Task<DuelResultDTO> FightNpc(string playerId, string npcId)
        {
            var npc = await _duelers.GetById(npcId);

            if (npc == null || !npc.IsActive)
            {
                return new DuelResultDTO { Success = false, AdditionalInfo = "npc_not_found" };
            }

            var player = await _players.GetPlayerDataByPlayerId(playerId);
            var me = PlayerDuelerBuilder.Build(player.PlayerId ?? "You", player, (await _gear.GetEquipedItems()).Values);
            var replay = ArenaSimulator.Run(me, npc, Random.Shared.Next());
            var result = new DuelResultDTO { Success = true, Won = replay.Winner == 0, Replay = replay };

            if (result.Won && !(player.DefeatedDuelers ?? new List<string>()).Contains(npc.Id))
            {
                await GrantFirstWin(player, npc, result);
            }

            return result;
        }

        private async Task GrantFirstWin(PlayerData player, Dueler npc, DuelResultDTO result)
        {
            var defeated = new List<string>(player.DefeatedDuelers ?? new List<string>()) { npc.Id };
            await _players.UpsertPlayerData(new PlayerData { Id = player.Id, DefeatedDuelers = defeated }, player);

            if (npc.RewardGold > 0)
            {
                await _resources.AddGold(player.Id, npc.RewardGold);
            }

            if (npc.RewardGems > 0)
            {
                await _resources.AddBlueGems(player.Id, npc.RewardGems);
            }

            result.FirstWin = true;
            result.RewardGold = npc.RewardGold;
            result.RewardGems = npc.RewardGems;
        }
    }
}
