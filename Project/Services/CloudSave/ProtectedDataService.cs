using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text;
using Microsoft.Extensions.Logging;
using System;
using Unity.Services.CloudCode.Apis;
using System.Collections.Generic;
using Unity.Services.CloudCode.Core;
using Unity.Services.CloudSave.Model;

namespace CloudLogic.Services.CloudSave
{
    public class ProtectedDataService
    {
        private readonly IGameApiClient _gameApiClient;
        protected readonly ILogger _logger;
        protected readonly string _dataKey;

        public ProtectedDataService(ILogger logger, string dataKey)
        {
            _logger = logger;
            _gameApiClient = GameApiClient.Create();
            _dataKey = dataKey;
        }

        public async Task<T> LoadData<T>(IExecutionContext ctx)
        {
            var resultData = (await _gameApiClient.CloudSaveData.GetProtectedItemsAsync(ctx,
                ctx.ServiceToken,
                ctx.ProjectId,
                ctx.PlayerId,
                new List<string>() { _dataKey })).Data;


            if (resultData.Results != null && resultData.Results.Count > 0)
            {
                var item = resultData.Results[0];
                return (T)item.Value;
            }
            else
            {
                var dataValue = default(T);

                await SetData(ctx, ctx.PlayerId, dataValue);
                return dataValue;
            }
        }

        public async Task<bool> SetData<T>(IExecutionContext ctx, string playerId, T dataValue)
        {

            var item = await LoadDataItem(ctx);

            if (item != null)
            {
                await _gameApiClient.CloudSaveData.SetProtectedItemAsync(ctx, ctx.ServiceToken, ctx.ProjectId, playerId,
                    new SetItemBody(_dataKey, dataValue, item.WriteLock));
            }
            else
            {
                await _gameApiClient.CloudSaveData.SetProtectedItemAsync(ctx, ctx.ServiceToken, ctx.ProjectId, playerId,
                    new SetItemBody(_dataKey, dataValue));
            }



            return true;
        }

        private async Task<Item> LoadDataItem(IExecutionContext ctx)
        {
            var resultData = (await _gameApiClient.CloudSaveData.GetProtectedItemsAsync(ctx,
                ctx.ServiceToken,
                ctx.ProjectId,
                ctx.PlayerId,
                new List<string>() { _dataKey })).Data;

            if (resultData.Results != null && resultData.Results.Count > 0)
            {
                var item = resultData.Results[0];
                return item;
            }

            return null;
        }

    }
}