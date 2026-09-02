using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DinnersReady.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DinnersReady.ViewModels;

public partial class RecipeViewModel(
    Recipe model,
    Func<RecipeViewModel, Task>? onDeleteRequested = null,
    Func<RecipeViewModel, Task>? onShareRequested = null) : ObservableObject
{
    // Fixes potential runtime crash for XAML design tooling
    public RecipeViewModel() : this(new Recipe()) { }

    public Recipe Model { get; set; } = model ?? throw new ArgumentNullException(nameof(model));

    public Func<RecipeViewModel, Task>? OnShareRequested { get; set; } = onShareRequested;
    public Func<RecipeViewModel, Task>? OnDeleteRequested { get; set; } = onDeleteRequested;

    #region Wrapped Model Properties

    public string Id
    {
        get => Model.Id;
        set => SetProperty(Model.Id, value, Model, (m, val) => m.Id = val);
    }

    public string Title
    {
        get => Model.Title;
        set => SetProperty(Model.Title, value, Model, (m, val) => m.Title = val);
    }

    public string Description
    {
        get => Model.Description;
        set => SetProperty(Model.Description, value, Model, (m, val) => m.Description = val);
    }

    public int PrepTimeMinutes
    {
        get => Model.PrepTimeMinutes;
        set
        {
            if (SetProperty(Model.PrepTimeMinutes, value, Model, (m, val) => m.PrepTimeMinutes = val))
            {
                OnPropertyChanged(nameof(PrepTimeDisplay));
                OnPropertyChanged(nameof(TotalTimeDisplay));
            }
        }
    }

    public int CookTimeMinutes
    {
        get => Model.CookTimeMinutes;
        set
        {
            if (SetProperty(Model.CookTimeMinutes, value, Model, (m, val) => m.CookTimeMinutes = val))
            {
                OnPropertyChanged(nameof(CookTimeDisplay));
                OnPropertyChanged(nameof(TotalTimeDisplay));
            }
        }
    }

    public string PrepTimeDisplay => FormatTime(PrepTimeMinutes);

    public string CookTimeDisplay => FormatTime(CookTimeMinutes);

    public string TotalTimeDisplay => FormatTime(PrepTimeMinutes + CookTimeMinutes);

    public List<string> UsedIngredients
    {
        get => Model.UsedIngredients;
        set => SetProperty(Model.UsedIngredients, value, Model, (m, val) => m.UsedIngredients = val);
    }

    public List<string> AdditionalIngredientsNeeded
    {
        get => Model.AdditionalIngredientsNeeded;
        set => SetProperty(Model.AdditionalIngredientsNeeded, value, Model, (m, val) => m.AdditionalIngredientsNeeded = val);
    }

    public List<string> Instructions
    {
        get => Model.Instructions;
        set => SetProperty(Model.Instructions, value, Model, (m, val) => m.Instructions = val);
    }

    #endregion

    #region States

    [ObservableProperty] public partial bool IsSlidLeft { get; set; } = false;
    [ObservableProperty] public partial bool IsSlidRight { get; set; } = false;

    #endregion

    #region Commands

    [RelayCommand]
    public async Task RequestShare() => OnShareRequested?.Invoke(this);

    [RelayCommand]
    public async Task RequestDelete() => OnDeleteRequested?.Invoke(this);

    #endregion

    private static string FormatTime(int totalMinutes)
    {
        if (totalMinutes <= 0)
            return "N/A";

        int hours = totalMinutes / 60;
        int mins = totalMinutes % 60;

        if (hours == 0)
            return $"{mins} min{(mins == 1 ? "" : "s")}";

        if (mins == 0)
            return $"{hours} hr{(hours == 1 ? "" : "s")}";

        return $"{hours} hr{(hours == 1 ? "" : "s")} {mins} min{(mins == 1 ? "" : "s")}";
    }

    public string ToShareableText()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine(Title);
        sb.AppendLine($"Prep: {PrepTimeMinutes} min | Cook: {CookTimeMinutes} min");
        sb.AppendLine();

        if (UsedIngredients?.Count > 0)
        {
            sb.AppendLine("Ingredients:");
            foreach (var ing in UsedIngredients)
                sb.AppendLine($"• {ing}");
            sb.AppendLine();
        }

        if (AdditionalIngredientsNeeded?.Count > 0)
        {
            sb.AppendLine("Additional Ingredients Needed:");
            foreach (var ing in AdditionalIngredientsNeeded)
                sb.AppendLine($"• {ing}");
            sb.AppendLine();
        }

        if (Instructions?.Count > 0)
        {
            sb.AppendLine("Instructions:");
            foreach (var step in Instructions)
                sb.AppendLine(step);
        }

        return sb.ToString();
    }
}
