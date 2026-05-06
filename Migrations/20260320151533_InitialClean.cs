using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TelMedAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialClean : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GoogleId = table.Column<string>(type: "text", nullable: true),
                    FotoUrl = table.Column<string>(type: "text", nullable: true),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Apellido = table.Column<string>(type: "text", nullable: false),
                    Genero = table.Column<string>(type: "text", nullable: false),
                    FechaNacimiento = table.Column<DateOnly>(type: "date", nullable: false),
                    Direccion = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    Telefono = table.Column<string>(type: "text", nullable: false),
                    Rol = table.Column<string>(type: "text", nullable: false),
                    DebeCambiarPassword = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "cita",
                columns: table => new
                {
                    id_cita = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fecha_inicio = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    fecha_fin = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    motivo = table.Column<string>(type: "text", nullable: false),
                    tipo_consulta = table.Column<string>(type: "text", nullable: false),
                    estado = table.Column<string>(type: "text", nullable: false),
                    paciente_id = table.Column<int>(type: "integer", nullable: false),
                    doctor_id = table.Column<int>(type: "integer", nullable: true),
                    link_reunion = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cita", x => x.id_cita);
                    table.ForeignKey(
                        name: "FK_cita_Usuarios_doctor_id",
                        column: x => x.doctor_id,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_cita_Usuarios_paciente_id",
                        column: x => x.paciente_id,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "consulta",
                columns: table => new
                {
                    id_consulta = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    cita_id = table.Column<int>(type: "integer", nullable: false),
                    sintomas = table.Column<string>(type: "text", nullable: false),
                    evolucion = table.Column<string>(type: "text", nullable: false),
                    diagnostico = table.Column<string>(type: "text", nullable: false),
                    tratamiento = table.Column<string>(type: "text", nullable: false),
                    observaciones = table.Column<string>(type: "text", nullable: false),
                    medicamentos_json = table.Column<string>(type: "text", nullable: false),
                    fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_consulta", x => x.id_consulta);
                    table.ForeignKey(
                        name: "FK_consulta_cita_cita_id",
                        column: x => x.cita_id,
                        principalTable: "cita",
                        principalColumn: "id_cita",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_cita_doctor_id",
                table: "cita",
                column: "doctor_id");

            migrationBuilder.CreateIndex(
                name: "IX_cita_paciente_id",
                table: "cita",
                column: "paciente_id");

            migrationBuilder.CreateIndex(
                name: "IX_consulta_cita_id",
                table: "consulta",
                column: "cita_id");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "consulta");

            migrationBuilder.DropTable(
                name: "cita");

            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}
