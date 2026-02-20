using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Katino.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemovedUniqueIndexesOnCrmUserSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CrmUserSettings_NpCities_NpCityId",
                table: "CrmUserSettings");

            migrationBuilder.DropForeignKey(
                name: "FK_CrmUserSettings_NpWarehouses_NpWarehouseId",
                table: "CrmUserSettings");

            migrationBuilder.DropIndex(
                name: "IX_CrmUserSettings_NpCityId",
                table: "CrmUserSettings");

            migrationBuilder.DropIndex(
                name: "IX_CrmUserSettings_NpWarehouseId",
                table: "CrmUserSettings");

            migrationBuilder.CreateIndex(
                name: "IX_CrmUserSettings_NpCityId",
                table: "CrmUserSettings",
                column: "NpCityId");

            migrationBuilder.CreateIndex(
                name: "IX_CrmUserSettings_NpWarehouseId",
                table: "CrmUserSettings",
                column: "NpWarehouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_CrmUserSettings_NpCities_NpCityId",
                table: "CrmUserSettings",
                column: "NpCityId",
                principalTable: "NpCities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CrmUserSettings_NpWarehouses_NpWarehouseId",
                table: "CrmUserSettings",
                column: "NpWarehouseId",
                principalTable: "NpWarehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CrmUserSettings_NpCityId",
                table: "CrmUserSettings");

            migrationBuilder.DropIndex(
                name: "IX_CrmUserSettings_NpWarehouseId",
                table: "CrmUserSettings");

            migrationBuilder.CreateIndex(
                name: "IX_CrmUserSettings_NpCityId",
                table: "CrmUserSettings",
                column: "NpCityId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CrmUserSettings_NpWarehouseId",
                table: "CrmUserSettings",
                column: "NpWarehouseId",
                unique: true);
        }
    }
}
