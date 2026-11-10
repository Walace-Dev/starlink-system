using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Starlink.Api.Data;

#nullable disable

namespace Starlink.Api.Migrations;

/// <summary>
/// Integridade adicional no SQLite, inclusive para operações fora da API.
/// Como os gatilhos não alteram o modelo EF, o snapshot existente permanece válido.
/// </summary>
[DbContext(typeof(StarlinkDbContext))]
[Migration("20261110203000_AddBancoIntegrityGuards")]
public sealed class AddBancoIntegrityGuards : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            CREATE TRIGGER "TR_Vendas_ValidarInsert"
            BEFORE INSERT ON "Vendas"
            BEGIN
                SELECT RAISE(ABORT, 'Valor da venda deve ser maior que zero')
                WHERE NEW."Valor" <= 0;

                SELECT RAISE(ABORT, 'Data final anterior a data inicial')
                WHERE NEW."DataFimAluguel" IS NOT NULL
                  AND NEW."DataFimAluguel" < NEW."DataInicioAluguel";

                SELECT RAISE(ABORT, 'Cliente nao pertence ao dono da venda')
                WHERE NOT EXISTS (
                    SELECT 1 FROM "Clientes"
                    WHERE "Id" = NEW."ClienteId"
                      AND "DonoId" = NEW."DonoId"
                );
            END;
            """);

        migrationBuilder.Sql("""
            CREATE TRIGGER "TR_Vendas_ValidarUpdate"
            BEFORE UPDATE ON "Vendas"
            BEGIN
                SELECT RAISE(ABORT, 'Valor da venda deve ser maior que zero')
                WHERE NEW."Valor" <= 0;

                SELECT RAISE(ABORT, 'Data final anterior a data inicial')
                WHERE NEW."DataFimAluguel" IS NOT NULL
                  AND NEW."DataFimAluguel" < NEW."DataInicioAluguel";

                SELECT RAISE(ABORT, 'Cliente nao pertence ao dono da venda')
                WHERE NOT EXISTS (
                    SELECT 1 FROM "Clientes"
                    WHERE "Id" = NEW."ClienteId"
                      AND "DonoId" = NEW."DonoId"
                );
            END;
            """);

        migrationBuilder.Sql("""
            CREATE TRIGGER "TR_Clientes_BloquearTrocaDeDono"
            BEFORE UPDATE OF "DonoId" ON "Clientes"
            WHEN NEW."DonoId" <> OLD."DonoId"
            BEGIN
                SELECT RAISE(ABORT, 'Nao e permitido transferir cliente entre donos');
            END;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP TRIGGER IF EXISTS \"TR_Clientes_BloquearTrocaDeDono\";");
        migrationBuilder.Sql("DROP TRIGGER IF EXISTS \"TR_Vendas_ValidarUpdate\";");
        migrationBuilder.Sql("DROP TRIGGER IF EXISTS \"TR_Vendas_ValidarInsert\";");
    }
}
