using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ViaCekek.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tekneler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TekneKodu = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TekneAdi = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Aktif = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tekneler", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tekneler_TekneKodu",
                table: "Tekneler",
                column: "TekneKodu",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tekneler");
        }
    }
}
