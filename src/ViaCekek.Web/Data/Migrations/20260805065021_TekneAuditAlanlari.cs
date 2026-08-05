using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ViaCekek.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class TekneAuditAlanlari : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "GuncellemeTarihi",
                table: "Tekneler",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Guncelleyen",
                table: "Tekneler",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Kaydeden",
                table: "Tekneler",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "KayitTarihi",
                table: "Tekneler",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GuncellemeTarihi",
                table: "Tekneler");

            migrationBuilder.DropColumn(
                name: "Guncelleyen",
                table: "Tekneler");

            migrationBuilder.DropColumn(
                name: "Kaydeden",
                table: "Tekneler");

            migrationBuilder.DropColumn(
                name: "KayitTarihi",
                table: "Tekneler");
        }
    }
}
