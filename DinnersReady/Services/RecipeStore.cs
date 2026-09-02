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

public interface IRecipeStoreRepository
{
    Task SaveAsync(Recipe item);
    Task DeleteAsync(Recipe item);
    Task<IEnumerable<Recipe>> LoadAllAsync();
    Task ClearAllAsync();
}

public interface IRecipeStoreService
{
    Task AddRecipeAsync(Recipe item);
    Task RemoveRecipeAsync(Recipe item);
    Task<IEnumerable<Recipe>> GetRecipesAsync();
    Task ClearAllAsync();
}

public class RecipeStoreRepository : IRecipeStoreRepository
{
    private readonly string _storageFolder;
    private readonly JsonSerializerOptions _jsonOptions;

    public RecipeStoreRepository()
    {
        _storageFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "DinnersReady", "RecipeStore"
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

    private string GetFilePathForItem(Recipe item)
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

    public async Task<Recipe?> GetByIdAsync(string itemId)
    {
        string filePath = GetFilePathFromId(itemId);
        if (!File.Exists(filePath)) return null;

        try
        {
            using FileStream stream = File.OpenRead(filePath);
            return await JsonSerializer.DeserializeAsync<Recipe>(stream, _jsonOptions).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Deserialization failed: {ex}");
            return null;
        }
    }

    public async Task SaveAsync(Recipe item)
    {
        string filePath = GetFilePathForItem(item);
        using FileStream stream = File.Create(filePath);
        await JsonSerializer.SerializeAsync(stream, item, _jsonOptions).ConfigureAwait(false);
    }

    public Task DeleteAsync(Recipe item)
    {
        string filePath = GetFilePathForItem(item);
        if (File.Exists(filePath))
            File.Delete(filePath);

        return Task.CompletedTask;
    }

    public async Task<IEnumerable<Recipe>> LoadAllAsync()
    {
        var items = new List<Recipe>();
        if (!Directory.Exists(_storageFolder)) return items;

        foreach (string filePath in Directory.GetFiles(_storageFolder, "*.json"))
        {
            try
            {
                using FileStream stream = File.OpenRead(filePath);
                var item = await JsonSerializer.DeserializeAsync<Recipe>(stream, _jsonOptions).ConfigureAwait(false);
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
            foreach (string filePath in Directory.GetFiles(_storageFolder, "*.json"))
                File.Delete(filePath);

        return Task.CompletedTask;
    }
}

public class RecipeStore(IRecipeStoreRepository recipeStoreRepository) : IRecipeStoreService
{
    public async Task AddRecipeAsync(Recipe item)
    {
        if (item == null) return;

        await recipeStoreRepository.SaveAsync(item);
    }

    public async Task RemoveRecipeAsync(Recipe item)
    {
        if (item == null) return;
        await recipeStoreRepository.DeleteAsync(item);
    }

    public async Task<IEnumerable<Recipe>> GetRecipesAsync() => await recipeStoreRepository.LoadAllAsync();

    public async Task ClearAllAsync() => await recipeStoreRepository.ClearAllAsync();
}

