using DinnersReady.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DinnersReady.Services;

public interface IRecipeStoreRepository
{
    Task SaveAsync(Recipe item);
    Task DeleteAsync(string itemId);
    Task<List<Recipe>> LoadAllAsync();
    Task ClearAllAsync();
}

public interface IRecipeStoreService
{
    Task AddRecipeAsync(Recipe item);
    Task RemoveRecipeAsync(Recipe item);
    Task<IEnumerable<Recipe>> GetRecipesAsync();
    Task ClearAllAsync();
}

public class RecipeStoreRepository(IStorageProvider storageProvider) : IRecipeStoreRepository
{
    private const string KeyPrefix = "recipe_";

    public async Task SaveAsync(Recipe recipe)
    {
        string key = $"{KeyPrefix}{recipe.Id}";
        await storageProvider.SaveItemAsync(key, recipe);
    }

    public async Task<List<Recipe>> LoadAllAsync() => await storageProvider.GetAllItemsAsync<Recipe>();

    public async Task<Recipe?> GetByIdAsync(string itemId)
    {
        string key = $"{KeyPrefix}{itemId}";
        return await storageProvider.GetItemAsync<Recipe>(key);
    }

    public async Task DeleteAsync(string itemId)
    {
        string key = $"{KeyPrefix}{itemId}";
        await storageProvider.DeleteItemAsync(key);
    }

    public async Task ClearAllAsync() => await storageProvider.ClearAllAsync();
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
        await recipeStoreRepository.DeleteAsync(item.Id);
    }

    public async Task<IEnumerable<Recipe>> GetRecipesAsync() => await recipeStoreRepository.LoadAllAsync();

    public async Task ClearAllAsync() => await recipeStoreRepository.ClearAllAsync();
}

