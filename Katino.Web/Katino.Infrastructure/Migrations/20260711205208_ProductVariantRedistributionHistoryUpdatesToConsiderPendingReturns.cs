using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Katino.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ProductVariantRedistributionHistoryUpdatesToConsiderPendingReturns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPendingPhysicalArrival",
                table: "ProductVariantRedistributionHistory",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "QuantityResolved",
                table: "ProductVariantRedistributionHistory",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPendingPhysicalArrival",
                table: "ProductVariantRedistributionHistory");

            migrationBuilder.DropColumn(
                name: "QuantityResolved",
                table: "ProductVariantRedistributionHistory");
        }
    }
}
