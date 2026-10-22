using CloudLogic.Util;
using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;

namespace CloudLogic.RepoModels.Player
{
    [FirestoreData]
    public class PlayerData : FireBaseEntity
    {
        [FirestoreProperty]
        public int SelectedLevel { get; set; }

        [FirestoreProperty]
        public long Gold { get; set; }

        [FirestoreProperty]
        public int RerollRuneCount { get; set; }

        [FirestoreProperty]
        public long HeroLevel { get; set; }

        [FirestoreProperty]
        public long MaxLevelReached { get; set; }

        [FirestoreProperty]
        public int ItemsDropped {  get; set; }

        [FirestoreProperty]
        public string PlayerId { get; set; }

        //[FirestoreProperty]
        //public string PlayerId { get; set; }

        [FirestoreProperty]
        public long Energy { get; set; }

        [FirestoreProperty]
        public long GoldDropped {  get; set; }

        [FirestoreProperty]
        public long Crystals {  get; set; }

        [FirestoreProperty]
        public long NLCR {  get; set; }

        [FirestoreProperty]
        public long TalentPoints { get; set; }

        [FirestoreProperty]
        public List<int> LearnedTalents {  get; set; }

        [FirestoreProperty]
        public string DeviceId { get; set; } 

        [FirestoreProperty]
        public Timestamp LastRunCompletedAt { get; set; }       

        [FirestoreProperty]
        public int AuctionsCount { get; set; }

        [FirestoreProperty]
        public LoginRewardInfo LoginRewardInfo { get; set; }

        [FirestoreProperty]
        public int BlueGems {  get; set; }

        [FirestoreProperty]
        public int TalentResetToken { get; set; }

        [FirestoreProperty]
        public int CraftingMaterials { get; set; }

        [FirestoreProperty]
        public RunData RunData { get; set; }

        [FirestoreProperty]
        public Timestamp NextBlueGemAdAt { get; set; }

        [FirestoreProperty]
        public Timestamp LastEnergyGiveStamp { get; set; }

        [FirestoreProperty]
        public Timestamp LastPigGoldTakenStamp { get; set; }

        [FirestoreProperty]
        public bool FirstRunStarted { get; set; }

        [FirestoreProperty]
        public string Platform { get; set; }

        [FirestoreProperty]
        public Timestamp CreatedAt { get; set; }

        [FirestoreProperty]
        public int ExtraStashSlots { get; set; }

        [FirestoreProperty]
        public List<int> DuelSpells { get; set; }

        [FirestoreProperty]
        public int DuelStance { get; set; }

        [FirestoreProperty]
        public List<string> DefeatedDuelers { get; set; }

        [FirestoreProperty]
        public Timestamp LastShopBuyAt { get; set; }

        [FirestoreProperty]
        public Dictionary<string, int> ShopBuysToday { get; set; }

        [FirestoreProperty]
        public Timestamp ForgeCardUntil { get; set; }

        [FirestoreProperty]
        public Timestamp ForgeCardClaimedAt { get; set; }

        [FirestoreProperty]
        public bool Premium { get; set; }

        [FirestoreProperty]
        public int Revives { get; set; }

        [FirestoreProperty]
        public int RunRevives { get; set; }

        public bool HasForgeCard(DateTime now)
        {
            return ForgeCardUntil != default && ForgeCardUntil.ToDateTime() > now;
        }

        public bool ClaimedForgeCardOn(DateTime now)
        {
            return ForgeCardClaimedAt != default && ForgeCardClaimedAt.ToDateTime().Date == now.Date;
        }

        public int StashLimit => GameConstants.DefaultStashSize + ExtraStashSlots;

        public int AuctionLimit => Premium ? GameConstants.PremiumAuctionSlots : GameConstants.DefaultAuctionSlots;

        public int BoughtToday(string shopId, DateTime now)
        {
            return BoughtOn(now) && ShopBuysToday.TryGetValue(shopId, out var count) ? count : 0;
        }

        public bool BoughtOn(DateTime now)
        {
            return LastShopBuyAt != default && LastShopBuyAt.ToDateTime().Date == now.Date && ShopBuysToday != null;
        }

        public DateTime NextBlueGemAdAvailableAt { get; set; }

        public DateTime LastEnergyGivenAt => LastEnergyGiveStamp.ToDateTime();

        public DateTime LastPigGoldTakenAt => LastPigGoldTakenStamp.ToDateTime();
    }
}
