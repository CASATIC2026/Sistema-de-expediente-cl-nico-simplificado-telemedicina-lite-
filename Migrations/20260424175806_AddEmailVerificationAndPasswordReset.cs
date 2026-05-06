using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelMedAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailVerificationAndPasswordReset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EmailVerificationExpiresAt",
                table: "Usuarios",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmailVerificationToken",
                table: "Usuarios",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EmailVerified",
                table: "Usuarios",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PasswordResetExpiresAt",
                table: "Usuarios",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PasswordResetToken",
                table: "Usuarios",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailVerificationExpiresAt",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "EmailVerificationToken",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "EmailVerified",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "PasswordResetExpiresAt",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "PasswordResetToken",
                table: "Usuarios");
        }
    }
}
