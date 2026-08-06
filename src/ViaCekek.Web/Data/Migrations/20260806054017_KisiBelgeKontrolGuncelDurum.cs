using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ViaCekek.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class KisiBelgeKontrolGuncelDurum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KisiBelgeKontrolleri_CekekTakipleri_CekekTakipId",
                table: "KisiBelgeKontrolleri");

            migrationBuilder.DropIndex(
                name: "IX_KisiBelgeKontrolleri_CekekTakipId_KisiBelgeId",
                table: "KisiBelgeKontrolleri");

            migrationBuilder.AlterColumn<int>(
                name: "CekekTakipId",
                table: "KisiBelgeKontrolleri",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "KisiId",
                table: "KisiBelgeKontrolleri",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_KisiBelgeKontrolleri_CekekTakipId",
                table: "KisiBelgeKontrolleri",
                column: "CekekTakipId");

            migrationBuilder.CreateIndex(
                name: "IX_KisiBelgeKontrolleri_KisiId_KisiBelgeId",
                table: "KisiBelgeKontrolleri",
                columns: new[] { "KisiId", "KisiBelgeId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_KisiBelgeKontrolleri_CekekTakipleri_CekekTakipId",
                table: "KisiBelgeKontrolleri",
                column: "CekekTakipId",
                principalTable: "CekekTakipleri",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_KisiBelgeKontrolleri_Kisiler_KisiId",
                table: "KisiBelgeKontrolleri",
                column: "KisiId",
                principalTable: "Kisiler",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KisiBelgeKontrolleri_CekekTakipleri_CekekTakipId",
                table: "KisiBelgeKontrolleri");

            migrationBuilder.DropForeignKey(
                name: "FK_KisiBelgeKontrolleri_Kisiler_KisiId",
                table: "KisiBelgeKontrolleri");

            migrationBuilder.DropIndex(
                name: "IX_KisiBelgeKontrolleri_CekekTakipId",
                table: "KisiBelgeKontrolleri");

            migrationBuilder.DropIndex(
                name: "IX_KisiBelgeKontrolleri_KisiId_KisiBelgeId",
                table: "KisiBelgeKontrolleri");

            migrationBuilder.DropColumn(
                name: "KisiId",
                table: "KisiBelgeKontrolleri");

            migrationBuilder.AlterColumn<int>(
                name: "CekekTakipId",
                table: "KisiBelgeKontrolleri",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_KisiBelgeKontrolleri_CekekTakipId_KisiBelgeId",
                table: "KisiBelgeKontrolleri",
                columns: new[] { "CekekTakipId", "KisiBelgeId" });

            migrationBuilder.AddForeignKey(
                name: "FK_KisiBelgeKontrolleri_CekekTakipleri_CekekTakipId",
                table: "KisiBelgeKontrolleri",
                column: "CekekTakipId",
                principalTable: "CekekTakipleri",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
