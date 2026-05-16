using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelMedAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddIncapacidadToConsulta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "dias_incapacidad",
                table: "consulta",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_fin_incapacidad",
                table: "consulta",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_inicio_incapacidad",
                table: "consulta",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "motivo_incapacidad",
                table: "consulta",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "observaciones_incapacidad",
                table: "consulta",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "tiene_incapacidad",
                table: "consulta",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "dias_incapacidad",
                table: "consulta");

            migrationBuilder.DropColumn(
                name: "fecha_fin_incapacidad",
                table: "consulta");

            migrationBuilder.DropColumn(
                name: "fecha_inicio_incapacidad",
                table: "consulta");

            migrationBuilder.DropColumn(
                name: "motivo_incapacidad",
                table: "consulta");

            migrationBuilder.DropColumn(
                name: "observaciones_incapacidad",
                table: "consulta");

            migrationBuilder.DropColumn(
                name: "tiene_incapacidad",
                table: "consulta");
        }
    }
}
