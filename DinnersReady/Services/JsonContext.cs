using DinnersReady.Models;
using Microsoft.Extensions.AI;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DinnersReady;

// Request DTOs for Gemini API
public class GeminiPart { public string Text { get; set; } = string.Empty; }
public class GeminiContent { public List<GeminiPart> Parts { get; set; } = []; }
public class GeminiRequest { public List<GeminiContent> Contents { get; set; } = []; }

[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNameCaseInsensitive = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(Recipe))]
[JsonSerializable(typeof(Ingredient))]
[JsonSerializable(typeof(List<Recipe>))]
[JsonSerializable(typeof(List<Ingredient>))]
[JsonSerializable(typeof(List<string>))]
[JsonSerializable(typeof(GeminiRequest))]
public partial class DinnersReadyJsonContext : JsonSerializerContext
{
}