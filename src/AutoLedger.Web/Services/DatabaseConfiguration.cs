namespace AutoLedger.Web.Services;

/// <summary>
/// Resolves the PostgreSQL connection string from configuration, supporting both a normal
/// ConnectionStrings:Default value (local/Docker Compose) and the <c>DATABASE_URL</c> URI that
/// managed hosts like Fly.io inject (postgres://user:pass@host:port/db).
/// </summary>
public static class DatabaseConfiguration
{
    public static string ResolveConnectionString(IConfiguration configuration)
    {
        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
        if (!string.IsNullOrWhiteSpace(databaseUrl))
            return FromUri(databaseUrl);

        var configured = configuration.GetConnectionString("Default");
        if (!string.IsNullOrWhiteSpace(configured))
            return configured;

        throw new InvalidOperationException(
            "No database connection configured. Set ConnectionStrings:Default or DATABASE_URL.");
    }

    private static string FromUri(string databaseUrl)
    {
        var uri = new Uri(databaseUrl);
        var userInfo = uri.UserInfo.Split(':', 2);

        // Honour the URL's sslmode (Fly's internal flycast Postgres uses sslmode=disable and
        // closes SSL handshakes — forcing SSL there crashes the app). Default to Prefer.
        var sslMode = MapSslMode(ReadQueryValue(uri.Query, "sslmode"));

        var parts = new List<string>
        {
            $"Host={uri.Host}",
            $"Port={(uri.Port > 0 ? uri.Port : 5432)}",
            $"Database={uri.AbsolutePath.TrimStart('/')}",
            $"Username={Uri.UnescapeDataString(userInfo[0])}",
            $"Password={Uri.UnescapeDataString(userInfo.Length > 1 ? userInfo[1] : string.Empty)}",
            $"SSL Mode={sslMode}"
        };
        if (sslMode != "Disable")
            parts.Add("Trust Server Certificate=true");

        return string.Join(';', parts);
    }

    private static string? ReadQueryValue(string query, string key)
    {
        // query looks like "?sslmode=disable&foo=bar"
        foreach (var pair in query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var kv = pair.Split('=', 2);
            if (kv.Length == 2 && kv[0].Equals(key, StringComparison.OrdinalIgnoreCase))
                return Uri.UnescapeDataString(kv[1]);
        }
        return null;
    }

    private static string MapSslMode(string? sslmode) => sslmode?.ToLowerInvariant() switch
    {
        "disable" => "Disable",
        "allow" => "Allow",
        "require" => "Require",
        "verify-ca" => "VerifyCA",
        "verify-full" => "VerifyFull",
        _ => "Prefer"
    };
}
