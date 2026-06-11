using System.Text.Json;

namespace webapi.Endpoints;

public static class RickAndMortyEndpoints
{
    private const int TotalCharacters = 826;
    private const int TotalEpisodes = 51;
    private const int TotalLocations = 126;

    public static void MapRickAndMortyEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/rickandmorty").WithTags("Rick & Morty");

        group.MapGet("/characters/random", async (IHttpClientFactory httpFactory) =>
        {
            var id = Random.Shared.Next(1, TotalCharacters + 1);
            return await FetchCharacter(id, httpFactory);
        })
        .WithName("GetRandomCharacter")
        .WithSummary("Personaje aleatorio de Rick and Morty");

        group.MapGet("/characters/{id:int}", async (int id, IHttpClientFactory httpFactory) =>
            await FetchCharacter(id, httpFactory))
        .WithName("GetCharacter")
        .WithSummary("Personaje de Rick and Morty por ID");

        group.MapGet("/episodes/random", async (IHttpClientFactory httpFactory) =>
        {
            var id = Random.Shared.Next(1, TotalEpisodes + 1);
            var http = httpFactory.CreateClient("rickandmorty");
            try
            {
                var json = await http.GetStringAsync($"episode/{id}");
                var ep = JsonDocument.Parse(json).RootElement;
                return Results.Ok(new
                {
                    id = ep.GetProperty("id").GetInt32(),
                    name = ep.GetProperty("name").GetString(),
                    airDate = ep.GetProperty("air_date").GetString(),
                    episode = ep.GetProperty("episode").GetString(),
                    characterCount = ep.GetProperty("characters").GetArrayLength(),
                    url = ep.GetProperty("url").GetString()
                });
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return Results.NotFound(new { message = $"Episode {id} not found" });
            }
        })
        .WithName("GetRandomEpisode")
        .WithSummary("Episodio aleatorio de Rick and Morty");

        group.MapGet("/locations/random", async (IHttpClientFactory httpFactory) =>
        {
            var id = Random.Shared.Next(1, TotalLocations + 1);
            var http = httpFactory.CreateClient("rickandmorty");
            try
            {
                var json = await http.GetStringAsync($"location/{id}");
                var loc = JsonDocument.Parse(json).RootElement;
                return Results.Ok(new
                {
                    id = loc.GetProperty("id").GetInt32(),
                    name = loc.GetProperty("name").GetString(),
                    type = loc.GetProperty("type").GetString(),
                    dimension = loc.GetProperty("dimension").GetString(),
                    residentCount = loc.GetProperty("residents").GetArrayLength()
                });
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return Results.NotFound(new { message = $"Location {id} not found" });
            }
        })
        .WithName("GetRandomLocation")
        .WithSummary("Locación aleatoria de Rick and Morty");
    }

    private static async Task<IResult> FetchCharacter(int id, IHttpClientFactory httpFactory)
    {
        var http = httpFactory.CreateClient("rickandmorty");
        try
        {
            var json = await http.GetStringAsync($"character/{id}");
            var c = JsonDocument.Parse(json).RootElement;
            return Results.Ok(new
            {
                id = c.GetProperty("id").GetInt32(),
                name = c.GetProperty("name").GetString(),
                status = c.GetProperty("status").GetString(),
                species = c.GetProperty("species").GetString(),
                type = c.GetProperty("type").GetString(),
                gender = c.GetProperty("gender").GetString(),
                origin = c.GetProperty("origin").GetProperty("name").GetString(),
                location = c.GetProperty("location").GetProperty("name").GetString(),
                image = c.GetProperty("image").GetString(),
                episodeCount = c.GetProperty("episode").GetArrayLength()
            });
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return Results.NotFound(new { message = $"Character {id} not found" });
        }
    }
}
