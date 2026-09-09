using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GastAi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddParcelamento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "GrupoParcelamentoId",
                table: "Lancamentos",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NumeroParcela",
                table: "Lancamentos",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalParcelas",
                table: "Lancamentos",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Lancamentos_GrupoParcelamentoId",
                table: "Lancamentos",
                column: "GrupoParcelamentoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Lancamentos_GrupoParcelamentoId",
                table: "Lancamentos");

            migrationBuilder.DropColumn(
                name: "GrupoParcelamentoId",
                table: "Lancamentos");

            migrationBuilder.DropColumn(
                name: "NumeroParcela",
                table: "Lancamentos");

            migrationBuilder.DropColumn(
                name: "TotalParcelas",
                table: "Lancamentos");
        }
    }
}
