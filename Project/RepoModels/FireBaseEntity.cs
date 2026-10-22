using Google.Cloud.Firestore;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

public class FireBaseEntity
{
    [FirestoreDocumentId]
    [JsonProperty("Id")]
    public string Id { get; set; }

    public List<string> ExplicitUpserts { get; set; } = new List<string>();

    public void ExplicitValuesUpsert(string[] explicits)
    {
        ExplicitUpserts = explicits.ToList();
    }
}