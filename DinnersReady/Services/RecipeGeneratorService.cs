using DinnersReady.Models;
using Microsoft.Extensions.AI;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace DinnersReady.Services;

public class RecipeGeneratorService(IChatClient chatClient)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<Recipe?> GenerateRecipeAsync(IEnumerable<string> availableIngredients, CancellationToken ct = default)
    {
        var ingredientsText = string.Join(", ", availableIngredients);

        var systemPrompt = """
            You are an expert chef assistant. 
            Generate a recipe based primarily on the available ingredients provided by the user. 
            Respond ONLY with a valid JSON object matching this structure:
            {
              "title": "Recipe Name",
              "description": "Short summary",
              "prepTimeMinutes": 10,
              "cookTimeMinutes": 20,
              "usedIngredients": ["Item 1", "Item 2"],
              "additionalIngredientsNeeded": ["Optional Item"],
              "instructions": ["Step 1...", "Step 2..."]
            }
            Do not include markdown code block formatting (like ```json). Return raw JSON only.
            """;

        var userPrompt = $"Available ingredients: {ingredientsText}";

        ChatResponse response = await chatClient.GetResponseAsync(
        [
            new(ChatRole.System, systemPrompt),
            new(ChatRole.User, userPrompt)
        ], cancellationToken: ct);

        var cleanJson = response.Text?.Trim();
        if (string.IsNullOrWhiteSpace(cleanJson))
            return null;

        return JsonSerializer.Deserialize<Recipe>(cleanJson, JsonOptions);
    }
}