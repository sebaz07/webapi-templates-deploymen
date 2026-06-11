using System.Text.Json;

namespace webapi.Endpoints;

public static class ClientEndpoints
{
    public static void MapClientEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/client").WithTags("Client");

        group.MapGet("/info", async (HttpContext ctx, IHttpClientFactory httpFactory) =>
        {
            var ip = ResolveIp(ctx);

            JsonElement? geo = null;
            try
            {
                if (!string.IsNullOrEmpty(ip) && !ip.StartsWith("127.") && !ip.StartsWith("::1") && ip != "unknown")
                {
                    var http = httpFactory.CreateClient("ipapi");
                    var raw = await http.GetStringAsync($"json/{ip}");
                    geo = JsonDocument.Parse(raw).RootElement.Clone();
                }
            }
            catch { }

            return Results.Ok(new
            {
                ip,
                userAgent = ctx.Request.Headers.UserAgent.ToString(),
                requestTimeUtc = DateTime.UtcNow,
                method = ctx.Request.Method,
                protocol = ctx.Request.Protocol,
                host = ctx.Request.Host.ToString(),
                path = ctx.Request.Path.Value,
                headers = ctx.Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
                geoLocation = geo
            });
        })
        .WithName("GetClientInfo")
        .WithSummary("IP, headers, user-agent y geolocalización del cliente");
    }

    private static string ResolveIp(HttpContext ctx)
    {
        var forwarded = ctx.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwarded))
            return forwarded.Split(',')[0].Trim();

        var realIp = ctx.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
            return realIp;

        return ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
