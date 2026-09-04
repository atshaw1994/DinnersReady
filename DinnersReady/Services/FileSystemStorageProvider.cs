using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace DinnersReady.Services;

public class FileSystemStorageProvider : IStorageProvider
{
    private readonly string _storageFolder;

    public FileSystemStorageProvider()
    {
        _storageFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "DinnersReady",
            "Storage"
        );

        if (!Directory.Exists(_storageFolder))
        {
            Directory.CreateDirectory(_storageFolder);
        }
    }

    private string GetFilePath(string key)
    {
        char[] invalidChars = Path.GetInvalidFileNameChars();
        char[] safeChars = key.Select(c => invalidChars.Contains(c) ? '_' : c).ToArray();
        return Path.Combine(_storageFolder, $"{new string(safeChars)}.json");
    }

    public async Task SaveItemAsync<T>(string key, T item)
    {
        string filePath = GetFilePath(key);
        var typeInfo = DinnersReadyJsonContext.Default.GetTypeInfo(typeof(T))!;
        string json = JsonSerializer.Serialize(item, typeInfo);
        await File.WriteAllTextAsync(filePath, json);
    }

    public async Task<T?> GetItemAsync<T>(string key)
    {
        string filePath = GetFilePath(key);
        if (!File.Exists(filePath)) return default;

        string json = await File.ReadAllTextAsync(filePath);
        var typeInfo = DinnersReadyJsonContext.Default.GetTypeInfo(typeof(T))!;
        return (T?)JsonSerializer.Deserialize(json, typeInfo);
    }

    public async Task<List<T>> GetAllItemsAsync<T>()
    {
        var items = new List<T>();
        if (!Directory.Exists(_storageFolder)) return items;

        var typeInfo = DinnersReadyJsonContext.Default.GetTypeInfo(typeof(T))!;
        string[] files = Directory.GetFiles(_storageFolder, "*.json");

        foreach (string file in files)
        {
            try
            {
                string json = await File.ReadAllTextAsync(file);
                var item = JsonSerializer.Deserialize(json, typeInfo);
                if (item is T typedItem)
                {
                    items.Add(typedItem);
                }
            }
            catch (JsonException)
            {
                // Skip corrupted files
            }
        }

        return items;
    }

    public async Task DeleteItemAsync(string key)
    {
        string filePath = GetFilePath(key);
        if (File.Exists(filePath))
        {
            await Task.Run(() => File.Delete(filePath));
        }
    }

    public async Task ClearAllAsync()
    {
        if (Directory.Exists(_storageFolder))
        {
            await Task.Run(() =>
            {
                var directoryInfo = new DirectoryInfo(_storageFolder);
                foreach (FileInfo file in directoryInfo.GetFiles())
                {
                    file.Delete();
                }
            });
        }
    }
}