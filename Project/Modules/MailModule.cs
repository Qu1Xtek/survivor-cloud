using CloudLogic.DI;
using CloudLogic.RepoModels.Items;
using CloudLogic.Repositories;
using CloudLogic.Services;
using CloudLogic.Services.Interfaces;
using CloudLogic.Services.Items;
using Google.Protobuf.WellKnownTypes;
using Google.Type;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;
using Unity.Services.CloudCode.Shared;
using Unity.Services.CloudSave.Model;

namespace CloudLogic.Modules
{
    public class MailModule
    {
        private readonly ILogger _logger;
        private readonly IGameApiClient _gameApiClient;

        public MailModule(ILogger<MailModule> logger)
        {
            _logger = logger;
            _gameApiClient = GameApiClient.Create();
        }

        private async Task<ApiResponse<SetItemResponse>> SetProtectedItemsResponse(IExecutionContext ctx, SetItemBody data)
        {
            return await _gameApiClient.CloudSaveData.SetProtectedItemAsync(ctx,
                ctx.ServiceToken,
                ctx.ProjectId,
                ctx.PlayerId,
                data);
        }

        [CloudCodeFunction(nameof(GetPlayerMail))]
        public async Task<List<Mail>> GetPlayerMail(IExecutionContext ctx)
        {
            return await CloudDi.WithScope(_logger, ctx, async sp =>
            {
                var playerId = ctx.PlayerId;
                var res = await sp.GetRequiredService<IMailService>().GetPlayerMail(playerId);
                return res ?? new List<Mail>();
            });
        }

        [CloudCodeFunction(nameof(ClaimMail))]
        public async Task ClaimMail(IExecutionContext ctx, string mailId)
        {
            await CloudDi.WithScope(_logger, ctx, sp =>
                sp.GetRequiredService<IMailService>().ClaimMail(ctx.PlayerId, mailId));
        }

        [CloudCodeFunction(nameof(ReportBug))]
        public async Task ReportBug(IExecutionContext ctx, string requiredText, string optionaltext)
        {
            await CloudDi.WithScope(_logger, ctx, sp =>
                sp.GetRequiredService<IBugService>()
                .ReportBug(ctx.PlayerId, requiredText, optionaltext));
        }


        [CloudCodeFunction(nameof(SendFeedback))]
        public async Task SendFeedback(IExecutionContext ctx, string requiredText)
        {
            await CloudDi.WithScope(_logger, ctx, sp =>
                sp.GetRequiredService<FeedbackRepo>().CreateFeedback(new RepoModels.Feedback
                {
                    DateCreated = Google.Cloud.Firestore.Timestamp.FromDateTime(System.DateTime.UtcNow),
                    FeedbackText = requiredText,
                    OptionalInfo = ctx.EnvironmentName,
                    Type = 0,
                    PlayerId = ctx.PlayerId
                }));

            var item = new Unity.Services.CloudSave.Model.SetItemBody("FeedbackGiven", "true");
            await SetProtectedItemsResponse(ctx, item);
        }

    }
}
