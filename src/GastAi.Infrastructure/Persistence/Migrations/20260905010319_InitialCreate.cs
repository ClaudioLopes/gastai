using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GastAi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categorias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    Tipo = table.Column<int>(type: "INTEGER", nullable: false),
                    Cor = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    PadraoDoSistema = table.Column<bool>(type: "INTEGER", nullable: false),
                    Ativa = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Lancamentos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Tipo = table.Column<int>(type: "INTEGER", nullable: false),
                    Valor = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Data = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    CategoriaId = table.Column<int>(type: "INTEGER", nullable: false),
                    FormaPagamento = table.Column<int>(type: "INTEGER", nullable: true),
                    Observacao = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lancamentos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Lancamentos_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrcamentosMensais",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CategoriaId = table.Column<int>(type: "INTEGER", nullable: true),
                    MesReferencia = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    ValorLimite = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrcamentosMensais", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrcamentosMensais_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Categorias",
                columns: new[] { "Id", "Ativa", "Cor", "Nome", "PadraoDoSistema", "Tipo" },
                values: new object[,]
                {
                    { 1, true, "#F97316", "Alimentação", true, 1 },
                    { 2, true, "#3B82F6", "Transporte", true, 1 },
                    { 3, true, "#8B5CF6", "Moradia", true, 1 },
                    { 4, true, "#EC4899", "Lazer", true, 1 },
                    { 5, true, "#EF4444", "Saúde", true, 1 },
                    { 6, true, "#6B7280", "Outros", true, 1 },
                    { 7, true, "#22C55E", "Salário", true, 2 },
                    { 8, true, "#14B8A6", "Freelance", true, 2 },
                    { 9, true, "#6B7280", "Outro", true, 2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Lancamentos_CategoriaId",
                table: "Lancamentos",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Lancamentos_Data",
                table: "Lancamentos",
                column: "Data");

            migrationBuilder.CreateIndex(
                name: "IX_OrcamentosMensais_CategoriaId_MesReferencia",
                table: "OrcamentosMensais",
                columns: new[] { "CategoriaId", "MesReferencia" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Lancamentos");

            migrationBuilder.DropTable(
                name: "OrcamentosMensais");

            migrationBuilder.DropTable(
                name: "Categorias");
        }
    }
}
