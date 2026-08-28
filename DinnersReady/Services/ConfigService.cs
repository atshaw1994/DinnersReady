using System;
using System.IO;
using System.Text.Json;
using Avalonia.Platform;

namespace DinnersReady.Services;

public static class ConfigService
{
    public static string GetGeminiApiKey()
    {
        try
        {
            // Load appsettings.json from the embedded Avalonia assembly resources
            var uri = new Uri("avares://DinnersReady/appsettings.json");

            if (AssetLoader.Exists(uri))
            {
                using var stream = AssetLoader.Open(uri);
                using var doc = JsonDocument.Parse(stream);

                if (doc.RootElement.TryGetProperty("GeminiApiKey", out var keyElement))
                {
                    return keyElement.GetString() ?? string.Empty;
                }
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"[ConfigService] Error reading appsettings.json: {ex.Message}");
        }

        return string.Empty;
    }
}