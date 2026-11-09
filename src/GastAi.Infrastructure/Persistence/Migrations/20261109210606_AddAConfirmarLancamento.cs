using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GastAi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAConfirmarLancamento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AConfirmar",
                table: "Lancamentos",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AConfirmar",
                table: "Lancamentos");
        }
    }
}
