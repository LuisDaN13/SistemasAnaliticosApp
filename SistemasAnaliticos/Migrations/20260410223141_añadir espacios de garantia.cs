using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemasAnaliticos.Migrations
{
    /// <inheritdoc />
    public partial class añadirespaciosdegarantia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "montoFinal",
                table: "Garantia",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "porcentaje",
                table: "Garantia",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "montoFinal",
                table: "Garantia");

            migrationBuilder.DropColumn(
                name: "porcentaje",
                table: "Garantia");
        }
    }
}
