using DinnersReady.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace DinnersReady.Services;

public interface IIngredientStoreRepository
{
    Task SaveAsync(Ingredient item);
    Task DeleteAsync(Ingredient item);
    Task<List<Ingredient>> LoadAllAsync();
    Task ClearAllAsync();
}

public interface IIngredientStoreService
{
    Task AddIngredientAsync(Ingredient item);
    Task RemoveIngredientAsync(Ingredient item); 
    Task ModifyIngredientAsync(Ingredient item);
    Task<IEnumerable<Ingredient>> GetIngredientsAsync();
    Task ClearAllAsync();
}

public class IngredientStoreRepository(IStorageProvider storageProvider) : IIngredientStoreRepository
{
    private const string KeyPrefix = "ingredient_";

    public async Task SaveAsync(Ingredient ingredient)
    {
        string key = $"{KeyPrefix}{ingredient.Id}";
        await storageProvider.SaveItemAsync(key, ingredient);
    }

    public async Task<List<Ingredient>> LoadAllAsync() => await storageProvider.GetAllItemsAsync<Ingredient>();

    public async Task DeleteAsync(Ingredient item)
    {
        string key = $"{KeyPrefix}{item.Id}";
        await storageProvider.DeleteItemAsync(key);
    }

    public async Task ClearAllAsync() => await storageProvider.ClearAllAsync();
}

public class IngredientStore(IIngredientStoreRepository ingredientStoreRepository) : IIngredientStoreService
{
    public async Task AddIngredientAsync(Ingredient item)
    {
        if (item == null) return;
        await ingredientStoreRepository.SaveAsync(item);
    }

    public async Task ModifyIngredientAsync(Ingredient item)
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
