namespace webapi.Endpoints;

public static class TimeEndpoints
{
    private static readonly string[] Zones =
    [
        "UTC",
        "America/New_York",
        "America/Chicago",
        "America/Los_Angeles",
        "America/Bogota",
        "America/Argentina/Buenos_Aires",
        "Europe/London",
        "Europe/Madrid",
        "Europe/Paris",
        "Asia/Tokyo",
        "Asia/Shanghai",
        "Australia/Sydney"
    ];

    public static void MapTimeEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/time").WithTags("Time");

        group.MapGet("/", () =>
        {
            var now = DateTime.UtcNow;

            var zones = Zones
                .Select(z =>
                {
                    try
                    {
                        var tz = TimeZoneInfo.FindSystemTimeZoneById(z);
                        var local = TimeZoneInfo.ConvertTimeFromUtc(now, tz);
                        return new
                        {
                            zone = z,
                            time = local.ToString("yyyy-MM-dd HH:mm:ss"),
                            offset = tz.GetUtcOffset(now).ToString(@"\+hh\:mm"),
                            isDst = tz.IsDaylightSavingTime(now)
                        };
                    }
                    catch { return null; }
                })
                .Where(z => z is not null)
                .ToList();

            return Results.Ok(new
            {
                utc = now.ToString("yyyy-MM-dd HH:mm:ss"),
                iso8601 = now.ToString("o"),
                unixSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                unixMilliseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                dayOfWeek = now.DayOfWeek.ToString(),
                weekOfYear = System.Globalization.ISOWeek.GetWeekOfYear(now),
                zones
            });
        })
        .WithName("GetTime")
        .WithSummary("Hora UTC actual, Unix timestamp y múltiples zonas horarias");
    }
}
