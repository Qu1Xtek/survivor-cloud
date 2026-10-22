using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System;
using CloudLogic.Services.Items;
using CloudLogic.Repositories;
using CloudLogic.Cache;
using CloudLogic.Services;
using CloudLogic.Services.Trading;
using CloudLogic.Services.Reward;
using CloudLogic.Services.ItemTemplate;
using CloudLogic.Services.Player;
using CloudLogic.DI;
using CloudLogic.Services.Interfaces;

public static class ServiceProvider
{
    private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();
    private static bool _isInitialized = false;

    public static void Initialize(ILogger logger)
    {
        //if (_isInitialized) return;

        //_isInitialized = true;

        ////RegisterSingleton(new GoldService(logger));
        //RegSGT(new ItemTemplateRepo(logger));
        //RegSGT(new WaveDetailsRepo(logger));
        //RegSGT(new VersionSettingRepo(logger));
        //// NOTE: Legacy service-locator initialization (kept only for backward compatibility / compilation).
        //// Prefer CloudLogic.DI.CloudDi for per-request scoped resolution.
        //var legacyCtx = new RequestContextAccessor();
        //legacyCtx.PushLogger(logger);

        //RegSGT<ICloudCache>(new RedisCloudCache(RedisConfig.GetConnectionString(), legacyCtx));
        //RegSGT<IItemTemplateService>(new ItemTemplateService(GS<ICloudCache>(), GS<ItemTemplateRepo>(), logger));
        //RegSGT<IItemGenerationService>(new ItemGenerationService(GS<IItemTemplateService>(),logger));
        //RegSGT(new ItemsRepo(logger));
        //RegSGT(new GearScoreService(logger, GS<ItemsRepo>(), GS<ICloudCache>()));
        //RegSGT(new MailRepo(logger));
        //RegSGT(new AuctionHouseRepo(logger, GS<ICloudCache>()));
        //RegSGT(new PlayerDataRepo(logger));
        //RegSGT(new PlayerInfoRepo(logger));
        //RegSGT(new ItemDropRepo(logger));
        //RegSGT(new ItemDropService(GS<ItemDropRepo>(),GS<IItemTemplateService>(), GS<IPlayerManagerService>()));
        //RegSGT(new ItemsService(GS<ItemDropService>(), GS<IItemTemplateService>(), GS<ItemsRepo>(), GS<IItemGenerationService>(), GS<IPlayerManagerService>(), GS<IGearService>()));
        //RegSGT<IFCMService>(new FCMService(new PushNotifier()));
        //RegSGT<IMailService>(new MailService(GS<MailRepo>(), GS<IFCMService>()));
        //RegSGT<IAuctionHouseService>(new AuctionHouseService(GS<AuctionHouseRepo>(),
        //    GS<PlayerDataRepo>(),
        //    GS<ItemsRepo>(),
        //    GS<IMailService>(),
        //    GS<ICloudCache>(),
        //    logger));

        //RegSGT(new DailyLoginWeekRepo(logger));
        //RegSGT(new SellService(GS<ItemsRepo>(), GS<PlayerDataRepo>(), logger));
        //RegSGT(new QuestRepo(logger));
        //RegSGT(new ClaimedRewardRepo(logger));
        
        //RegSGT(new QuestService(GS<QuestRepo>()));
        //RegSGT<IDailyLoginWeekService>(new DailyLoginWeekService(GS<DailyLoginWeekRepo>(), GS<ICloudCache>(), GS<QuestService>()));

        //RegSGT<IPlayerManagerService>(new PlayerManagerService(
        //    GS<ItemsRepo>(),
        //    GS<PlayerDataRepo>(),
        //    GS<IAuctionHouseService>(),
        //    GS<IMailService>(),
        //    logger));

        //RegSGT<IEnergyService>(new EnergyService(GS<IPlayerManagerService>()));
        //RegSGT<IRunService>(new RunService(GS<IEnergyService>(), GS<IPlayerManagerService>(), GS<ItemDropService>()));

        //RegSGT<IRewardService>(
        //    new RewardService(GS<ClaimedRewardRepo>(),
        //    GS<IItemGenerationService>(),
        //    GS<IPlayerManagerService>(), 
        //    logger,
        //    GS<IEnergyService>(),
        //    GS<ItemsService>()));

        //RegSGT(new DailyRewardService(GS<IDailyLoginWeekService>(), GS<PlayerDataRepo>(), GS<IRewardService>(), logger));


        //RegSGT(new BlacksmithService(
        //    GS<IPlayerManagerService>(),
        //    GS<IItemTemplateService>(),
        //    GS<ItemsRepo>(),
        //    GS<IItemGenerationService>(),
        //    GS<PlayerDataRepo>()));

       
        //RegSGT(new ArmoryService(
        //    GS<IItemTemplateService>(),
        //    GS<ItemsRepo>(),
        //    GS<IItemGenerationService>(),
        //    GS<PlayerDataRepo>(),
        //    GS<ItemsService>()));

        //RegSGT(new AlterItemService(
        //    GS<IItemTemplateService>(),
        //    GS<ItemsRepo>(),
        //    GS<PlayerDataRepo>()));

        //RegSGT(new ReforgeService(
        //    GS<IItemTemplateService>(),
        //    GS<ItemsRepo>(),
        //    GS<PlayerDataRepo>()));
    }

    public static void RegSGT<T>(T implementation)
    {
        _services[typeof(T)] = implementation;
    }

    public static T GS<T>()
    {
        Initialize(Holder.Logger);

        if (_services.TryGetValue(typeof(T), out var service))
        {
            return (T)service;
        }

        throw new InvalidOperationException($"Service of type {typeof(T).Name} is not registered");
    }

    public static void Reset()
    {
        _services.Clear();
        _isInitialized = false;
    }
}

// Helper class to wrap ILogger to be ILogger<T>
public class Holder
{
    public static ILogger Logger;
}