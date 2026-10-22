using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;
using Unity.Services.CloudCode.Core;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using Google.Cloud.Firestore;
using CloudLogic.RepoModels.Items;

public class ItemTemplateRepo : BaseFireStoreService
{
    public ItemTemplateRepo(ILogger logger) : base("ItemTemplates", logger)
    {
    }



    public async Task<ItemTemplate> SaveNewTemplate(ItemTemplate item)
    {
        return await CreateRecord(item);
    }

    public async Task<bool> DeleteTemplate(string itemId)
    {
        return await DeleteRecordById(itemId);
    }

    private ItemTemplate ConvertToItemTemplate(DocumentSnapshot document)
    {
        try
        {
            var data = document.ToDictionary();

            var itemTemplate = new ItemTemplate
            {
                Id = document.Id,
                ItemId = GetStringValue(data, "ItemId"),
                Name = GetStringValue(data, "Name"),
                Level = GetIntValue(data, "Level"),
                Type = (ItemType)GetIntValue(data, "Type"),
                Rarity = (RarityType)GetIntValue(data, "Rarity"),
                MaxDamageRange = GetListOfInt(data, "MaxDamageRange"),
                DamageDeltaRange = GetListOfDouble(data, "DamageDeltaRange"),
                AdditionalDamageRange = GetListOfInt(data, "AdditionalDamageRange"),
                HealthRange = GetListOfInt(data, "HealthRange"),
                ArmorRange = GetListOfInt(data, "ArmorRange"),
                CriticalChanceRange = GetListOfInt(data, "CriticalChanceRange"),
                CriticalDamageRange = GetListOfInt(data, "CriticalDamageRange"),
                CritPowerRange = GetListOfInt(data, "CritPowerRange"),
                Icon = GetStringValue(data, "Icon"),
                WeaponName = GetStringValue(data, "WeaponName"),
                ModelName = GetStringValue(data, "ModelName"),
                AttackSpeedRange = GetListOfDouble(data, "AttackSpeedRange"),
                DodgeRange = GetListOfInt(data, "DodgeRange"),
                LuckRange = GetListOfInt(data, "LuckRange"),
                PhysicalDamageRange = GetListOfInt(data, "PhysicalDamageRange"),
                MagicalDamageRange = GetListOfInt(data, "MagicalDamageRange"),
                AdditionalRangeRange = GetListOfInt(data, "AdditionalRangeRange"),
                HealthPerSecondRange = GetListOfInt(data, "HealthPerSecondRange"),
                CooldownReductionRange = GetListOfInt(data, "CooldownReductionRange"),
                MoveSpeedRange = GetListOfInt(data, "MoveSpeedRange")
            };

            // Manually convert RarityOptions (int keys stored as string keys in Firestore)
            if (data.ContainsKey("RarityOptions") && data["RarityOptions"] != null)
            {
                try
                {
                    var rarityOptionsDict = data["RarityOptions"] as Dictionary<string, object>;
                    if (rarityOptionsDict != null && rarityOptionsDict.Count > 0)
                    {
                        itemTemplate.RarityOptions = new Dictionary<int, string>();
                        foreach (var kvp in rarityOptionsDict)
                        {
                            if (int.TryParse(kvp.Key, out var rarityInt))
                            {
                                itemTemplate.RarityOptions[rarityInt] = kvp.Value?.ToString() ?? string.Empty;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning("Error converting RarityOptions for document {0}: {1}", document.Id, ex.Message);
                    itemTemplate.RarityOptions = null;
                }
            }

            // Manually convert LegendaryStatOptions
            if (data.ContainsKey("LegendaryStatOptions") && data["LegendaryStatOptions"] != null)
            {
                try
                {
                    var legendaryDict = data["LegendaryStatOptions"] as Dictionary<string, object>;
                    if (legendaryDict != null && legendaryDict.Count > 0)
                    {
                        itemTemplate.LegendaryStatOptions = new Dictionary<string, List<int>>();
                        foreach (var kvp in legendaryDict)
                        {
                            if (kvp.Value is IEnumerable<object> list)
                            {
                                itemTemplate.LegendaryStatOptions[kvp.Key] = list.Select(x => Convert.ToInt32(x)).ToList();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning("Error converting LegendaryStatOptions for document {0}: {1}", document.Id, ex.Message);
                    itemTemplate.LegendaryStatOptions = null;
                }
            }

            return itemTemplate;
        }
        catch (Exception ex)
        {
            _logger?.LogError("Error converting document {0}: {1}", document.Id, ex.Message);
            return null;
        }
    }

    // Helper methods for safer data conversion
    private string GetStringValue(Dictionary<string, object> data, string key)
    {
        return data.ContainsKey(key) && data[key] != null ? data[key].ToString() : "";
    }

    private int GetIntValue(Dictionary<string, object> data, string key)
    {
        if (data.ContainsKey(key) && data[key] != null)
        {
            try
            {
                return Convert.ToInt32(data[key]);
            }
            catch
            {
                _logger?.LogWarning("Failed to convert {0} to int", key);
                return 0;
            }
        }
        return 0;
    }

    private double GetDoubleValue(Dictionary<string, object> data, string key)
    {
        if (data.ContainsKey(key) && data[key] != null)
        {
            try
            {
                return Convert.ToDouble(data[key]);
            }
            catch
            {
                _logger.LogWarning("Failed to convert {0} to double", key);
                return 0;
            }
        }
        return 0;
    }

    private List<int> GetListOfInt(Dictionary<string, object> data, string key)
    {
        if (data.ContainsKey(key) && data[key] != null)
        {
            try
            {
                if (data[key] is IEnumerable<object> list)
                {
                    return list.Select(item => Convert.ToInt32(item)).ToList();
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning("Failed to convert {0} to List<int>: {1}", key, ex.Message);
            }
        }
        return new List<int>();
    }

    private List<double> GetListOfDouble(Dictionary<string, object> data, string key)
    {
        if (data.ContainsKey(key) && data[key] != null)
        {
            try
            {
                if (data[key] is IEnumerable<object> list)
                {
                    return list.Select(item => Convert.ToDouble(item)).ToList();
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning("Failed to convert {0} to List<double>: {1}", key, ex.Message);
            }
        }
        return new List<double>();
    }

    [CloudCodeFunction("TestConnection")]
    public async Task<string> TestConnection(IExecutionContext ctx)
    {
        return "Connection successful";
    }

    public async Task<List<ItemTemplate>> GetItemTemplates()
    {
        _logger?.LogInformation("GetItemTemplates function called");

        try
        {
            // Initialize Firestore with explicit credentials
            if (!InitializeFirestore())
            {
                _logger?.LogError("Firestore initialization failed, returning test data");
                return null;
            }

            _logger?.LogInformation("Fetching items from Firestore collection: ItemTemplates");

            using (var cts = new CancellationTokenSource(_timeout))
            {
                CollectionReference itemsRef = _firestoreDb.Collection("ItemTemplates");
                QuerySnapshot snapshot = await itemsRef.GetSnapshotAsync(cts.Token);

                _logger?.LogInformation("Query executed, found {0} documents", snapshot.Documents.Count);

                var itemTemplates = new List<ItemTemplate>();

                foreach (DocumentSnapshot document in snapshot.Documents)
                {
                    if (document.Exists)
                    {
                        var template = ConvertToItemTemplate(document);
                        if (template != null)
                        {
                            itemTemplates.Add(template);
                        }
                    }
                }

                _logger?.LogInformation("Successfully retrieved {0} item templates", itemTemplates.Count);

                // Return test data if no items were found in the database
                if (itemTemplates.Count == 0)
                {
                    _logger?.LogWarning("No items found in database, returning test data");
                    return null;
                }

                return itemTemplates;
            }
        }
        catch (OperationCanceledException)
        {
            _logger?.LogError("Firestore query timed out after {0} seconds", _timeout.TotalSeconds);
            return null;
        }
        catch (Exception ex)
        {
            _logger?.LogError("Error retrieving item templates: {0}", ex.Message);
            if (ex.InnerException != null)
            {
                _logger?.LogError("Inner exception: {0}", ex.InnerException.Message);
            }

            return null; // Return test data on error
        }
    }
}