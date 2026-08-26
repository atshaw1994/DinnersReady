using DinnersReady.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DinnersReady.Services
{
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
}
