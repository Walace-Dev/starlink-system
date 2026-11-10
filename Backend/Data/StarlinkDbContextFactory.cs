using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Starlink.Api.Data;

/// <summary>
/// Habilita migrations sem inicializar o host ASP.NET nem ler a chave JWT.
/// Execute o dotnet ef a partir da raiz da solução ou da pasta Backend.
/// </summary>
public sealed class StarlinkDbContextFactory : IDesignTimeDbContextFactory<StarlinkDbContext>
{
    public StarlinkDbContext CreateDbContext(string[] args)
    {
        var backendDirectory = SqliteDatabaseLocation.BackendDirectory(
            Directory.GetCurrentDirectory());

        var connectionString = SqliteDatabaseLocation.Resolve(
            "Data Source=starlink.db", backendDirectory);

        var options = new DbContextOptionsBuilder<StarlinkDbContext>()
            .UseSqlite(connectionString)
            .Options;

        return new StarlinkDbContext(options);
    }
}
