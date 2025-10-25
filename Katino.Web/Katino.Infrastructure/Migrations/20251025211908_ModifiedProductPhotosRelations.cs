using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Katino.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ModifiedProductPhotosRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductPhotos_Products_ProductId",
                table: "ProductPhotos");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "ProductPhotos",
                newName: "ProductVariantId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductPhotos_ProductId",
                table: "ProductPhotos",
                newName: "IX_ProductPhotos_ProductVariantId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductPhotos_ProductVariants_ProductVariantId",
                table: "ProductPhotos",
                column: "ProductVariantId",
                principalTable: "ProductVariants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductPhotos_ProductVariants_ProductVariantId",
                table: "ProductPhotos");

            migrationBuilder.RenameColumn(
                name: "ProductVariantId",
                table: "ProductPhotos",
                newName: "ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductPhotos_ProductVariantId",
                table: "ProductPhotos",
                newName: "IX_ProductPhotos_ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductPhotos_Products_ProductId",
                table: "ProductPhotos",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
