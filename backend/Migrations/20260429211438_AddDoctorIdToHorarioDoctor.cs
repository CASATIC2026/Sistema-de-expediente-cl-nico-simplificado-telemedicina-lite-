using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelMedAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddDoctorIdToHorarioDoctor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
{
    // Limpiamos registros huérfanos ANTES de agregar el FK
    migrationBuilder.Sql("DELETE FROM horario_doctor;");

    migrationBuilder.AddColumn<int>(
        name: "doctor_id",
        table: "horario_doctor",
        type: "integer",
        nullable: false,
        defaultValue: 0);

    migrationBuilder.CreateIndex(
        name: "IX_horario_doctor_doctor_id",
        table: "horario_doctor",
        column: "doctor_id");

    migrationBuilder.AddForeignKey(
        name: "FK_horario_doctor_Usuarios_doctor_id",
        table: "horario_doctor",
        column: "doctor_id",
        principalTable: "Usuarios",
        principalColumn: "Id",
        onDelete: ReferentialAction.Cascade);
}
        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_horario_doctor_Usuarios_doctor_id",
                table: "horario_doctor");

            migrationBuilder.DropIndex(
                name: "IX_horario_doctor_doctor_id",
                table: "horario_doctor");

            migrationBuilder.DropColumn(
                name: "doctor_id",
                table: "horario_doctor");
        }
    }
}
