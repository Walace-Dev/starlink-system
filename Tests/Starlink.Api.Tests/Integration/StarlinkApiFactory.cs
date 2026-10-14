
using System.Data.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Starlink.Api.Data;

namespace Starlink.Api.Tests.Integration;

public sealed class StarlinkApiFactory
    : WebApplicationFactory<Program>
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        // Configuracao JWT exclusiva do ambiente de testes.
        // Esta chave NAO deve ser utilizada em producao.
        builder.ConfigureHostConfiguration(config =>
        {
            config.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["Jwt:Key"] =
                        "STARLINK_TEST_ONLY_KEY_0123456789_ABCDEFGHIJKLMNOPQRSTUVWXYZ"
                });
        });

        return base.CreateHost(builder);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Ambiente dedicado aos testes automatizados.
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            // Remove as configuracoes anteriores do DbContext.
            services.RemoveAll<
                DbContextOptions<StarlinkDbContext>>();

            services.RemoveAll<
                IDbContextOptionsConfiguration<StarlinkDbContext>>();

            // SQLite temporario em memoria RAM.
            services.AddSingleton<DbConnection>(_ =>
            {
                var connection = new SqliteConnection(
                    "Data Source=:memory:");

                connection.Open();

                return connection;
            });

            // Registra novamente o contexto utilizando
            // a conexao temporaria dos testes.
            services.AddDbContext<StarlinkDbContext>(
                (provider, options) =>
                {
                    var connection = provider
                        .GetRequiredService<DbConnection>();

                    options.UseSqlite(connection);
                });
        });
    }

    public void CreateDatabase()
    {
        using var scope = Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<StarlinkDbContext>();

        var connection = db.Database.GetDbConnection();

        // Protecao: impede a criacao do esquema
        // em uma conexao SQLite persistente.
        if (!connection.ConnectionString.Contains(
                ":memory:",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Os testes devem utilizar SQLite em memoria.");
        }

        // Cria as tabelas no banco temporario.
        db.Database.EnsureCreated();
    }
}
