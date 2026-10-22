using Google.Cloud.Firestore;
using System;

namespace CloudLogic.RepoModels.Purchase
{
    [FirestoreData]
    public class PurchaseRecord : FireBaseEntity
    {
        [FirestoreProperty] public string PlayerId { get; set; }
        [FirestoreProperty] public string ProductId { get; set; }
        [FirestoreProperty] public string Store { get; set; }
        [FirestoreProperty] public string PurchaseToken { get; set; }
        [FirestoreProperty] public string OrderId { get; set; }
        [FirestoreProperty] public int GemsGranted { get; set; }
        [FirestoreProperty] public DateTime GrantedAt { get; set; }
        [FirestoreProperty] public bool Refunded { get; set; }
    }
}
