using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Google.Apis.Json;

public class BaseFireStoreService
{
    protected readonly ILogger _logger;
    protected readonly string _collectionName;
    private const string PROJECT_ID = "survivorforge-a1fa4";
    protected FirestoreDb _firestoreDb;
    protected readonly TimeSpan _timeout = TimeSpan.FromSeconds(10);

    // Built once per process and shared across all repos/requests.
    // FirestoreDb is thread-safe; reusing it avoids a new gRPC channel + OAuth
    // token exchange on every Cloud Code call.
    private static readonly Lazy<FirestoreDb> _sharedDb = new Lazy<FirestoreDb>(
        BuildFirestoreDb, System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);
    protected CancellationTokenSource GetToken => new CancellationTokenSource(_timeout);

    private static readonly string ServiceAccountJson = @"{
  ""type"": ""service_account"",
  ""project_id"": ""survivorforge-a1fa4"",
  ""private_key_id"": ""2bb90d633c5d9ef04cceb96537516d2bd0a77a20"",
  ""private_key"": ""-----BEGIN PRIVATE KEY-----\nMIIEvQIBADANBgkqhkiG9w0BAQEFAASCBKcwggSjAgEAAoIBAQClg4i5LeUz7oRD\nDkSPLKOL4l1RpbAAu1EedB0rxUORrGVpZ13YzfQfPYCd6LzXpxM+wYRDZfiaHC/V\n0ABRpjNT9avEE6H7j46BoVh2xkqx0JnfqwRaE5dgNYeIL2MUtegGUDqC3ejJ31Jp\ntZmpAcsSXlaWQ4ugNL3RquFnsSxyf6Dqtj8VyDopo/WiBADYSTThORmh8VuVD7NM\nBXReapbjYyxIk0Ifnt8D5jveFRNoJjz6DB4Ubnb1uQfuOytOjG4BxDSyf0J6S5s8\nojFzjmZ2/coOxdOMwAv01YrcO856KikY0Ld6gsMCgsexsFmlMlDab5Jzs05/ujyD\n/yciBlS3AgMBAAECggEAQZWyarZt+TsNO8j+j8E/eSGUmtLNwCx5g1i9ame7U5D9\nilvESzGpB2wGeAwOMGHq9VYXQ+G3NdHvfojyZ2g1NJLHomWpLRaAgItfk4kVdgpT\nvtZ0XgEfXac4clVcrIs8Sqwl0099e+i7LlogbmDgiAHLy7UuMahUgy10LfN9ykx2\ne8qeMqHRAJLDCmzzu5Ew+kEdLrih1wPbXzVFpGmpmK0Lx95+uBGzNoV3Ei6gDfzv\nxSv7k6MM0ZpqwKzEhoYTs26BnOpZ6HQrhIA6gW6fHgBUsCXtMzssQly++GlKAEz9\nqZISQxjoHe/NJjQnl7QhBrUr7mwJlqf77plWCKxhKQKBgQDkuoVplB3CfxtzIuin\nMf1Gdjim4+R62raoteXhKRLM2b1judI4PTD8zZnlTnTC+qaOdcCcOcQ8lAPoT4+2\nqzYkL+aGWe7qP/80cG3iKLiETUaSS/pDYRipzWsJXF2SZSSSLivz37K16XHV5yFD\n3+jLB67p+GoWE8fOIDkwSMa1ywKBgQC5P4JKPctHNe+tMvlq72xeSn1N3ZZrcSfl\nV7TiZmmfQEOQiTYQ3kFVPeljcuS3QmZrE+hKNcP7dknbKmP+YTRLy9SzNiuChSOP\nfo5clYmWcrWGe0ASAelqrQAgOt67vv3wrVmKP/DbLdPnSuLcC00BWC1qBBquXVyL\nuJuZPONfRQKBgCAa6kUSjEbTrpevq7ZkNufgCj2quVRhRMhTl+vN5xyn54SK/a31\nMN4bQeHE7HC93AkdECcdmKTkJdRC7qo2SAg2TQZIs1dgGkNTEKlxC81OR3ybe4pf\no6WZ9s8+jsfQuJxoLLUzjhMorFjY83tif3uYHX4r50AoEKCnNz/Gx1WXAoGAFVaQ\nbnbRvIEk+4Fco4mypOXVqyt0ESo2mz9QEagUUIi5f6Oee7DuO6BnCtKIL+fRHlJr\ntmidC5EjjsBCRmHyAS0YchAyUNV/NB8yzM1YzOCnsjbuOWTI+0W0NwIw99VXMOr2\n3mrhFo4f04uXuBsBIr+Cc6zHWgBdEacRHUSbODUCgYEAptcEDaoAbnRsHa2DLJUT\nHvL64l4g4xVUcNzFEUvU2J1DX3VgiqMb86/GEFuzvF9DuKtQZukkcp7QU1Keq5F2\nIFb/2FBNPNiqPTmT8kZ+B4du3DOhdtRiyDVDZRNvLVltYaS73fe0qOhLfOR49gI3\n8MdyTrQivEPYlb6GOz1RIEc=\n-----END PRIVATE KEY-----\n"",
  ""client_email"": ""firebase-adminsdk-fbsvc@survivorforge-a1fa4.iam.gserviceaccount.com"",
  ""client_id"": ""113624286970388499811"",
  ""auth_uri"": ""https://accounts.google.com/o/oauth2/auth"",
  ""token_uri"": ""https://oauth2.googleapis.com/token"",
  ""auth_provider_x509_cert_url"": ""https://www.googleapis.com/oauth2/v1/certs"",
  ""client_x509_cert_url"": ""https://www.googleapis.com/robot/v1/metadata/x509/firebase-adminsdk-fbsvc%40survivorforge-a1fa4.iam.gserviceaccount.com"",
  ""universe_domain"": ""googleapis.com""
}";

    public BaseFireStoreService(string collectionName, ILogger logger)
    {
        _logger = logger;
        _collectionName = collectionName;

        if (!InitializeFirestore())
        {
            throw new InvalidOperationException("Failed to initialize Firestore");
        }
    }

    private static FirestoreDb BuildFirestoreDb()
    {
        var credential = GoogleCredential.FromJson(ServiceAccountJson)
            .CreateScoped("https://www.googleapis.com/auth/cloud-platform");

        var builder = new FirestoreDbBuilder
        {
            ProjectId = PROJECT_ID,
            Credential = credential
        };

        return builder.Build();
    }

    protected bool InitializeFirestore()
    {
        try
        {
            // Reuse the process-wide instance instead of building a new one per repo/request.
            _firestoreDb = _sharedDb.Value;

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to initialize Firestore: {0}", ex.Message);
            if (ex.InnerException != null)
            {
                _logger.LogError("Inner exception: {0}", ex.InnerException.Message);
            }

            return false;
        }
    }

    protected async Task<IReadOnlyList<DocumentSnapshot>> ReadRecords()
    {
        using (var cts = new CancellationTokenSource(_timeout))
        {
            var itemsRef = _firestoreDb.Collection(_collectionName);
            var snapshot = await itemsRef.GetSnapshotAsync(cts.Token);

            _logger.LogInformation($"Query executed, found {snapshot.Documents.Count} documents");

            return snapshot.Documents;
        }
    }
    protected async Task<List<DocumentSnapshot>> ReadRecordsByIds(List<string> documentIds)
    {
        if (_firestoreDb == null && !InitializeFirestore())
        {
            throw new InvalidOperationException("Failed to initialize Firestore");
        }

        if (documentIds == null || documentIds.Count == 0)
        {
            return new List<DocumentSnapshot>();
        }

        try
        {
            using (var cts = new CancellationTokenSource(_timeout))
            {
                var collection = _firestoreDb.Collection(_collectionName);

                // Firestore's whereIn supports max 30 elements
                var snapshots = new List<DocumentSnapshot>();
                foreach (var chunk in documentIds.Chunk(30)) // .NET 6+ has Enumerable.Chunk
                {
                    var query = collection.WhereIn(FieldPath.DocumentId, chunk.ToList());
                    var result = await query.GetSnapshotAsync(cts.Token);
                    snapshots.AddRange(result.Documents);
                }

                _logger?.LogInformation($"Retrieved {snapshots.Count} documents using whereIn on document IDs.");
                return snapshots;
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Error retrieving documents by IDs: {ex.Message}");
            return new List<DocumentSnapshot>();
        }
    }

    protected async Task<DocumentSnapshot> ReadRecordById(string documentId, Transaction tran = null)
    {
        if (_firestoreDb == null)
        {
            if (!InitializeFirestore())
            {
                throw new InvalidOperationException("Failed to initialize Firestore");
            }
        }

        using (var cts = new CancellationTokenSource(_timeout))
        {
            var docRef = _firestoreDb.Collection(_collectionName).Document(documentId);

            if (tran != null)
            {
                return await tran.GetSnapshotAsync(docRef);
            }

            var snapshot = await docRef.GetSnapshotAsync(cts.Token);

            if (snapshot.Exists)
            {
                return snapshot;
            }
            else
            {             
                return null;
            }
        }
    }
    protected async Task<List<T>> ReadRecordsByField<T>(string fieldKey, object fieldValue, Transaction tran = null) where T : new()
    {
        if (_firestoreDb == null)
        {
            if (!InitializeFirestore())
            {
                throw new InvalidOperationException("Failed to initialize Firestore");
            }
        }

        try
        {
            using (var cts = new CancellationTokenSource(_timeout))
            {
                var query = _firestoreDb.Collection(_collectionName)
                    .WhereEqualTo(fieldKey, fieldValue);

                if (tran != null)
                {
                    var docs = await tran.GetSnapshotAsync(query);
                    return ConvertDocuments<T>(docs.Documents.ToList());
                }

                var snapshot = await query.GetSnapshotAsync(cts.Token);

                return ConvertDocuments<T>(snapshot.Documents.ToList());
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Error querying documents by field: {ex.Message}");
            return new List<T>();
        }
    }

    protected T ConvertDocument<T>(DocumentSnapshot document) where T : new()
    {
        if (document == null || !document.Exists)
        {
            return default;
        }

        try
        {
            return document.ConvertTo<T>();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error converting document: {ex.Message}");
            return default;
        }
    }

    protected List<T> ConvertDocuments<T>(List<DocumentSnapshot> documents) where T : new()
    {
        if (documents == null || documents.Count <= 0)
        {
            return default;
        }

        List<T> list = new List<T>();
        try
        {
            foreach (DocumentSnapshot document in documents)
            {
                list.Add(document.ConvertTo<T>());
            }

            return list;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error converting document: {ex.Message}");
            return default;
        }
    }

    // A more complete method that gets and converts the document in one step
    protected async Task<T> GetRecordById<T>(string documentId, Transaction tran = null) where T : new()
    {
        var document = await ReadRecordById(documentId, tran);
        return ConvertDocument<T>(document);
    }

    protected async Task<T> GetRecordByIdWithRef<T, D>(string documentId, string refFieldName, string refField, Transaction tran = null) where T : new()
    {
        var document = await ReadRecordById(documentId, tran);
        var record = ConvertDocument<T>(document);

        using (var cts = new CancellationTokenSource(_timeout))
        {
            if (refFieldName != null)
            {
                // Get the reference value
                DocumentReference docRef = document.GetValue<DocumentReference>(refFieldName);
                if (docRef != null)
                {
                    try
                    {
                        // Fetch the referenced document
                        DocumentSnapshot refDoc = await docRef.GetSnapshotAsync(cts.Token);

                        // Find the property that should hold the referenced item
                        var refItemProperty = typeof(T).GetProperty(refField);
                        if (refItemProperty != null && refDoc.Exists)
                        {
                            // Get the type of the referenced item
                            //Type refItemType = refItemProperty.PropertyType;

                            // Convert and set the referenced item
                            var refItem = refDoc.ConvertTo<D>();
                            refItemProperty.SetValue(record, refItem);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError($"Failed to fetch referenced document: {ex.Message}");
                    }
                }
            }

            return record;
        }
    }

    /// <summary>
    /// Builds a dictionary of only [FirestoreProperty] properties for Firestore writes.
    /// Skips Id and null values. Caller adds Id when needed (Create/Store).
    /// </summary>
    private static Dictionary<string, object> ToFirestorePropertyDictionary(FireBaseEntity item)
    {
        var dict = new Dictionary<string, object>();
        foreach (var prop in item.GetType().GetProperties())
        {
            if (prop.Name == "Id") continue;
            if (!Attribute.IsDefined(prop, typeof(FirestorePropertyAttribute))) continue;
            var value = prop.GetValue(item);
            //if (value == null) continue;
            dict[prop.Name] = value;
        }
        return dict;
    }

    protected async Task<T> UpdateRecord<T>(T item, Transaction tran = null) where T : FireBaseEntity
    {
        if (_firestoreDb == null)
        {
            if (!InitializeFirestore())
            {
                throw new InvalidOperationException("Failed to initialize Firestore");
            }
        }

        using (var cts = new CancellationTokenSource(_timeout))
        {
            var docRef = _firestoreDb.Collection(_collectionName).Document(item.Id);
            var itemData = ToFirestorePropertyDictionary(item);

            if (tran != null)
            {
                tran.Update(docRef, itemData);
                return item;
            }

            await docRef.UpdateAsync(itemData, cancellationToken: cts.Token);
            return item;
        }
    }


    /// <summary>
    /// Upserts a record (creates if it doesn't exist, updates if it does).
    /// Only includes properties that are not null or default values to avoid overwriting existing data.
    /// </summary>
    protected async Task UpsertRecord<T>(T item, Transaction tran = null) where T : FireBaseEntity
    {
        if (_firestoreDb == null && !InitializeFirestore())
            throw new InvalidOperationException("Failed to initialize Firestore");

        using var cts = new CancellationTokenSource(_timeout);

        var collection = _firestoreDb.Collection(_collectionName);
        DocumentReference docRef;

        if (string.IsNullOrWhiteSpace(item.Id))
        {
            // Generate new ID
            docRef = collection.Document();
            item.Id = docRef.Id;
        }
        else
        {
            docRef = collection.Document(item.Id);
        }

        // Only include properties that are not null or default values
        var itemData = new Dictionary<string, object>();
        var excplicitData = new Dictionary<string, object>();
        var properties = item.GetType().GetProperties();

        foreach (var prop in properties)
        {
            // Skip the Id property as it's handled separately
            if (prop.Name == "Id" || !Attribute.IsDefined(prop, typeof(FirestorePropertyAttribute)))
                continue;

            var value = prop.GetValue(item);

            

            if (item.ExplicitUpserts != null && item.ExplicitUpserts.Contains(prop.Name))
            {
                excplicitData[prop.Name] = value;
                continue;
            }

            // Skip null values for reference types
            if (value == null)
            {
                continue;
            }

            // Skip default values for value types
            if (prop.PropertyType.IsValueType)
            {
                var defaultValue = Activator.CreateInstance(prop.PropertyType);
                if (value.Equals(defaultValue))
                    continue;
            }

            // Skip empty strings
            if (value is string str && string.IsNullOrWhiteSpace(str))
                continue;

            itemData[prop.Name] = value;
        }

        // Always include the Id
        itemData["Id"] = item.Id;

        if (tran != null)
        {
            tran.Set(docRef, itemData, SetOptions.MergeAll);
            if (excplicitData.Count > 0)
            {
                tran.Update(docRef, excplicitData);
            }
        }
        else
        {
            await docRef.SetAsync(itemData, SetOptions.MergeAll, cts.Token);
            if (excplicitData.Count > 0)
            {
                await docRef.UpdateAsync(excplicitData);
            }
        }
    }

    /// <summary>
    /// Upserts a record (creates if it doesn't exist, updates if it does).
    /// Only includes properties that are not null or default values to avoid overwriting existing data.
    /// </summary>
    protected async Task<T> UpsertRecord<T>(T item, T oldItem, Transaction tran = null) where T : FireBaseEntity
    {
        if (_firestoreDb == null && !InitializeFirestore())
            throw new InvalidOperationException("Failed to initialize Firestore");

        using var cts = new CancellationTokenSource(_timeout);

        var collection = _firestoreDb.Collection(_collectionName);
        DocumentReference docRef;

        if (string.IsNullOrWhiteSpace(item.Id))
        {
            // Generate new ID
            docRef = collection.Document();
            item.Id = docRef.Id;
        }
        else
        {
            docRef = collection.Document(item.Id);
        }

        // Only include properties that are not null or default values
        var itemData = new Dictionary<string, object>();
        var excplicitData = new Dictionary<string, object>();
        var properties = item.GetType().GetProperties();

        foreach (var prop in properties)
        {
            // Skip the Id property as it's handled separately
            if (prop.Name == "Id" || !Attribute.IsDefined(prop, typeof(FirestorePropertyAttribute)))
                continue;

            var value = prop.GetValue(item);            

            if (item.ExplicitUpserts.Contains(prop.Name))
            {
                excplicitData[prop.Name] = value;
                prop.SetValue(oldItem, value);

                continue;
            }

            // Skip null values for reference types
            if (value == null)
            {
                continue;
            }

            // Skip default values for value types
            if (prop.PropertyType.IsValueType)
            {
                var defaultValue = Activator.CreateInstance(prop.PropertyType);
                if (value.Equals(defaultValue))
                    continue;
            }

            // Skip empty strings
            if (value is string str && string.IsNullOrWhiteSpace(str))
                continue;

            itemData[prop.Name] = value;
            prop.SetValue(oldItem, value);
        }

        // Always include the Id
        itemData["Id"] = item.Id;

        if (tran != null)
        {
            tran.Set(docRef, itemData, SetOptions.MergeAll);
            if (excplicitData.Count > 0)
            {
                tran.Update(docRef, excplicitData);
            }
        }
        else
        {
            await docRef.SetAsync(itemData, SetOptions.MergeAll, cts.Token);
            if(excplicitData.Count > 0)
            {
                await docRef.UpdateAsync(excplicitData);
            }
        }

        return oldItem;
    }

    protected async Task<T> CreateRecord<T>(T item, Transaction tran = null) where T : FireBaseEntity
    {
        if (_firestoreDb == null && !InitializeFirestore())
            throw new InvalidOperationException("Failed to initialize Firestore");

        using var cts = new CancellationTokenSource(_timeout);

        var collection = _firestoreDb.Collection(_collectionName);
        DocumentReference docRef;

        if (string.IsNullOrWhiteSpace(item.Id))
        {
            docRef = collection.Document();
            item.Id = docRef.Id;
        }
        else
        {
            docRef = collection.Document(item.Id);
        }

        var itemData = ToFirestorePropertyDictionary(item);
        itemData["Id"] = item.Id;

        if (tran != null)
        {
            tran.Set(docRef, itemData, SetOptions.MergeAll);
        }
        else
        {
            await docRef.SetAsync(itemData, SetOptions.MergeAll, cts.Token);
        }

        return item;
    }

    /// <summary>
    /// Creates a document only if it does not already exist (create-unique).
    /// When a transaction is provided: reads the document first; if it does not exist, uses Transaction.Create.
    /// When no transaction: runs an internal transaction that reads then creates only if not exists.
    /// Does not overwrite existing documents.
    /// </summary>
    protected async Task<T> CreateUniqueRecord<T>(T item, Transaction tran = null) where T : FireBaseEntity
    {
        if (_firestoreDb == null && !InitializeFirestore())
            throw new InvalidOperationException("Failed to initialize Firestore");

        using var cts = new CancellationTokenSource(_timeout);

        var collection = _firestoreDb.Collection(_collectionName);
        DocumentReference docRef;

        if (string.IsNullOrWhiteSpace(item.Id))
        {
            docRef = collection.Document();
            item.Id = docRef.Id;
        }
        else
        {
            docRef = collection.Document(item.Id);
        }

        var itemData = ToFirestorePropertyDictionary(item);
        itemData["Id"] = item.Id;

        if (tran != null)
        {
            var snapshot = await tran.GetSnapshotAsync(docRef, cts.Token);
            if (!snapshot.Exists)
                tran.Create(docRef, itemData);
        }
        else
        {
            await RunOurTransactionAsync(async t =>
            {
                var snapshot = await t.GetSnapshotAsync(docRef, cts.Token);
                if (!snapshot.Exists)
                    t.Create(docRef, itemData);
            }, null, cts.Token);
        }

        return item;
    }

    protected async Task<T> StoreRecord<T>(T item, Dictionary<string, string> references = null, string refCol = "") where T : FireBaseEntity
    {
        if (_firestoreDb == null)
        {
            if (!InitializeFirestore())
            {
                throw new InvalidOperationException("Failed to initialize Firestore");
            }
        }

        using (var cts = new CancellationTokenSource(_timeout))
        {
            // Create a new document reference
            var itemsRef = _firestoreDb.Collection(_collectionName);
            var docRef = itemsRef.Document();
            item.Id = docRef.Id;
            Dictionary<string, object> itemData;
            if (item is Dictionary<string, object> dictItem)
            {
                itemData = dictItem;
            }
            else
            {
                itemData = ToFirestorePropertyDictionary(item);
                itemData["Id"] = item.Id;
            }

            if (references != null)
            {
                foreach (var refa in references)
                {
                    itemData[refa.Key] = _firestoreDb.Collection(refCol).Document(refa.Value); ;
                }
            }

            // Add the reference to the item data

            // Store the item with the reference in a single operation
            await docRef.SetAsync(itemData, cancellationToken: cts.Token);

            return item;
        }
    }

    /// <summary>
    /// Stores multiple records in a single batch commit.
    /// Mirrors <see cref="StoreRecord{T}"/> behavior:
    /// - generates doc ids
    /// - writes FirestoreProperty dictionary + Id
    /// - optionally attaches references to another collection
    /// </summary>
    /// <summary>
    /// Transaction-aware variant of <see cref="StoreRecord{T}"/>. Generates a new doc id and
    /// writes the record (plus optional references) through the supplied transaction so the
    /// create participates in the same atomic commit as its caller.
    /// </summary>
    protected T StoreRecord<T>(T item, Transaction tran, Dictionary<string, string> references = null, string refCol = "") where T : FireBaseEntity
    {
        if (_firestoreDb == null && !InitializeFirestore())
        {
            throw new InvalidOperationException("Failed to initialize Firestore");
        }

        var docRef = _firestoreDb.Collection(_collectionName).Document();
        item.Id = docRef.Id;

        var itemData = ToFirestorePropertyDictionary(item);
        itemData["Id"] = item.Id;

        if (references != null)
        {
            foreach (var refa in references)
            {
                itemData[refa.Key] = _firestoreDb.Collection(refCol).Document(refa.Value);
            }
        }

        tran.Set(docRef, itemData);
        return item;
    }

    protected async Task<List<T>> StoreRecords<T>(List<T> items, Dictionary<string, string> references = null, string refCol = "")
        where T : FireBaseEntity
    {
        if (_firestoreDb == null)
        {
            if (!InitializeFirestore())
                throw new InvalidOperationException("Failed to initialize Firestore");
        }

        if (items == null || items.Count == 0)
            return items ?? new List<T>();

        using (var cts = new CancellationTokenSource(_timeout))
        {
            var itemsRef = _firestoreDb.Collection(_collectionName);
            var batch = _firestoreDb.StartBatch();

            foreach (var item in items)
            {
                if (item == null) continue;

                var docRef = itemsRef.Document();
                item.Id = docRef.Id;

                Dictionary<string, object> itemData;
                if (item is Dictionary<string, object> dictItem)
                {
                    itemData = dictItem;
                }
                else
                {
                    itemData = ToFirestorePropertyDictionary(item);
                    itemData["Id"] = item.Id;
                }

                if (references != null)
                {
                    foreach (var refa in references)
                    {
                        itemData[refa.Key] = _firestoreDb.Collection(refCol).Document(refa.Value);
                    }
                }

                batch.Set(docRef, itemData);
            }

            await batch.CommitAsync(cts.Token);
            return items;
        }
    }

    protected CollectionReference GetCollectionRef()
    {
        return _firestoreDb.Collection(_collectionName);
    }
    protected async Task<List<T>> GetRecordsByQuery<T>(Query query)
    {
        try
        {
            using (var cts = new CancellationTokenSource(_timeout))
            {
                // Query items where OwnerId matches the provided playerId
                QuerySnapshot snapshot = await query.GetSnapshotAsync(cts.Token);

                //_logger.LogInformation($"Found {snapshot.Documents.Count} items for player {playerId}");

                var items = new List<T>();
                foreach (DocumentSnapshot document in snapshot.Documents)
                {
                    if (document.Exists)
                    {
                        try
                        {
                            T item = document.ConvertTo<T>();
                            items.Add(item);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Error converting document {document.Id}: {ex.Message}");
                        }
                    }
                }

                return items;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving player items: {ex.Message}");
            return new List<T>();
        }
    }

    protected async Task<List<T>> GetRecords<T>()
    {
        using (var cts = new CancellationTokenSource(_timeout))
        {
            CollectionReference itemsRef = _firestoreDb.Collection(_collectionName);
            QuerySnapshot snapshot = await itemsRef.GetSnapshotAsync(cts.Token);

            var itemTemplates = new List<T>();

            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                if (document.Exists)
                {
                    itemTemplates.Add(document.ConvertTo<T>());
                }
            }

            // Return test data if no items were found in the database
            if (itemTemplates.Count == 0)
            {             
                return null;
            }

            return itemTemplates;
        }
    }

    protected async Task<List<T>> GetRecordsWithRefs<T, D>(string refFieldName, string refField)
    {
        using (var cts = new CancellationTokenSource(_timeout))
        {
            // Get the base records
            CollectionReference collectionRef = _firestoreDb.Collection(_collectionName);
            QuerySnapshot snapshot = await collectionRef.GetSnapshotAsync(cts.Token);

            var records = new List<T>();

            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                if (document.Exists)
                {
                    // Convert document to type T
                    T record = document.ConvertTo<T>();
                    
                    if (refFieldName != null)
                    {
                        // Get the reference value
                        DocumentReference docRef = document.GetValue<DocumentReference>(refFieldName);
                        if (docRef != null)
                        {
                            try
                            {
                                // Fetch the referenced document
                                DocumentSnapshot refDoc = await docRef.GetSnapshotAsync(cts.Token);

                                // Find the property that should hold the referenced item
                                var refItemProperty = typeof(T).GetProperty(refField);
                                if (refItemProperty != null && refDoc.Exists)
                                {
                                    // Get the type of the referenced item
                                    //Type refItemType = refItemProperty.PropertyType;

                                    // Convert and set the referenced item
                                    var refItem = refDoc.ConvertTo<D>();
                                    refItemProperty.SetValue(record, refItem);
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger?.LogError($"Failed to fetch referenced document: {ex.Message}");
                            }
                        }
                    }

                    records.Add(record);
                }
            }

            // Return test data if no items were found in the database
            if (records.Count == 0)
            {
                _logger?.LogWarning("No records found in database, returning null");
                return null;
            }

            return records;
        }
    }

    protected async Task<bool> DeleteRecordById(string documentId, Transaction tran = null)
    {
        if (_firestoreDb == null)
        {
            if (!InitializeFirestore())
            {
                throw new InvalidOperationException("Failed to initialize Firestore");
            }
        }

        try
        {
            using (var cts = new CancellationTokenSource(_timeout))
            {
                var docRef = _firestoreDb.Collection(_collectionName).Document(documentId);

                if (tran != null)
                {
                    tran.Delete(docRef);
                    return true;
                }

                await docRef.DeleteAsync(cancellationToken: cts.Token);
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Error deleting document {documentId}: {ex.Message}");
            return false;
        }
    }

    protected async Task<bool> DeleteRecordsByIds(List<string> documentIds, Transaction tran = null)
    {
        if (_firestoreDb == null && !InitializeFirestore())
        {
            throw new InvalidOperationException("Failed to initialize Firestore");
        }

        if (documentIds == null || documentIds.Count == 0)
        {
            _logger?.LogWarning("No document IDs provided for deletion.");
            return false;
        }

        try
        {
            if (tran != null)
            {
                // Use transaction to delete all records
                foreach (var documentId in documentIds)
                {
                    var docRef = _firestoreDb.Collection(_collectionName).Document(documentId);
                    tran.Delete(docRef);
                }
                return true;
            }
            else
            {
                // Use batch for non-transactional deletes
                using (var cts = new CancellationTokenSource(_timeout))
                {
                    var batch = _firestoreDb.StartBatch();

                    foreach (var documentId in documentIds)
                    {
                        var docRef = _firestoreDb.Collection(_collectionName).Document(documentId);
                        batch.Delete(docRef);
                    }

                    await batch.CommitAsync(cts.Token);
                    return true;
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Error deleting documents: {ex.Message}");
            return false;
        }
    }

    public virtual async Task RunOurTransactionAsync(
        Func<Transaction, Task> callback,
        TransactionOptions options = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Use longer timeout only if using the default options
            if (options == TransactionOptions.Default)
            {
                // Unfortunately TransactionOptions doesn't have setters for properties
                // so we might need to handle timeouts at a different level
                using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(2));
                var combinedToken = CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationToken, cts.Token).Token;
                cancellationToken = combinedToken;
            }

            await _firestoreDb.RunTransactionAsync(callback, options, cancellationToken);
            _logger?.LogInformation("Transaction completed successfully");
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Transaction failed: {ex.Message}");
            throw;
        }
    }
}