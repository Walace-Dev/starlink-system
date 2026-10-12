using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Starlink.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddAutenticacaoDono : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Clientes_Documento",
                table: "Clientes");

            migrationBuilder.AddColumn<int>(
                name: "DonoId",
                table: "Vendas",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DonoId",
                table: "Clientes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Donos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    SenhaHash = table.Column<string>(type: "TEXT", nullable: false),
                    DataCadastro = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Donos", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Vendas_DonoId",
                table: "Vendas",
                column: "DonoId");

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_DonoId_Documento",
                table: "Clientes",
                columns: new[] { "DonoId", "Documento" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Donos_Email",
                table: "Donos",
                column: "Email",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Clientes_Donos_DonoId",
                table: "Clientes",
                column: "DonoId",
                principalTable: "Donos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Vendas_Donos_DonoId",
                table: "Vendas",
                column: "DonoId",
                principalTable: "Donos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clientes_Donos_DonoId",
                table: "Clientes");

            migrationBuilder.DropForeignKey(
                name: "FK_Vendas_Donos_DonoId",
                table: "Vendas");

            migrationBuilder.DropTable(
                name: "Donos");

            migrationBuilder.DropIndex(
                name: "IX_Vendas_DonoId",
                table: "Vendas");

            migrationBuilder.DropIndex(
                name: "IX_Clientes_DonoId_Documento",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "DonoId",
                table: "Vendas");

            migrationBuilder.DropColumn(
                name: "DonoId",
                table: "Clientes");

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_Documento",
                table: "Clientes",
                column: "Documento",
                unique: true);
        }
    }
}
