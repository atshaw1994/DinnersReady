using DinnersReady.Models;
using Microsoft.Extensions.AI;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace DinnersReady.Services;

public class RecipeGeneratorService(IChatClient chatClient)
{
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
          "instructions": ["1...", "2..."]
        }
        Do not include markdown code block formatting (like ```json). Return raw JSON only.
        """;

        var userPrompt = $"Available ingredients: {ingredientsText}";

        var options = new ChatOptions
        {
            ResponseFormat = ChatResponseFormat.Json
        };

        ChatResponse response = await chatClient.GetResponseAsync(
        [
            new(ChatRole.System, systemPrompt),
            new(ChatRole.User, userPrompt)
        ], options, cancellationToken: ct);

        var cleanJson = response.Text?.Trim();
        if (string.IsNullOrWhiteSpace(cleanJson))
            return null;

        if (cleanJson.StartsWith("```json"))
        {
            cleanJson = cleanJson.Replace("```json", "").Replace("```", "").Trim();
        }

        try
        {
            return JsonSerializer.Deserialize(cleanJson, DinnersReadyJsonContext.Default.Recipe);
        }
        catch (JsonException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[RecipeGeneratorService] Failed to parse AI JSON response: {ex.Message}");
            return null;
        }
    }
}