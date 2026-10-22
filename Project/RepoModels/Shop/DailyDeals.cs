using Google.Cloud.Firestore;
using System.Collections.Generic;

namespace CloudLogic.RepoModels.Shop
{
    [FirestoreData]
    public class DailyDeals : FireBaseEntity
    {
        [FirestoreProperty]
        public List<DailyDeal> Deals { get; set; } = new();
    }

    [FirestoreData]
    public class DailyDeal
    {
        [FirestoreProperty]
        public string ShopId { get; set; }

        [FirestoreProperty]
        public int Discount { get; set; }
    }
}
