using CloudLogic.DI;
using CloudLogic.RepoModels;
using CloudLogic.RepoModels.Achievements;
using CloudLogic.RepoModels.Items;
using CloudLogic.Repositories;
using CloudLogic.Response.Base;
using CloudLogic.Services;
using CloudLogic.Services.Achievements;
using CloudLogic.Services.Interfaces;
using CloudLogic.Services.Items;
using CloudLogic.Services.Player;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.CloudCode.Core;

namespace CloudLogic.Modules
{
    public class GearModule
    {
        private readonly ILogger _logger;

        public GearModule(ILogger<GearModule> logger)
        {
            _logger = logger;
        }

        [CloudCodeFunction(nameof(SetNewGearscore))]
        public async Task<GearscoreResponse> SetNewGearscore(IExecutionContext ctx, GearScoreEntry entry)
        {
            return await CloudDi.WithScope(_logger, ctx, async sp =>
            {
                var gearScoreService = sp.GetRequiredService<GearScoreService>();
                var playerService = sp.GetRequiredService<IPlayerManagerService>();
                var achievementsService = sp.GetRequiredService<AchievementsService>();

                entry.PlayerId = ctx.PlayerId;
                entry.Player = (await playerService.GetPlayerDataAsync(ctx.PlayerId)).PlayerId;

                if (entry.Player.ToLower().Contains("devb") 
                || entry.Player.ToLower().Contains("devd")
                || entry.Player.ToLower().Contains("videos"))
                {
                    return new GearscoreResponse()
                    {
                        Success = true,
                        Gearscore = 0,
                        AchievementProgressInfo = null
                    };
                }

                var score = await gearScoreService.SubmitGearScore(entry);
                var achievementProgress = await achievementsService.SetIfMore(
                    ctx.PlayerId,
                    AchievementType.GearScore.ToString(),
                    score);

                return new GearscoreResponse()
                {
                    Success = true,
                    Gearscore = score,
                    AchievementProgressInfo = achievementProgress
                };
            });
        }


        [CloudCodeFunction(nameof(GetGearscore))]
        public async Task<List<GearScoreEntry>> GetGearscore(IExecutionContext ctx)
        {
            return await CloudDi.WithScope(_logger, ctx, sp =>
                sp.GetRequiredService<GearScoreService>().GetGearScores());
        }

        [CloudCodeFunction(nameof(EquipItem))]
        public async Task<string> EquipItem(IExecutionContext ctx, string itemId)
        {
            return await CloudDi.WithScope(_logger, ctx, async sp =>
            {
                var itemsRepo = sp.GetRequiredService<ItemsRepo>();
                var gearService = sp.GetRequiredService<IGearService>();

                var item = await itemsRepo.GetPlayerItem(ctx.PlayerId, itemId);
                await gearService.EquipItem(item);
                return "OK";
            });
        }


        [CloudCodeFunction(nameof(EquipItems))]
        public async Task<string> EquipItems(IExecutionContext ctx, List<string> itemIds)
        {
            return await CloudDi.WithScope(_logger, ctx, async sp =>
            {
                var itemsRepo = sp.GetRequiredService<ItemsRepo>();
                var gearService = sp.GetRequiredService<IGearService>();

                var item = await itemsRepo.GetMyOwnItems(ctx.PlayerId, itemIds);
                await gearService.EquipItems(item);
                return "OK";
            });
        }

        [CloudCodeFunction(nameof(UnequipItem))]
        public async Task<string> UnequipItem(IExecutionContext ctx, ItemType itemType)
        {
            return await CloudDi.WithScope(_logger, ctx, async sp =>
            {
                await sp.GetRequiredService<IGearService>().UnequipItem(itemType);
                return "OK";
            });
        }

        [CloudCodeFunction(nameof(UnequipItems))]
        public async Task<string> UnequipItems(IExecutionContext ctx, List<ItemType> itemTypes)
        {
            return await CloudDi.WithScope(_logger, ctx, async sp =>
            {
                await sp.GetRequiredService<IGearService>().UnequipItems(itemTypes);
                return "OK";
            });
        }

        [CloudCodeFunction(nameof(HandleEquipmentChange))]
        public async Task<string> HandleEquipmentChange(
            IExecutionContext ctx,
            List<ItemType> unequips,
            List<string> equips,
            Dictionary<string, string> skinEquips = null)
        {
            return await CloudDi.WithScope(_logger, ctx, async sp =>
            {
                var itemsRepo = sp.GetRequiredService<ItemsRepo>();
                var items = await itemsRepo.GetMyOwnItems(ctx.PlayerId, equips);

                await sp.GetRequiredService<IGearService>().HandleEquipmentChange(unequips, items);

                if (skinEquips != null && skinEquips.Count > 0)
                {
                    await sp.GetRequiredService<IPlayerSkinService>().EquipSkins(ctx.PlayerId, skinEquips);
                }

                return "OK";
            });
        }

        [CloudCodeFunction(nameof(GetFirstWeapon))]
        public async Task<Item> GetFirstWeapon(IExecutionContext ctx, int choice)
        {
            return await CloudDi.WithScope(_logger, ctx, async sp =>
            {
                var itemsService = sp.GetRequiredService<ItemsService>();
                var gearService = sp.GetRequiredService<IGearService>();

                var item = await itemsService.GetFirstWeapon(ctx.PlayerId, choice);
                await gearService.SetInitialGiven(choice);

                return item;
            });
        }
    }
}
