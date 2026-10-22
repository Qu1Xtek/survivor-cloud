using CloudLogic.DI;
using CloudLogic.RepoModels.Items;
using CloudLogic.Repositories;
using CloudLogic.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;
using Unity.Services.CloudCode.Shared;
using Unity.Services.CloudSave.Model;

namespace CloudLogic.Services.Items
{
    public class GearService : IGearService
    {
        private readonly IGameApiClient _gameApiClient;
        private readonly ItemsService _itemsService;
        private readonly RequestContextAccessor _contextAccessor;

        public GearService(ItemsService itemsService, RequestContextAccessor contextAccessor)
        {
            _gameApiClient = GameApiClient.Create();
            _itemsService = itemsService;
            _contextAccessor = contextAccessor;
        }

        private List<string> EquipmentType = new List<string>
        {
            {ItemType.Head.ToString()},
            {ItemType.Bracer.ToString()},
            {ItemType.Belt.ToString()},
            { ItemType.Boots.ToString() },
            { ItemType.Hands.ToString() },
            { ItemType.Ring.ToString() },
            { ItemType.Amulet.ToString() },
            { ItemType.Chest.ToString() },
            { ItemType.Legs.ToString() },
            { ItemType.Weapon.ToString() },
        };

        private Dictionary<ItemType, RepoModels.Items.Item> GetEquipment()
        {
            return new Dictionary<ItemType, RepoModels.Items.Item>
            {
                {ItemType.Head, null },
                {ItemType.Bracer, null },
                {ItemType.Belt, null },
                {ItemType.Boots, null },
                {ItemType.Hands, null },
                {ItemType.Ring, null },
                {ItemType.Amulet, null },
                {ItemType.Chest, null },
                {ItemType.Legs, null },
                {ItemType.Weapon, null },
            };
        }

        public async Task EquipItem(RepoModels.Items.Item item)
        {
            var executionContext = _contextAccessor.ExecutionContext;
            if (executionContext == null)
                throw new InvalidOperationException("ExecutionContext is not available");

            var data = ItemBody(item.Type.ToString(), item.Id);

            if (item != null && item.OwnerId == executionContext.PlayerId)
            {
                await SetProtectedItemsResponse(executionContext, data);
            }
        }

        public async Task EquipItems(List<RepoModels.Items.Item> items)
        {
            foreach (var item in items)
            {
                await EquipItem(item);
            }
        }

        public async Task UnequipItem(ItemType slotId)
        {
            var executionContext = _contextAccessor.ExecutionContext;
            if (executionContext == null)
                throw new InvalidOperationException("ExecutionContext is not available");

            var data = ItemBody(slotId.ToString(), "");

            await SetProtectedItemsResponse(executionContext, data);
        }
        
        public async Task UnequipItems(List<ItemType> slotIds)
        {
            foreach (var slotId in slotIds)
            {
                await UnequipItem(slotId);
            }
        }

        public async Task HandleEquipmentChange(List<ItemType> unequips, List<RepoModels.Items.Item> equips)
        {
            await UnequipItems(unequips);
            await EquipItems(equips);
        }

        public async Task<Dictionary<ItemType, RepoModels.Items.Item>> GetEquipedItems()
        {
            var executionContext = _contextAccessor.ExecutionContext;
            if (executionContext == null)
                throw new InvalidOperationException("ExecutionContext is not available");

            var result = await GetProtectedItemsResponse(executionContext);

            var data = result.Data;
            var equipment = GetEquipment();

            // Collect equipped item ids and batch-fetch them in a single query (avoids N+1 round trips).
            var equippedIds = data.Results
                .Where(s => s.Value != null && !string.IsNullOrEmpty((string)s.Value))
                .Select(s => (string)s.Value)
                .ToList();

            if (equippedIds.Count == 0)
            {
                return equipment;
            }

            var items = await _itemsService.GetItemsByIds(equippedIds);
            var itemsById = items.Where(i => i != null).ToDictionary(i => i.Id);

            foreach (var equipSlot in data.Results)
            {
                if (equipSlot.Value != null && !string.IsNullOrEmpty((string)equipSlot.Value)
                    && itemsById.TryGetValue((string)equipSlot.Value, out var item))
                {
                    equipment[Enum.Parse<ItemType>(equipSlot.Key)] = item;
                }
            }

            return equipment;

        }

        public async Task<List<string>> GetEquippedItemsIds()
        {
            var executionContext = _contextAccessor.ExecutionContext;
            if (executionContext == null)
                throw new InvalidOperationException("ExecutionContext is not available");

            var result = await GetProtectedItemsResponse(executionContext);

            var data = result.Data;
            var ids = new List<string>();

            foreach (var equipSlot in data.Results)
            {
                if (equipSlot.Value != null && !string.IsNullOrEmpty((string)equipSlot.Value))
                {
                    ids.Add((string)equipSlot.Value);
                }
            }

            return ids;
        }

        public async Task<bool> SetInitialGiven(int choice)
        {
            var executionContext = _contextAccessor.ExecutionContext;
            if (executionContext == null)
                throw new InvalidOperationException("ExecutionContext is not available");

            var protectedItems = await GetProtectedItemsResponse(executionContext);

            if (!protectedItems.Data.Results.Any(r => r.Key == "InitialGiven"))
            {
                var data = ItemBody("InitialGiven", "true");

                SetProtectedItemsResponse(executionContext, data);
            }

            return true;
        }

        private Unity.Services.CloudSave.Model.SetItemBody ItemBody(string key, object data)
        {
            return new Unity.Services.CloudSave.Model.SetItemBody(key, data);
        }

        private async Task<ApiResponse<GetItemsResponse>> GetProtectedItemsResponse(IExecutionContext ctx) 
        {
            return await _gameApiClient.CloudSaveData.GetProtectedItemsAsync(ctx,
                ctx.AccessToken,
                ctx.ProjectId,
                ctx.PlayerId, 
                EquipmentType);
        }

        private async Task<ApiResponse<SetItemResponse>> SetProtectedItemsResponse(IExecutionContext ctx, SetItemBody data)
        {
            return await _gameApiClient.CloudSaveData.SetProtectedItemAsync(ctx,
                ctx.ServiceToken,
                ctx.ProjectId,
                ctx.PlayerId,
                data);
        }
    }
}
