using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TelMedAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddHorarioDoctor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "horario_doctor",
                columns: table => new
                {
                    id_horario = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    dia_semana = table.Column<int>(type: "integer", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    hora_inicio = table.Column<TimeSpan>(type: "interval", nullable: false),
                    hora_fin = table.Column<TimeSpan>(type: "interval", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_horario_doctor", x => x.id_horario);
                });

            migrationBuilder.InsertData(
                table: "horario_doctor",
                columns: new[] { "id_horario", "activo", "dia_semana", "hora_fin", "hora_inicio" },
                values: new object[,]
                {
                    { 1, true, 1, new TimeSpan(0, 16, 0, 0, 0), new TimeSpan(0, 8, 0, 0, 0) },
                    { 2, true, 2, new TimeSpan(0, 16, 0, 0, 0), new TimeSpan(0, 8, 0, 0, 0) },
                    { 3, true, 3, new TimeSpan(0, 16, 0, 0, 0), new TimeSpan(0, 8, 0, 0, 0) },
                    { 4, true, 4, new TimeSpan(0, 16, 0, 0, 0), new TimeSpan(0, 8, 0, 0, 0) },
                    { 5, true, 5, new TimeSpan(0, 16, 0, 0, 0), new TimeSpan(0, 8, 0, 0, 0) },
                    { 6, true, 6, new TimeSpan(0, 12, 0, 0, 0), new TimeSpan(0, 9, 0, 0, 0) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "horario_doctor");
        }
    }
}
