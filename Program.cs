using Scalar.AspNetCore;
using webapi.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var timeout = TimeSpan.FromSeconds(10);

builder.Services.AddHttpClient("pokeapi", c =>
{
    c.BaseAddress = new Uri("https://pokeapi.co/api/v2/");
    c.Timeout = timeout;
});

builder.Services.AddHttpClient("rickandmorty", c =>
{
    c.BaseAddress = new Uri("https://rickandmortyapi.com/api/");
    c.Timeout = timeout;
});

builder.Services.AddHttpClient("ipapi", c =>
{
    c.BaseAddress = new Uri("http://ip-api.com/");
    c.Timeout = timeout;
});

builder.Services.AddHttpClient("dadjokes", c =>
{
    c.BaseAddress = new Uri("https://icanhazdadjoke.com/");
    c.DefaultRequestHeaders.Add("Accept", "application/json");
    c.Timeout = timeout;
});

builder.Services.AddHttpClient("adviceslip", c =>
{
    c.BaseAddress = new Uri("https://api.adviceslip.com/");
    c.Timeout = timeout;
});

builder.Services.AddHttpClient("numbersapi", c =>
{
    c.BaseAddress = new Uri("http://numbersapi.com/");
    c.Timeout = timeout;
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.MapClientEndpoints();
app.MapTimeEndpoints();
app.MapPokemonEndpoints();
app.MapRickAndMortyEndpoints();
app.MapFunEndpoints();

app.Run();
