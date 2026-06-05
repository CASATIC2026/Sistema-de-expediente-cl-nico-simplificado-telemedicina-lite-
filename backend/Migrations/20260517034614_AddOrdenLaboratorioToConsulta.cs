using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelMedAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddOrdenLaboratorioToConsulta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "estado_orden",
                table: "consulta",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "examenes_json",
                table: "consulta",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "observaciones_laboratorio",
                table: "consulta",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "resultado_pdf_path",
                table: "consulta",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "tiene_orden_laboratorio",
                table: "consulta",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "estado_orden",
                table: "consulta");

            migrationBuilder.DropColumn(
                name: "examenes_json",
                table: "consulta");

            migrationBuilder.DropColumn(
                name: "observaciones_laboratorio",
                table: "consulta");

            migrationBuilder.DropColumn(
                name: "resultado_pdf_path",
                table: "consulta");

            migrationBuilder.DropColumn(
                name: "tiene_orden_laboratorio",
                table: "consulta");
        }
    }
}
