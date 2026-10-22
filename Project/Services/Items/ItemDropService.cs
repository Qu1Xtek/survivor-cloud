using CloudLogic.RepoModels.Items;
using CloudLogic.Repositories;
using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.CloudSave.Model;
using ItemTemplateModel = CloudLogic.RepoModels.Items.ItemTemplate;

namespace CloudLogic.Services.Items
{
    public class ItemDropService
    {        
        private ItemDropRepo _repo;
        private IItemTemplateService _templateService;
        private IPlayerManagerService _playerManagerService;

        public ItemDropService(ItemDropRepo repo, IItemTemplateService templateService, IPlayerManagerService playerManagerService)
        {
            _repo = repo;
            _templateService = templateService;
            _playerManagerService = playerManagerService;          
        }

        public async Task PrepareNewRunDropBag(string playerId)
        {
            await _repo.UpsertRunBagAsync(new ItemDrop
            {
                Id = playerId,
                RunDropBag = new Dictionary<string, DroppedItem>(),
                ExplicitUpserts = new List<string>() { nameof(ItemDrop.RunDropBag) }
            });
        }

        public async Task<DroppedItem> DropItem(string playerId, bool isBoss = false)
        {
            DroppedItem dropResult = null;
            await _repo.RunOurTransactionAsync(async tran =>
            {
                var playerData = await _playerManagerService.GetPlayerDataAsync(playerId, null, tran);
                var selectedLevel = playerData.SelectedLevel;

                if (!isBoss && playerData.RunData.ItemsDropped >= playerData.RunData.MaxItemDrops)
                {
                    return;
                }

                RarityType rarity = GetRarity();

                if (isBoss && rarity < RarityType.Rare)
                {
                    rarity = RarityType.Rare;
                }

                if (rarity == RarityType.Legendary)
                {
                    int index = new Random().Next(0, 1000);

                    if (index >= 900)
                    {
                        dropResult = await GetRerollRuneDrop(playerId, tran);
                        return; // Break code exeuction , rune is dropped!
                    }
                }

                if (selectedLevel < 26)
                {
                    selectedLevel = selectedLevel == 0 // Avoid first level dropping 0 level items
                        ? -1
                        : selectedLevel + 1;

                    selectedLevel = selectedLevel <= 10
                        ? (int)Math.Ceiling(selectedLevel / 2f + 1)
                        : selectedLevel - 5;
                }
                else
                {
                    selectedLevel = selectedLevel + 1;
                }


                List<ItemTemplateModel> itemPool = new List<ItemTemplateModel>();

                // From stage 35 up, freeze the drop floor at level 31 so the window only widens
                // upward (stage 39 -> levels 31-40) - keeps top-tier items rare instead of flooding.
                int windowBottom = playerData.SelectedLevel < 35
                    ? selectedLevel - 5
                    : 31;

                var fullTemplatePool = await _templateService.GetItemTemplatesForGeneration(rarity, windowBottom, selectedLevel, false);

                var maxRarityPossible = fullTemplatePool.Max(x => x.Rarity);

                rarity = rarity > maxRarityPossible ? maxRarityPossible : rarity;

                var itemTMPL = await _templateService.GetItemTemplate(rarity, windowBottom, selectedLevel, false);

                await _playerManagerService.IncreaseItemDropCountForData(playerData, tran);

                var droppedItem = new DroppedItem
                {
                    Rarity = rarity,
                    DroppedAt = Timestamp.FromDateTime(DateTime.UtcNow),
                    ItemTemplateId = itemTMPL.Id,
                    DropType = DropType.Item,
                    PlayerId = playerId
                };

                await _repo.AddItemDrop(playerId, droppedItem, tran);

                // Return the DroppedItem
                dropResult = droppedItem;
            });

            return dropResult;
        }

        private async Task<DroppedItem> GetRerollRuneDrop(string playerId = "", Transaction tran = null)
        {
            var droppedRune = new DroppedItem
            {
                Rarity = RarityType.Legendary,
                DroppedAt = Timestamp.FromDateTime(DateTime.UtcNow),
                DropType = DropType.RerollRune,
                PlayerId = playerId
            };

            await _repo.AddItemDrop(playerId, droppedRune, tran);

            return droppedRune;
        }

        public async Task<ItemDrop> GetRunBagAsync(string playerId, string dropId)
        {
            return await _repo.GetItemDropsByPlayerId(playerId);
        }

        public async Task<DroppedItem> GetItemDropByIdAsync(string playerId, string dropId)
        {
            var itemDrops = await _repo.GetItemDropsByPlayerId(playerId);
            
            if (itemDrops.RunDropBag.ContainsKey(dropId))
            {
                return itemDrops.RunDropBag[dropId];
            }

            return null;
        }

        public async Task<List<DroppedItem>> GetItemDropsByIdAsync(string playerId, string[] dropId)
        {
            var itemDrops = await _repo.GetItemDropsByPlayerId(playerId);

            //return itemDrops.RunDropBag.Intersect(d => d.Id == dropId);

            return null;
        }

        public async Task DeleteItemDropById(string playerId, string id)
        {
            var itemDrops = await GetRunBagAsync(playerId, id);

            itemDrops.RunDropBag.Remove(id);
            itemDrops.ExplicitUpserts = new List<string> { nameof(ItemDrop.RunDropBag)};

            await _repo.UpsertRunBagAsync(itemDrops);
        }

        public static RarityType GetRarity()
        {
            int index = new Random().Next(0, 1000);
            if (index >= 980)
            {
                return RarityType.Legendary;
            }
            else if (index >= 950)
            {
                return RarityType.Epic;
            }
            else if (index >= 800)
            {
                return RarityType.Rare;
            }
            else if (index >= 650)
            {
                return RarityType.Uncommon;
            }
            else
            {
                return RarityType.Common;
            }
        }
    }
}
