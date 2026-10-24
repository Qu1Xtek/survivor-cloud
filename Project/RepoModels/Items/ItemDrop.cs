using Google.Cloud.Firestore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CloudLogic.RepoModels.Items
{
    [FirestoreData]
    public class ItemDrop : FireBaseEntity
    {
        [FirestoreProperty]
        public Dictionary<string, DroppedItem> RunDropBag { get; set; }
    }

    [FirestoreData]
    public class DroppedItem
    {
        [FirestoreProperty]        
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [FirestoreProperty]
        public string ItemTemplateId { get; set; }

        [FirestoreProperty]
        public RarityType Rarity { get; set; }

        [FirestoreProperty]
        public Timestamp DroppedAt { get; set; }

        [FirestoreProperty]
        public string PlayerId { get; set; }

        [FirestoreProperty]
        public DropType DropType { get; set; }

        [FirestoreProperty]
        public int Level { get; set; }

        public string AdditionalInfo { get; set; }
    }
}
