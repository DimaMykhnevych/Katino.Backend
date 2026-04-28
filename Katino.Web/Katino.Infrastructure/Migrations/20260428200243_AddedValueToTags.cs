using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Katino.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedValueToTags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OrderTags_Type",
                table: "OrderTags");

            migrationBuilder.AddColumn<string>(
                name: "Value",
                table: "OrderTags",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_OrderTags_Type",
                table: "OrderTags",
                column: "Type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OrderTags_Type",
                table: "OrderTags");

            migrationBuilder.DropColumn(
                name: "Value",
                table: "OrderTags");

            migrationBuilder.CreateIndex(
                name: "IX_OrderTags_Type",
                table: "OrderTags",
                column: "Type",
                unique: true);
        }
    }
}
