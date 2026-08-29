using DinnersReady.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DinnersReady.Services;

public interface IIngredientStoreRepository
{
    Task SaveAsync(Ingredient item);
    Task DeleteAsync(Ingredient item);
    Task<IEnumerable<Ingredient>> LoadAllAsync();
    Task ClearAllAsync();
}

public interface IIngredientStoreService
{
    Task AddIngredientAsync(Ingredient item);
    Task RemoveIngredientAsync(Ingredient item);
    Task<IEnumerable<Ingredient>> GetIngredientsAsync();
    Task ClearAllAsync();
}

public class IngredientStoreRepository : IIngredientStoreRepository
{
    private readonly string _storageFolder;
    private readonly JsonSerializerOptions _jsonOptions;

    public IngredientStoreRepository()
    {
        _storageFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "DinnersReady", "IngredientStore"
        );
        Directory.CreateDirectory(_storageFolder);

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            TypeInfoResolver = DinnersReadyJsonContext.Default,
            Converters = { new JsonStringEnumConverter() }
        };
    }

    private string GetFilePathForItem(Ingredient item)
    {
        ArgumentNullException.ThrowIfNull(item);
        return GetFilePathFromId(item.Id);
    }

    private string GetFilePathFromId(string itemId)
    {
        // Consistent hash naming across Save, Delete, and GetById
        byte[] hashBytes = MD5.HashData(Encoding.UTF8.GetBytes(itemId ?? string.Empty));
        string safeHash = Convert.ToHexString(hashBytes);
        return Path.Combine(_storageFolder, $"{safeHash}.json");
    }

    public async Task<Ingredient?> GetByIdAsync(string itemId)
    {
        string filePath = GetFilePathFromId(itemId);
        if (!File.Exists(filePath)) return null;

        try
        {
            using FileStream stream = File.OpenRead(filePath);
            return await JsonSerializer.DeserializeAsync<Ingredient>(stream, _jsonOptions).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Deserialization failed: {ex}");
            return null;
        }
    }

    public async Task SaveAsync(Ingredient item)
    {
        string filePath = GetFilePathForItem(item);
        using FileStream stream = File.Create(filePath);
        await JsonSerializer.SerializeAsync(stream, item, _jsonOptions).ConfigureAwait(false);
    }

    public Task DeleteAsync(Ingredient item)
    {
        string filePath = GetFilePathForItem(item);
        if (File.Exists(filePath))
            File.Delete(filePath);

        return Task.CompletedTask;
    }

    public async Task<IEnumerable<Ingredient>> LoadAllAsync()
    {
        var items = new List<Ingredient>();
        if (!Directory.Exists(_storageFolder)) return items;

        foreach (string filePath in Directory.GetFiles(_storageFolder, "*.json"))
        {
            try
            {
                using FileStream stream = File.OpenRead(filePath);
                var item = await JsonSerializer.DeserializeAsync<Ingredient>(stream, _jsonOptions).ConfigureAwait(false);
                if (item != null)
                    items.Add(item);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Deserialization failed: {ex}");
            }
        }

        return items;
    }

    public Task ClearAllAsync()
    {
        if (Directory.Exists(_storageFolder))
        {
            foreach (string filePath in Directory.GetFiles(_storageFolder, "*.json"))
            {
                File.Delete(filePath);
            }
        }

        return Task.CompletedTask;
    }
}

public class IngredientStore(IIngredientStoreRepository ingredientStoreRepository) : IIngredientStoreService
{
    public async Task AddIngredientAsync(Ingredient item)
    {
        if (item == null) return;

        await ingredientStoreRepository.SaveAsync(item);
    }

    public async Task RemoveIngredientAsync(Ingredient item)
    {
        if (item == null) return;
        await ingredientStoreRepository.DeleteAsync(item);
    }

    public async Task<IEnumerable<Ingredient>> GetIngredientsAsync() => await ingredientStoreRepository.LoadAllAsync();

    public async Task ClearAllAsync() => await ingredientStoreRepository.ClearAllAsync();
}
