using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ViaCekek.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class KisiBelgeIskeleKurma : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "GecerliIskeleKurma",
                table: "KisiBelgeleri",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GecerliIskeleKurma",
                table: "KisiBelgeleri");
        }
    }
}
