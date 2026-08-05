using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ViaCekek.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AracModulu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Araclar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TakipNumarasi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AracTuru = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FirmaAdi = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Aktif = table.Column<bool>(type: "bit", nullable: false),
                    YasaklanmaSebebi = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    KayitTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Kaydeden = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Guncelleyen = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Araclar", x => x.Id);
                    table.CheckConstraint("CK_Araclar_YasaklanmaSebebi_Aktif", "[Aktif] = 1 AND [YasaklanmaSebebi] IS NULL OR [Aktif] = 0");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Araclar_FirmaAdi",
                table: "Araclar",
                column: "FirmaAdi");

            migrationBuilder.CreateIndex(
                name: "IX_Araclar_TakipNumarasi",
                table: "Araclar",
                column: "TakipNumarasi",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Araclar");
        }
    }
}
