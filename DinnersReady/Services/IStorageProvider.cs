using System.Collections.Generic;
using System.Threading.Tasks;

namespace DinnersReady.Services;

public interface IStorageProvider
{
    Task SaveItemAsync<T>(string key, T item);
    Task<T?> GetItemAsync<T>(string key);
    Task<List<T>> GetAllItemsAsync<T>();
    Task DeleteItemAsync(string key);
    Task ClearAllAsync();
}