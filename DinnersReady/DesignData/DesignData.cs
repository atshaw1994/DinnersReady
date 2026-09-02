using DinnersReady.Models;
using DinnersReady.ViewModels;
using System;

namespace DinnersReady.DesignData;

public static class Recipes
{
    public static Recipe CheesyOmelette { get; } = new Recipe
    {
        Title = "Classic Cheesy Omelette",
        Description = "A quick, fluffy, three-egg omelette packed with melted cheddar and cooked to golden perfection in fresh olive oil.",
        PrepTimeMinutes = 5,
        CookTimeMinutes = 5,
        UsedIngredients =
        [
            "Eggs",
            "Cheddar Cheese",
            "Olive Oil"
        ],
        AdditionalIngredientsNeeded =
        [
            "Salt",
            "Black Pepper",
            "Fresh Chives"
        ],
        Instructions =
        [
            "1. Whisk eggs thoroughly in a bowl with a pinch of salt and black pepper.",
            "2. Heat olive oil in a non-stick skillet over medium heat.",
            "3. Pour in the egg mixture, gently lifting the cooked edges to let raw egg flow underneath.",
            "4. Once set on the bottom but slightly runny on top, sprinkle shredded cheese over one half.",
            "5. Fold the omelette in half over the cheese, cook for 30 seconds until melted, and serve hot."
        ]
    };
}

public static class Ingredients
{
    #region Raw Ingredient Models

    public static Ingredient GroundCumin { get; } = new Ingredient
    {
        Id = "cumin-ground",
        Name = "Ground Cumin",
        Category = "Spices",
        DefaultLocation = "Pantry",
        DefaultUnit = "g",
        Quantity = 50,
        Unit = "g",
        ExpiryDate = DateTimeOffset.Now.AddMonths(6),
        Location = StorageLocation.Pantry
    };

    public static Ingredient WholeMilk { get; } = new Ingredient
    {
        Id = "whole-milk",
        Name = "Whole Milk",
        Category = "Dairy",
        DefaultLocation = "Fridge",
        DefaultUnit = "L",
        Quantity = 1.5,
        Unit = "L",
        ExpiryDate = DateTimeOffset.Now.AddDays(5),
        Location = StorageLocation.Fridge
    };

    public static Ingredient Eggs { get; } = new Ingredient
    {
        Id = "fresh-eggs",
        Name = "Fresh Eggs",
        Category = "Dairy",
        DefaultLocation = "Fridge",
        DefaultUnit = "pcs",
        Quantity = 12,
        Unit = "pcs",
        ExpiryDate = DateTimeOffset.Now.AddDays(14),
        Location = StorageLocation.Fridge
    };

    public static Ingredient CheddarCheese { get; } = new Ingredient
    {
        Id = "cheddar-cheese",
        Name = "Cheddar Cheese",
        Category = "Dairy",
        DefaultLocation = "Fridge",
        DefaultUnit = "g",
        Quantity = 250,
        Unit = "g",
        ExpiryDate = DateTimeOffset.Now.AddDays(10),
        Location = StorageLocation.Fridge
    };

    public static Ingredient OliveOil { get; } = new Ingredient
    {
        Id = "olive-oil",
        Name = "Extra Virgin Olive Oil",
        Category = "Pantry Essentials",
        DefaultLocation = "Pantry",
        DefaultUnit = "ml",
        Quantity = 500,
        Unit = "ml",
        ExpiryDate = DateTimeOffset.Now.AddMonths(12),
        Location = StorageLocation.Pantry
    };

    #endregion

    #region Wrapped IngredientViewModels

    public static IngredientViewModel GroundCuminViewModel { get; } = new(GroundCumin);
    public static IngredientViewModel WholeMilkViewModel { get; } = new(WholeMilk);
    public static IngredientViewModel EggsViewModel { get; } = new(Eggs);
    public static IngredientViewModel CheddarCheeseViewModel { get; } = new(CheddarCheese);
    public static IngredientViewModel OliveOilViewModel { get; } = new(OliveOil);

    #endregion
}
