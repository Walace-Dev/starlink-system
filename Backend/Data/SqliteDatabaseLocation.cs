using Microsoft.Data.Sqlite;

namespace Starlink.Api.Data;

/// <summary>
/// Resolve caminhos SQLite relativos a partir da pasta do backend.
/// Caminhos absolutos são preservados para permitir volumes persistentes.
/// </summary>
public static class SqliteDatabaseLocation
{
    public static string Resolve(string connectionString, string baseDirectory)
    {
        var settings = new SqliteConnectionStringBuilder(connectionString);

        if (string.IsNullOrWhiteSpace(settings.DataSource))
            throw new InvalidOperationException("A configuração SQLite precisa de Data Source.");

        var source = settings.DataSource;
        if (source != ":memory:" &&
            !source.StartsWith("file:", StringComparison.OrdinalIgnoreCase) &&
            !Path.IsPathFullyQualified(source))
        {
            settings.DataSource = Path.GetFullPath(Path.Combine(baseDirectory, source));
        }

        return settings.ToString();
    }

    public static string BackendDirectory(string currentDirectory)
    {
        var candidate = Path.Combine(currentDirectory, "Backend");

        if (File.Exists(Path.Combine(candidate, "Starlink.Api.csproj")))
            return candidate;

        // Na pasta Backend ou em uma publicação, usa o diretório fornecido.
        return currentDirectory;
    }
}
