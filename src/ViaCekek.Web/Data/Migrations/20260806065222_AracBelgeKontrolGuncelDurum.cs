using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ViaCekek.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AracBelgeKontrolGuncelDurum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AracBelgeKontrolleri_CekekTakipleri_CekekTakipId",
                table: "AracBelgeKontrolleri");

            migrationBuilder.DropIndex(
                name: "IX_AracBelgeKontrolleri_CekekTakipId_AracBelgeId",
                table: "AracBelgeKontrolleri");

            migrationBuilder.AlterColumn<int>(
                name: "CekekTakipId",
                table: "AracBelgeKontrolleri",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "AracId",
                table: "AracBelgeKontrolleri",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_AracBelgeKontrolleri_AracId_AracBelgeId",
                table: "AracBelgeKontrolleri",
                columns: new[] { "AracId", "AracBelgeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AracBelgeKontrolleri_CekekTakipId",
                table: "AracBelgeKontrolleri",
                column: "CekekTakipId");

            migrationBuilder.AddForeignKey(
                name: "FK_AracBelgeKontrolleri_Araclar_AracId",
                table: "AracBelgeKontrolleri",
                column: "AracId",
                principalTable: "Araclar",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AracBelgeKontrolleri_CekekTakipleri_CekekTakipId",
                table: "AracBelgeKontrolleri",
                column: "CekekTakipId",
                principalTable: "CekekTakipleri",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AracBelgeKontrolleri_Araclar_AracId",
                table: "AracBelgeKontrolleri");

            migrationBuilder.DropForeignKey(
                name: "FK_AracBelgeKontrolleri_CekekTakipleri_CekekTakipId",
                table: "AracBelgeKontrolleri");

            migrationBuilder.DropIndex(
                name: "IX_AracBelgeKontrolleri_AracId_AracBelgeId",
                table: "AracBelgeKontrolleri");

            migrationBuilder.DropIndex(
                name: "IX_AracBelgeKontrolleri_CekekTakipId",
                table: "AracBelgeKontrolleri");

            migrationBuilder.DropColumn(
                name: "AracId",
                table: "AracBelgeKontrolleri");

            migrationBuilder.AlterColumn<int>(
                name: "CekekTakipId",
                table: "AracBelgeKontrolleri",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AracBelgeKontrolleri_CekekTakipId_AracBelgeId",
                table: "AracBelgeKontrolleri",
                columns: new[] { "CekekTakipId", "AracBelgeId" });

            migrationBuilder.AddForeignKey(
                name: "FK_AracBelgeKontrolleri_CekekTakipleri_CekekTakipId",
                table: "AracBelgeKontrolleri",
                column: "CekekTakipId",
                principalTable: "CekekTakipleri",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
