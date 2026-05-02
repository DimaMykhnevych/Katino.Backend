using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Katino.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedCreationDateTimeSystemToOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreationDateTimeSystem",
                table: "Orders",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.Sql("UPDATE `Orders` SET `CreationDateTimeSystem` = `CreationDateTime`");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreationDateTimeSystem",
                table: "Orders",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetime(6)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreationDateTimeSystem",
                table: "Orders");
        }
    }
}
