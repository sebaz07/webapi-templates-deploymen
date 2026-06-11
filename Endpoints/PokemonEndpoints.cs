using System.Text.Json;

namespace webapi.Endpoints;

public static class PokemonEndpoints
{
    private const int TotalPokemon = 1025;

    public static void MapPokemonEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/pokemon").WithTags("Pokemon");

        group.MapGet("/random", async (IHttpClientFactory httpFactory) =>
        {
            var id = Random.Shared.Next(1, TotalPokemon + 1);
            return await FetchPokemon(id.ToString(), httpFactory);
        })
        .WithName("GetRandomPokemon")
        .WithSummary("Obtiene un Pokémon aleatorio");

        group.MapGet("/{nameOrId}", async (string nameOrId, IHttpClientFactory httpFactory) =>
            await FetchPokemon(nameOrId.ToLowerInvariant(), httpFactory))
        .WithName("GetPokemon")
        .WithSummary("Obtiene un Pokémon por nombre o ID");
    }

    private static async Task<IResult> FetchPokemon(string nameOrId, IHttpClientFactory httpFactory)
    {
        var http = httpFactory.CreateClient("pokeapi");
        try
        {
            var json = await http.GetStringAsync($"pokemon/{nameOrId}");
            var p = JsonDocument.Parse(json).RootElement;

            var speciesUrl = p.GetProperty("species").GetProperty("url").GetString()!;
            string? flavorText = null;
            try
            {
                var speciesJson = await http.GetStringAsync(speciesUrl);
                var species = JsonDocument.Parse(speciesJson).RootElement;
                flavorText = species.GetProperty("flavor_text_entries").EnumerateArray()
                    .FirstOrDefault(e => e.GetProperty("language").GetProperty("name").GetString() == "en")
                    .GetProperty("flavor_text").GetString()?
                    .Replace("\n", " ").Replace("\f", " ");
            }
            catch { }

            return Results.Ok(new
            {
                id = p.GetProperty("id").GetInt32(),
                name = p.GetProperty("name").GetString(),
                description = flavorText,
                height = $"{p.GetProperty("height").GetInt32() / 10.0} m",
                weight = $"{p.GetProperty("weight").GetInt32() / 10.0} kg",
                types = p.GetProperty("types").EnumerateArray()
                    .OrderBy(t => t.GetProperty("slot").GetInt32())
                    .Select(t => t.GetProperty("type").GetProperty("name").GetString())
                    .ToList(),
                abilities = p.GetProperty("abilities").EnumerateArray()
                    .Select(a => new
                    {
                        name = a.GetProperty("ability").GetProperty("name").GetString(),
                        hidden = a.GetProperty("is_hidden").GetBoolean()
                    })
                    .ToList(),
                stats = p.GetProperty("stats").EnumerateArray()
                    .Select(s => new
                    {
                        stat = s.GetProperty("stat").GetProperty("name").GetString(),
                        value = s.GetProperty("base_stat").GetInt32()
                    })
                    .ToList(),
                sprites = new
                {
                    front = p.GetProperty("sprites").GetProperty("front_default").GetString(),
                    frontShiny = p.GetProperty("sprites").GetProperty("front_shiny").GetString()
                }
            });
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return Results.NotFound(new { message = $"Pokémon '{nameOrId}' no encontrado" });
        }
    }
}
