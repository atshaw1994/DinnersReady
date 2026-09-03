using DinnersReady.Models;
using DinnersReady.Services;
using Microsoft.Extensions.AI;
using Moq;

namespace DinnersReady.Tests;

[Trait("Category", "Services")]
public class ServicesTests
{
    #region IngredientStore Tests

    [Fact]
    public async Task IngredientStore_AddIngredientAsync_CallsRepositorySave()
    {
        // Arrange
        var mockRepo = new Mock<IIngredientStoreRepository>();
        var store = new IngredientStore(mockRepo.Object);
        var ingredient = new Ingredient { Id = "ing-1", Name = "Salt", Location = StorageLocation.Pantry };

        // Act
        await store.AddIngredientAsync(ingredient);

        // Assert
        mockRepo.Verify(r => r.SaveAsync(ingredient), Times.Once);
    }

    [Fact]
    public async Task IngredientStore_RemoveIngredientAsync_CallsRepositoryDelete()
    {
        // Arrange
        var mockRepo = new Mock<IIngredientStoreRepository>();
        var store = new IngredientStore(mockRepo.Object);
        var ingredient = new Ingredient { Id = "ing-1", Name = "Pepper" };

        // Act
        await store.RemoveIngredientAsync(ingredient);

        // Assert
        mockRepo.Verify(r => r.DeleteAsync(ingredient), Times.Once);
    }

    [Fact]
    public async Task IngredientStore_GetIngredientsAsync_ReturnsLoadedItems()
    {
        // Arrange
        var mockRepo = new Mock<IIngredientStoreRepository>();
        var items = new List<Ingredient>
        {
            new() { Id = "1", Name = "Flour" },
            new() { Id = "2", Name = "Sugar" }
        };
        mockRepo.Setup(r => r.LoadAllAsync()).ReturnsAsync(items);

        var store = new IngredientStore(mockRepo.Object);

        // Act
        var result = await store.GetIngredientsAsync();

        // Assert
        Assert.Equal(2, result.Count());
        mockRepo.Verify(r => r.LoadAllAsync(), Times.Once);
    }

    #endregion

    #region RecipeStore Tests

    [Fact]
    public async Task RecipeStore_AddRecipeAsync_CallsRepositorySave()
    {
        // Arrange
        var mockRepo = new Mock<IRecipeStoreRepository>();
        var store = new RecipeStore(mockRepo.Object);
        var recipe = new Recipe { Id = "rec-1", Title = "Pancakes" };

        // Act
        await store.AddRecipeAsync(recipe);

        // Assert
        mockRepo.Verify(r => r.SaveAsync(recipe), Times.Once);
    }

    [Fact]
    public async Task RecipeStore_RemoveRecipeAsync_CallsRepositoryDelete()
    {
        // Arrange
        var mockRepo = new Mock<IRecipeStoreRepository>();
        var store = new RecipeStore(mockRepo.Object);
        var recipe = new Recipe { Id = "rec-1", Title = "Pancakes" };

        // Act
        await store.RemoveRecipeAsync(recipe);

        // Assert
        mockRepo.Verify(r => r.DeleteAsync(recipe.Id), Times.Once);
    }

    #endregion

    #region RecipeGeneratorService Tests

    [Fact]
    public async Task RecipeGeneratorService_GenerateRecipeAsync_ParsesValidJson()
    {
        // Arrange
        var mockChatClient = new Mock<IChatClient>();
        string fakeJsonResponse = """
        {
            "title": "Omelette",
            "description": "Quick breakfast",
            "prepTimeMinutes": 5,
            "cookTimeMinutes": 5,
            "usedIngredients": ["Eggs", "Butter"],
            "additionalIngredientsNeeded": [],
            "instructions": ["1. Beat eggs.", "2. Cook in pan."]
        }
        """;

        mockChatClient
            .Setup(c => c.GetResponseAsync(
                It.IsAny<IEnumerable<ChatMessage>>(),
                It.IsAny<ChatOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChatResponse(new ChatMessage(ChatRole.Assistant, fakeJsonResponse)));

        var service = new RecipeGeneratorService(mockChatClient.Object);

        // Act
        var result = await service.GenerateRecipeAsync(["Eggs", "Butter"], TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Omelette", result.Title);
        Assert.Equal(2, result.UsedIngredients.Count);
    }

    [Fact]
    public async Task RecipeGeneratorService_GenerateRecipeAsync_ReturnsNullOnEmptyResponse()
    {
        // Arrange
        var mockChatClient = new Mock<IChatClient>();
        mockChatClient
            .Setup(c => c.GetResponseAsync(
                It.IsAny<IEnumerable<ChatMessage>>(),
                It.IsAny<ChatOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChatResponse(new ChatMessage(ChatRole.Assistant, "")));

        var service = new RecipeGeneratorService(mockChatClient.Object);

        // Act
        var result = await service.GenerateRecipeAsync(["Eggs"], TestContext.Current.CancellationToken);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region SharingService Tests

    [Fact]
    public async Task DesktopFallbackShareService_ExecutesWithoutThrowing()
    {
        // Arrange
        var shareService = new DesktopFallbackShareService();

        // Act & Assert
        var exception = await Record.ExceptionAsync(() => shareService.ShareTextAsync("Test Title", "Test Content"));
        Assert.Null(exception);
    }

    #endregion

    [Fact]
    public async Task RecipeGeneratorService_GenerateRecipeAsync_ReturnsNullOnMalformedJson()
    {
        // Arrange
        var mockChatClient = new Mock<IChatClient>();
        mockChatClient
            .Setup(c => c.GetResponseAsync(
                It.IsAny<IEnumerable<ChatMessage>>(),
                It.IsAny<ChatOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChatResponse(new ChatMessage(ChatRole.Assistant, "Sorry, I can't generate a recipe with those ingredients.")));

        var service = new RecipeGeneratorService(mockChatClient.Object);

        // Act
        var result = await service.GenerateRecipeAsync(new[] { "Rocks", "Dirt" }, TestContext.Current.CancellationToken);

        // Assert
        Assert.Null(result);
    }
}