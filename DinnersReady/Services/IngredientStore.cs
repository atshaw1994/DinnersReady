using DinnersReady.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DinnersReady.Services
{
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
}
