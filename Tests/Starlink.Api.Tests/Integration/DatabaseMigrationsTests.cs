using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Starlink.Api.Data;
using Starlink.Api.Models;
using Xunit;

namespace Starlink.Api.Tests.Integration;

public class DatabaseMigrationsTests
{
    [Fact]
    public void Migrations_CriamEsquemaERegistramHistorico()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        using var db = CreateContext(connection);
        db.Database.Migrate();

        Assert.Contains(
            "20261110203000_AddBancoIntegrityGuards",
            db.Database.GetAppliedMigrations());

        Assert.True(db.Donos.Count() == 0);
        Assert.True(db.Clientes.Count() == 0);
        Assert.True(db.Vendas.Count() == 0);
    }

    [Fact]
    public void VendaComClienteDeOutroDono_EBloqueadaPeloBanco()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        using var db = CreateContext(connection);
        db.Database.Migrate();

        var donoA = new Dono
        {
            Nome = "Proprietario A",
            Email = "banco.a@teste.local",
            SenhaHash = "hash-ficticio"
        };
        var donoB = new Dono
        {
            Nome = "Proprietario B",
            Email = "banco.b@teste.local",
            SenhaHash = "hash-ficticio"
        };

        db.Donos.AddRange(donoA, donoB);
        db.SaveChanges();

        var clienteA = new Cliente
        {
            DonoId = donoA.Id,
            Nome = "Cliente A",
            Documento = "52998224725"
        };
        db.Clientes.Add(clienteA);
        db.SaveChanges();

        db.Vendas.Add(new Venda
        {
            DonoId = donoB.Id,
            ClienteId = clienteA.Id,
            Valor = 100,
            DataInicioAluguel = DateTime.UtcNow,
            DataFimAluguel = DateTime.UtcNow.AddDays(3)
        });

        var exception = Assert.Throws<DbUpdateException>(() => db.SaveChanges());
        Assert.Contains(
            "Cliente nao pertence ao dono da venda",
            exception.InnerException?.Message);
    }

    private static StarlinkDbContext CreateContext(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<StarlinkDbContext>()
            .UseSqlite(connection)
            .Options;

        return new StarlinkDbContext(options);
    }
}
