using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelMedAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificacionRecordatorioCita : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "notificacion_recordatorio_enviada",
                table: "cita",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "notificacion_recordatorio_enviada",
                table: "cita");
        }
    }
}
