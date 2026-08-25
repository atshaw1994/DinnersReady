# DinnersReady

DinnersReady is a cross-platform kitchen inventory app built with [Avalonia UI](https://avaloniaui.net/) and .NET, designed to help you track pantry, fridge, and freezer ingredients so you always know what's on hand for your next meal.

## Features

- **Ingredient tracking** — Add ingredients with category, storage location, unit of measure, quantity, and expiry date.
- **Ingredient library** — A built-in library of common ingredients (`IngredientsLibrary.json`) provides autocomplete suggestions and sensible defaults (default location, unit, and typical shelf life).
- **MVVM architecture** — Built using the [CommunityToolkit.Mvvm](https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/) source generators for clean, observable view models and commands.
- **Cross-platform** — Runs on Desktop (Windows/macOS/Linux), Android, iOS, and Browser (WebAssembly) from a single shared codebase.

## Project Structure

| Project | Description |
|---|---|
| `DinnersReady` | Shared Avalonia application project containing views, view models, models, and assets. |
| `DinnersReady.Desktop` | Desktop app head (Windows/macOS/Linux). |
| `DinnersReady.Android` | Android app head. |
| `DinnersReady.iOS` | iOS app head. |
| `DinnersReady.Browser` | Browser/WebAssembly app head. |

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Visual Studio 2026 (with the Avalonia and mobile/WASM workloads) or the [Avalonia for VS Code](https://marketplace.visualstudio.com/items?itemName=AvaloniaTeam.vscode-avalonia) extension

### Build and Run

Open `DinnersReady.slnx` in Visual Studio and select a startup project (e.g. `DinnersReady.Desktop`), or run from the command line:

```powershell
dotnet run --project DinnersReady.Desktop\DinnersReady.Desktop.csproj
```

To run on other platforms, substitute the corresponding head project (`DinnersReady.Android`, `DinnersReady.iOS`, or `DinnersReady.Browser`).

## Contributing

Issues and pull requests are welcome!

## License

This project currently has no license specified.