using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Katino.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ProdcutAndMeasurementsUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "ProductVariantMeasurements",
                type: "varchar(512)",
                maxLength: 512,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Products",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Products");

            migrationBuilder.AlterColumn<decimal>(
                name: "Value",
                table: "ProductVariantMeasurements",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(string),
                oldType: "varchar(512)",
                oldMaxLength: 512,
                oldNullable: true)
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
