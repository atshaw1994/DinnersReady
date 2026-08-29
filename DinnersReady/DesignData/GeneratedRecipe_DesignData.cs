using DinnersReady.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DinnersReady.DesignData;

public static class GeneratedRecipe_DesignData
{
    public static GeneratedRecipe CheesyOmelette { get; } = new GeneratedRecipe
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
