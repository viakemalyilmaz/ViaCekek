using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ViaCekek.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class KisiModulu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Kisiler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KimlikNumarasi = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AdSoyad = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FirmaAdi = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Telefon = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    Aktif = table.Column<bool>(type: "bit", nullable: false),
                    YasaklanmaSebebi = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    KayitTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Kaydeden = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Guncelleyen = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kisiler", x => x.Id);
                    table.CheckConstraint("CK_Kisiler_YasaklanmaSebebi_Aktif", "[Aktif] = 1 AND [YasaklanmaSebebi] IS NULL OR [Aktif] = 0");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Kisiler_FirmaAdi",
                table: "Kisiler",
                column: "FirmaAdi");

            migrationBuilder.CreateIndex(
                name: "IX_Kisiler_KimlikNumarasi",
                table: "Kisiler",
                column: "KimlikNumarasi",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Kisiler");
        }
    }
}
