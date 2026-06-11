using System.Text.Json;

namespace webapi.Endpoints;

public static class FunEndpoints
{
    public static void MapFunEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/fun").WithTags("Fun");

        group.MapGet("/joke", async (IHttpClientFactory httpFactory) =>
        {
            var http = httpFactory.CreateClient("dadjokes");
            http.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json");
            var json = await http.GetStringAsync("random");
            var doc = JsonDocument.Parse(json).RootElement;
            return Results.Ok(new
            {
                id = doc.GetProperty("id").GetString(),
                joke = doc.GetProperty("joke").GetString(),
                status = doc.GetProperty("status").GetInt32()
            });
        })
        .WithName("GetRandomJoke")
        .WithSummary("Chiste aleatorio (dad joke)");

        group.MapGet("/advice", async (IHttpClientFactory httpFactory) =>
        {
            var http = httpFactory.CreateClient("adviceslip");
            var json = await http.GetStringAsync("advice");
            var doc = JsonDocument.Parse(json).RootElement;
            var slip = doc.GetProperty("slip");
            return Results.Ok(new
            {
                id = slip.GetProperty("id").GetInt32(),
                advice = slip.GetProperty("advice").GetString()
            });
        })
        .WithName("GetRandomAdvice")
        .WithSummary("Consejo aleatorio");

        group.MapGet("/number/{n:int}", async (int n, IHttpClientFactory httpFactory) =>
        {
            var http = httpFactory.CreateClient("numbersapi");
            var fact = await http.GetStringAsync($"{n}/trivia?json");
            try
            {
                var doc = JsonDocument.Parse(fact).RootElement;
                return Results.Ok(new
                {
                    number = n,
                    text = doc.GetProperty("text").GetString(),
                    found = doc.GetProperty("found").GetBoolean(),
                    type = doc.GetProperty("type").GetString()
                });
            }
            catch
            {
                return Results.Ok(new { number = n, text = fact });
            }
        })
        .WithName("GetNumberFact")
        .WithSummary("Dato curioso sobre un número");

        group.MapGet("/number/random", async (IHttpClientFactory httpFactory) =>
        {
            var n = Random.Shared.Next(1, 10000);
            var http = httpFactory.CreateClient("numbersapi");
            var fact = await http.GetStringAsync($"{n}/trivia?json");
            try
            {
                var doc = JsonDocument.Parse(fact).RootElement;
                return Results.Ok(new
                {
                    number = n,
                    text = doc.GetProperty("text").GetString(),
                    found = doc.GetProperty("found").GetBoolean(),
                    type = doc.GetProperty("type").GetString()
                });
            }
            catch
            {
                return Results.Ok(new { number = n, text = fact });
            }
        })
        .WithName("GetRandomNumberFact")
        .WithSummary("Dato curioso sobre un número aleatorio");
    }
}
