using DinnersReady.Models;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DinnersReady
{
    [JsonSerializable(typeof(Ingredient))]
    [JsonSerializable(typeof(List<Ingredient>))]
    [JsonSerializable(typeof(Recipe))]
    [JsonSerializable(typeof(List<Recipe>))]
    [JsonSerializable(typeof(Dictionary<string, double>))]
    public partial class DinnersReadyJsonContext : JsonSerializerContext;
}