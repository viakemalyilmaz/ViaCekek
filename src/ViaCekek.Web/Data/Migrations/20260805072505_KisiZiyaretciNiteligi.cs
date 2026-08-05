using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ViaCekek.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class KisiZiyaretciNiteligi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Kaptan",
                table: "Kisiler",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "TeknePersoneli",
                table: "Kisiler",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "TekneSahibi",
                table: "Kisiler",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Kaptan",
                table: "Kisiler");

            migrationBuilder.DropColumn(
                name: "TeknePersoneli",
                table: "Kisiler");

            migrationBuilder.DropColumn(
                name: "TekneSahibi",
                table: "Kisiler");
        }
    }
}
