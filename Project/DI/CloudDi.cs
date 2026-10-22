using CloudLogic.Cache;
using CloudLogic.Repositories;
using CloudLogic.Services;
using CloudLogic.Services.Achievements;
using CloudLogic.Services.Duel;
using CloudLogic.Services.ItemTemplate;
using CloudLogic.Services.Items;
using CloudLogic.Services.Player;
using CloudLogic.Services.Reward;
using CloudLogic.Services.Shop;
using CloudLogic.Services.Trading;
using CloudLogic.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;
using Unity.Services.CloudCode.Core;
using CloudLogic.Repositories;
using CloudLogic.Services.Gem;
using CloudLogic.Services.IAP;
using CloudLogic.Util;

namespace CloudLogic.DI
{
    /// <summary>
    /// Minimal .NET DI setup for Unity Cloud Code:
    /// - Root container built once per process
    /// - Per-function-call scope for correct lifetimes and logging
    /// </summary>
    public static class CloudDi
    {
        private static readonly Lazy<IServiceProvider> _root =
            new(BuildRootProvider, LazyThreadSafetyMode.ExecutionAndPublication);

        public static IServiceScope CreateScope(ILogger logger, IExecutionContext? executionContext = null)
        {
            var scope = _root.Value.CreateScope();

            // Set ambient logger and execution context for this async flow before anything resolves them.
            var ctx = scope.ServiceProvider.GetRequiredService<RequestContextAccessor>();
            ctx.PushLogger(logger ?? NullLogger.Instance);
            ctx.PushContext(executionContext);

            return scope;
        }

        public static async Task<T> WithScope<T>(ILogger logger, IExecutionContext ctx, Func<IServiceProvider, Task<T>> work)
        {
            if (work == null) throw new ArgumentNullException(nameof(work));

            using var scope = CreateScope(logger, ctx);
            try
            {
                return await work(scope.ServiceProvider);
            }
            catch (Exception ex)
            {
                scope.ServiceProvider.GetRequiredService<EventsLogger>().LogWithEx("UnhandledException", ex);
                throw;
            }
            finally
            {
                // Ensure we clear ambient context even if work throws.
                var accessor = scope.ServiceProvider.GetRequiredService<RequestContextAccessor>();
                accessor.PopLogger();
            }
        }

        public static async Task WithScope(ILogger logger, IExecutionContext ctx, Func<IServiceProvider, Task> work)
        {
            if (work == null) throw new ArgumentNullException(nameof(work));

            using var scope = CreateScope(logger, ctx);
            try
            {
                await work(scope.ServiceProvider);
            }
            catch (Exception ex)
            {
                scope.ServiceProvider.GetRequiredService<EventsLogger>().LogWithEx("UnhandledException", ex);
                throw;
            }
            finally
            {
                var accessor = scope.ServiceProvider.GetRequiredService<RequestContextAccessor>();
                accessor.PopLogger();
            }
        }

        private static IServiceProvider BuildRootProvider()
        {
            var services = new ServiceCollection();

            // Ambient request context (logger etc.)
            services.AddSingleton<RequestContextAccessor>();
            services.AddScoped<ILogger>(sp => sp.GetRequiredService<RequestContextAccessor>().Logger);
            services.AddScoped<EventsLogger>();

            // Cache (singleton to reuse Redis connection)
            services.AddSingleton<ICloudCache>(sp =>
                new RedisCloudCache(RedisConfig.GetConnectionString(), sp.GetRequiredService<RequestContextAccessor>()));

            // Repositories (scoped to avoid sharing per-call logger/state)
            services.AddScoped<ItemTemplateRepo>();
            services.AddScoped<WaveDetailsRepo>();
            services.AddScoped<VersionSettingRepo>();
            services.AddScoped<ItemsRepo>();
            services.AddScoped<MailRepo>();
            services.AddScoped<AuctionHouseRepo>();
            services.AddScoped<PlayerDataRepo>();
            services.AddScoped<PlayerInfoRepo>();
            services.AddScoped<ItemDropRepo>();
            services.AddScoped<ClaimedRewardRepo>();
            services.AddScoped<DailyLoginWeekRepo>();
            services.AddScoped<QuestRepo>();
            services.AddScoped<AuditRepo>();
            services.AddScoped<ClientDiagnosticsRepo>();
            services.AddScoped<RunAnalysisRepo>();
            services.AddScoped<PlayerGemsRepo>();
            services.AddScoped<GemConfigsRepo>();
            services.AddScoped<AchievementRepo>();
            services.AddScoped<AchievementProgressRepo>();
            services.AddScoped<QuestProgressRepo>();
            services.AddScoped<QuestsManagementRepo>();
            services.AddScoped<ShopRepo>();
            services.AddScoped<IapProductRepo>();
            services.AddScoped<DailyDealsRepo>();
            services.AddScoped<DailyDealsService>();
            services.AddScoped<BugReportRepo>();
            services.AddScoped<FeedbackRepo>();
            services.AddScoped<PurchaseRecordRepo>();
            services.AddScoped<GemBoughtProductRepo>();
            services.AddScoped<DailyMissionsSelectedRepo>();
            services.AddScoped<PlayerSkinsRepo>();
            services.AddScoped<SkinConfigsRepo>();
            services.AddScoped<SkinsRepo>();

            // Services
            services.AddScoped<IBugService, BugService>();
            services.AddScoped<QuestService>();
            services.AddScoped<AchievementsService>();
            services.AddScoped<PlayerQuestsService>();
            services.AddScoped<ShopService>();
            services.AddScoped<IDailyLoginWeekService, DailyLoginWeekService>();
            services.AddScoped<IItemTemplateService, ItemTemplateService>();
            services.AddScoped<IItemGenerationService, ItemGenerationService>();
            services.AddScoped<IItemCreateService, ItemCreateService>();
            services.AddScoped<ItemDropService>();
            services.AddScoped<ItemsService>();
            services.AddScoped<GearScoreService>();
            services.AddScoped<IGearService, GearService>();

            services.AddScoped<IFCMService>(_ => new FCMService(new PushNotifier()));
            services.AddScoped<IMailService, MailService>();
            services.AddScoped<IAuctionHouseService, AuctionHouseService>();
            services.AddScoped<IPlayerGemService, PlayerGemService>();
            services.AddScoped<IPlayerManagerService, PlayerManagerService>();
            services.AddScoped<IEnergyService, EnergyService>();
            services.AddScoped<IRunService, RunService>();
            services.AddScoped<CloudLogic.Services.Player.RunManagement.RunCompleter>();
            services.AddScoped<IRewardService, RewardService>();
            services.AddScoped<DailyRewardService>();

            services.AddScoped<BlacksmithService>();
            services.AddScoped<ArmoryService>();
            services.AddScoped<AlterItemService>();
            services.AddScoped<ReforgeService>();
            services.AddScoped<SellService>();
            services.AddScoped<OwnedItemsService>();
            services.AddScoped<ResourceService>();
            services.AddScoped<ReviveService>();
            services.AddScoped<ResourceDropService>();
            services.AddScoped<AdService>();
            services.AddScoped<TalentsService>();
            services.AddScoped<IPurchaseRecordService, PurchaseRecordService>();
            services.AddScoped<IRefundService, RefundService>();
            services.AddScoped<IPlayerSkinService, PlayerSkinService>();

            services.AddScoped<DuelerRepo>();
            services.AddScoped<DuelService>();

            return services.BuildServiceProvider(new ServiceProviderOptions
            {
                ValidateOnBuild = false,
                ValidateScopes = false
            });
        }
    }
}

