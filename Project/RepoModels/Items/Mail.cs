using Google.Cloud.Firestore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CloudLogic.RepoModels.Items
{
    [FirestoreData]
    [Serializable]
    public class Mail : FireBaseEntity
    {
        [FirestoreProperty]
        [JsonProperty(nameof(PlayerId))]
        public string PlayerId { get; set; }

        [FirestoreProperty]
        [JsonProperty(nameof(Title))]
        public string Title {  get; set; }

        [FirestoreProperty]
        [JsonProperty(nameof(Message))]
        public string Message { get; set; }

        [FirestoreProperty]
        [JsonProperty(nameof(MailData))]
        public string MailData { get; set; }

        [FirestoreProperty]
        [JsonProperty(nameof(Read))]
        public bool Read {  get; set; }

        [FirestoreProperty]
        [JsonProperty(nameof(Claim))]
        public Dictionary<string, int> Claim { get; set; }

        [FirestoreProperty]
        [JsonProperty(nameof(Gold))]
        public long Gold { get; set; }
    }
}
