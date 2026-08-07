using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ViaCekek.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class CekekTakipDurumVeBeklenenBitis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "BeklenenBitisZamani",
                table: "CekekTakipleri",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Durum",
                table: "CekekTakipleri",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BeklenenBitisZamani",
                table: "CekekTakipleri");

            migrationBuilder.DropColumn(
                name: "Durum",
                table: "CekekTakipleri");
        }
    }
}
