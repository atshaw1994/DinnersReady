using DinnersReady.Models;
using System.Collections.Generic;
using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;
using System.Threading.Tasks;

namespace DinnersReady.Services;

public partial class WebLocalStorageProvider : IStorageProvider
{
    #region JSImports

    [JSImport("globalThis.localStorage.setItem")]
    private static partial void SetItem(string key, string value);

    [JSImport("globalThis.localStorage.getItem")]
    private static partial string? GetItem(string key);

    [JSImport("globalThis.localStorage.removeItem")]
    private static partial void RemoveItem(string key);

    [JSImport("globalThis.localStorage.clear")]
    private static partial void ClearStorage(); 

    #endregion

    private static string GetIndexKey<T>() => $"index_{typeof(T).Name}";

    private static HashSet<string> GetIndex<T>()
    {
        string? json = GetItem(GetIndexKey<T>());
        if (string.IsNullOrWhiteSpace(json)) return [];

        return JsonSerializer.Deserialize(json, DinnersReadyJsonContext.Default.GetTypeInfo(typeof(List<string>))!) is List<string> list ? [.. list] : [];
    }

    private static void SaveIndex<T>(HashSet<string> index)
    {
        var list = new List<string>(index);
        string json = JsonSerializer.Serialize(list, DinnersReadyJsonContext.Default.GetTypeInfo(typeof(List<string>))!);
        SetItem(GetIndexKey<T>(), json);
    }

    public Task SaveItemAsync<T>(string key, T item)
    {
        string json = JsonSerializer.Serialize(item, DinnersReadyJsonContext.Default.GetTypeInfo(typeof(T))!);
        SetItem(key, json);

        var index = GetIndex<T>();
        if (index.Add(key))
        {
            SaveIndex<T>(index);
        }

        return Task.CompletedTask;
    }

    public Task<T?> GetItemAsync<T>(string key)
    {
        string? json = GetItem(key);
        if (string.IsNullOrWhiteSpace(json)) return Task.FromResult<T?>(default);

        var result = (T?)JsonSerializer.Deserialize(json, DinnersReadyJsonContext.Default.GetTypeInfo(typeof(T))!);
        return Task.FromResult(result);
    }

    public Task<List<T>> GetAllItemsAsync<T>()
    {
        var items = new List<T>();
        var index = GetIndex<T>();
        var typeInfo = DinnersReadyJsonContext.Default.GetTypeInfo(typeof(T))!;

        foreach (string key in index)
        {
            string? json = GetItem(key);
            if (!string.IsNullOrWhiteSpace(json))
            {
                var item = JsonSerializer.Deserialize(json, typeInfo);
                if (item is T typedItem)
                {
                    items.Add(typedItem);
                }
            }
        }

        return Task.FromResult(items);
    }

    public Task DeleteItemAsync(string key)
    {
        RemoveItem(key);

        var index = GetIndex<Ingredient>();
        if (index.Remove(key))
        {
            SaveIndex<Ingredient>(index);
        }

        var recipeIndex = GetIndex<Recipe>();
        if (recipeIndex.Remove(key))
        {
            SaveIndex<Recipe>(recipeIndex);
        }

        return Task.CompletedTask;
    }

    public Task ClearAllAsync()
    {
        ClearStorage();
        return Task.CompletedTask;
    }
}