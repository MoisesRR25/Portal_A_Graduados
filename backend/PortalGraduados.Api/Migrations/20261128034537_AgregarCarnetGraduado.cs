using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortalGraduados.Api.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCarnetGraduado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "carnet",
                table: "graduado",
                type: "character varying(7)",
                maxLength: 7,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_graduado_carnet",
                table: "graduado",
                column: "carnet",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_graduado_carnet",
                table: "graduado");

            migrationBuilder.DropColumn(
                name: "carnet",
                table: "graduado");
        }
    }
}
