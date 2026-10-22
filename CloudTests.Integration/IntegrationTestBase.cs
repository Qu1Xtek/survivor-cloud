using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using CloudLogic.Modules;
using Microsoft.Extensions.DependencyInjection;
using CloudLogic.DI;
using CloudLogic.Repositories;
using CloudLogic.Services.Player;
using CloudLogic.Services.Items;
using CloudLogic.Services.ItemTemplate;
using CloudLogic.Services;
using CloudLogic.Services.Trading;
using CloudLogic.Services.Reward;
using CloudLogic.Services.Shop;
using CloudLogic.Cache;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CloudLogic.Services.Interfaces;
using CloudLogic.Services.Gem;
using CloudLogic.Services.Achievements;

namespace CloudTests.Integration
{
    /// <summary>
    /// Base class for integration tests that sets up real services and repositories.
    /// Uses Firestore emulator or test database.
    /// </summary>
    public abstract class IntegrationTestBase
    {
        public static string GlobalTestPlayerId = "c6Z2SkyzwGEddzXkqbNUjJKAKOGn";

        protected ILogger Logger { get; }
        protected RequestContextAccessor ContextAccessor { get; }
        protected IServiceProvider ServiceProvider { get; }

        protected IntegrationTestBase()
        {
            Logger = NullLogger.Instance;
            ContextAccessor = new RequestContextAccessor();
            ContextAccessor.PushLogger(Logger);

            // Build service provider with real dependencies
            var services = new ServiceCollection();
            
            // Register real repositories
            services.AddSingleton<RequestContextAccessor>(ContextAccessor);
            services.AddScoped<ILogger>(sp => sp.GetRequiredService<RequestContextAccessor>().Logger);
            // Use in-memory cache for tests (avoid Redis dependency/stale config).
            services.AddSingleton<ICloudCache, TestCloudCache>();
            
            // Register repositories
            services.AddScoped<PlayerDataRepo>(sp => new PlayerDataRepo(Logger));
            services.AddScoped<ItemDropRepo>(sp => new ItemDropRepo(Logger));
            services.AddScoped<ItemTemplateRepo>(sp => new ItemTemplateRepo(Logger));
            services.AddScoped<ItemsRepo>(sp => new ItemsRepo(Logger));
            services.AddScoped<AuctionHouseRepo>(sp => new AuctionHouseRepo(Logger, sp.GetRequiredService<ICloudCache>()));
            services.AddScoped<MailRepo>(sp => new MailRepo(Logger));
            services.AddScoped<QuestRepo>(sp => new QuestRepo(Logger));
            services.AddScoped<DailyLoginWeekRepo>(sp => new DailyLoginWeekRepo(Logger));
            services.AddScoped<ClaimedRewardRepo>(sp => new ClaimedRewardRepo(Logger));
            services.AddScoped<ShopRepo>(sp => new ShopRepo(Logger));
            services.AddScoped<PlayerGemsRepo>(sp => new PlayerGemsRepo(Logger));
            services.AddScoped<SkinsRepo>(sp => new SkinsRepo(Logger));
            services.AddScoped<SkinConfigsRepo>(sp => new SkinConfigsRepo(Logger, sp.GetRequiredService<ICloudCache>()));
            services.AddScoped<PlayerSkinsRepo>(sp => new PlayerSkinsRepo(Logger));
            services.AddScoped<GemBoughtProductRepo>(sp => new GemBoughtProductRepo(Logger));
            services.AddScoped<AchievementRepo>(sp => new AchievementRepo(Logger));
            services.AddScoped<AchievementProgressRepo>(sp => new AchievementProgressRepo(Logger));
            services.AddScoped<QuestProgressRepo>(sp => new QuestProgressRepo(Logger));
            
            // Register services
            services.AddScoped<IFCMService>(sp => new FCMService(new PushNotifier()));
            services.AddScoped<IMailService, MailService>();
            services.AddScoped<IAuctionHouseService, AuctionHouseService>();
            services.AddScoped<IEnergyService, EnergyService>();
            services.AddScoped<IRunService, RunService>();
            services.AddScoped<IPlayerManagerService, PlayerManagerService>();
            services.AddScoped<ItemDropService>();
            services.AddScoped<IItemTemplateService, ItemTemplateService>();
            services.AddScoped<IItemGenerationService, ItemGenerationService>();
            services.AddScoped<IItemCreateService, ItemCreateService>();
            services.AddScoped<ItemsService>();
            services.AddScoped<QuestService>();
            services.AddScoped<IDailyLoginWeekService, DailyLoginWeekService>();
            services.AddScoped<IRewardService, RewardService>();
            services.AddScoped<IPlayerSkinService, PlayerSkinService>();
            services.AddScoped<ShopService>();
            services.AddScoped<DailyRewardService>();
            services.AddScoped<ArmoryService>();
            services.AddScoped<SellService>();
            services.AddScoped<DailyLoginWeekService>();
            services.AddScoped<ResourceService>();
            services.AddScoped<TalentsService>();
            services.AddScoped<ResourceDropService>();
            services.AddScoped<AuditRepo>();
            services.AddScoped<IPlayerGemService, PlayerGemService>();
            services.AddScoped<AchievementsService>();
            services.AddSingleton<ILogger<RewarModule>>(NullLogger<RewarModule>.Instance);
            services.AddScoped<RewarModule>();
            //services.AddScoped<GoldService>();            

            ServiceProvider = services.BuildServiceProvider();
        }

        [TestCleanup]
        public virtual void Cleanup()
        {
            // Clean up test data if needed
            ContextAccessor?.PopLogger();
        }
    }
}
