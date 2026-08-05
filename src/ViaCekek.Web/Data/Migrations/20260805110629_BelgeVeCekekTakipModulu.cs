using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ViaCekek.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class BelgeVeCekekTakipModulu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "KvkkOnayDurumu",
                table: "Kisiler",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "KvkkOnayFormuAlindi",
                table: "Kisiler",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "KvkkOnayTarihi",
                table: "Kisiler",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AracBelgeleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BelgeTanimi = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Alindi = table.Column<bool>(type: "bit", nullable: false),
                    GecerlilikTarihiKontrolu = table.Column<bool>(type: "bit", nullable: false),
                    Aktif = table.Column<bool>(type: "bit", nullable: false),
                    GecerliArac = table.Column<bool>(type: "bit", nullable: false),
                    GecerliVinc = table.Column<bool>(type: "bit", nullable: false),
                    GecerliVidanjor = table.Column<bool>(type: "bit", nullable: false),
                    GecerliKompresor = table.Column<bool>(type: "bit", nullable: false),
                    GecerliBasincliKap = table.Column<bool>(type: "bit", nullable: false),
                    KayitTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Kaydeden = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Guncelleyen = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AracBelgeleri", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CekekTakipleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KisiId = table.Column<int>(type: "int", nullable: true),
                    AracId = table.Column<int>(type: "int", nullable: true),
                    KimlikNumarasi = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TakipNumarasi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AdSoyad = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    FirmaAdi = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Telefon = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    TekneId = table.Column<int>(type: "int", nullable: true),
                    GirisTarihi = table.Column<DateOnly>(type: "date", nullable: true),
                    GirisSaati = table.Column<TimeOnly>(type: "time", nullable: true),
                    CikisTarihi = table.Column<DateOnly>(type: "date", nullable: true),
                    CikisSaati = table.Column<TimeOnly>(type: "time", nullable: true),
                    ZiyaretSebebi = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    KayitTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Kaydeden = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Guncelleyen = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CekekTakipleri", x => x.Id);
                    table.CheckConstraint("CK_CekekTakipleri_KisiVeyaArac", "([KisiId] IS NOT NULL AND [AracId] IS NULL) OR ([KisiId] IS NULL AND [AracId] IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_CekekTakipleri_Araclar_AracId",
                        column: x => x.AracId,
                        principalTable: "Araclar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CekekTakipleri_Kisiler_KisiId",
                        column: x => x.KisiId,
                        principalTable: "Kisiler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CekekTakipleri_Tekneler_TekneId",
                        column: x => x.TekneId,
                        principalTable: "Tekneler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "KisiBelgeleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BelgeTanimi = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Alindi = table.Column<bool>(type: "bit", nullable: false),
                    GecerlilikTarihiKontrolu = table.Column<bool>(type: "bit", nullable: false),
                    Aktif = table.Column<bool>(type: "bit", nullable: false),
                    GecerliCalisma = table.Column<bool>(type: "bit", nullable: false),
                    GecerliGorusme = table.Column<bool>(type: "bit", nullable: false),
                    GecerliKesif = table.Column<bool>(type: "bit", nullable: false),
                    GecerliKontrol = table.Column<bool>(type: "bit", nullable: false),
                    GecerliMalzemeAlma = table.Column<bool>(type: "bit", nullable: false),
                    GecerliMalzemeBirakma = table.Column<bool>(type: "bit", nullable: false),
                    KayitTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Kaydeden = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Guncelleyen = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KisiBelgeleri", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AracBelgeKontrolleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CekekTakipId = table.Column<int>(type: "int", nullable: false),
                    AracBelgeId = table.Column<int>(type: "int", nullable: false),
                    AlindiSonucu = table.Column<bool>(type: "bit", nullable: false),
                    GecerlilikTarihiSonucu = table.Column<DateTime>(type: "datetime2", nullable: true),
                    KayitTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Kaydeden = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Guncelleyen = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AracBelgeKontrolleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AracBelgeKontrolleri_AracBelgeleri_AracBelgeId",
                        column: x => x.AracBelgeId,
                        principalTable: "AracBelgeleri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AracBelgeKontrolleri_CekekTakipleri_CekekTakipId",
                        column: x => x.CekekTakipId,
                        principalTable: "CekekTakipleri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KisiBelgeKontrolleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CekekTakipId = table.Column<int>(type: "int", nullable: false),
                    KisiBelgeId = table.Column<int>(type: "int", nullable: false),
                    AlindiSonucu = table.Column<bool>(type: "bit", nullable: false),
                    GecerlilikTarihiSonucu = table.Column<DateTime>(type: "datetime2", nullable: true),
                    KayitTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Kaydeden = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Guncelleyen = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KisiBelgeKontrolleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KisiBelgeKontrolleri_CekekTakipleri_CekekTakipId",
                        column: x => x.CekekTakipId,
                        principalTable: "CekekTakipleri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KisiBelgeKontrolleri_KisiBelgeleri_KisiBelgeId",
                        column: x => x.KisiBelgeId,
                        principalTable: "KisiBelgeleri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AracBelgeKontrolleri_AracBelgeId",
                table: "AracBelgeKontrolleri",
                column: "AracBelgeId");

            migrationBuilder.CreateIndex(
                name: "IX_AracBelgeKontrolleri_CekekTakipId_AracBelgeId",
                table: "AracBelgeKontrolleri",
                columns: new[] { "CekekTakipId", "AracBelgeId" });

            migrationBuilder.CreateIndex(
                name: "IX_CekekTakipleri_AracId",
                table: "CekekTakipleri",
                column: "AracId");

            migrationBuilder.CreateIndex(
                name: "IX_CekekTakipleri_KisiId",
                table: "CekekTakipleri",
                column: "KisiId");

            migrationBuilder.CreateIndex(
                name: "IX_CekekTakipleri_TekneId",
                table: "CekekTakipleri",
                column: "TekneId");

            migrationBuilder.CreateIndex(
                name: "IX_KisiBelgeKontrolleri_CekekTakipId_KisiBelgeId",
                table: "KisiBelgeKontrolleri",
                columns: new[] { "CekekTakipId", "KisiBelgeId" });

            migrationBuilder.CreateIndex(
                name: "IX_KisiBelgeKontrolleri_KisiBelgeId",
                table: "KisiBelgeKontrolleri",
                column: "KisiBelgeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AracBelgeKontrolleri");

            migrationBuilder.DropTable(
                name: "KisiBelgeKontrolleri");

            migrationBuilder.DropTable(
                name: "AracBelgeleri");

            migrationBuilder.DropTable(
                name: "CekekTakipleri");

            migrationBuilder.DropTable(
                name: "KisiBelgeleri");

            migrationBuilder.DropColumn(
                name: "KvkkOnayDurumu",
                table: "Kisiler");

            migrationBuilder.DropColumn(
                name: "KvkkOnayFormuAlindi",
                table: "Kisiler");

            migrationBuilder.DropColumn(
                name: "KvkkOnayTarihi",
                table: "Kisiler");
        }
    }
}
