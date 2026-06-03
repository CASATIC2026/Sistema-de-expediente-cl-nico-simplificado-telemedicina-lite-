using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelMedAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddDUIToUsuarios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_consulta_cita_id",
                table: "consulta");

            migrationBuilder.AddColumn<string>(
                name: "DUI",
                table: "Usuarios",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_consulta_cita_id",
                table: "consulta",
                column: "cita_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_consulta_cita_id",
                table: "consulta");

            migrationBuilder.DropColumn(
                name: "DUI",
                table: "Usuarios");

            migrationBuilder.CreateIndex(
                name: "IX_consulta_cita_id",
                table: "consulta",
                column: "cita_id");
        }
    }
}
