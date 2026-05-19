using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Katino.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTelegramChatConfigs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TelegramChatConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TelegramSettingsId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ChatId = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ChatName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NotificationType = table.Column<int>(type: "int", nullable: false),
                    NotificationsEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TelegramChatConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TelegramChatConfigs_TelegramSettings_TelegramSettingsId",
                        column: x => x.TelegramSettingsId,
                        principalTable: "TelegramSettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_TelegramChatConfigs_TelegramSettingsId",
                table: "TelegramChatConfigs",
                column: "TelegramSettingsId");

            migrationBuilder.Sql(@"
                INSERT INTO `TelegramChatConfigs` (`Id`, `TelegramSettingsId`, `ChatId`, `ChatName`, `NotificationType`, `NotificationsEnabled`)
                SELECT UUID(), `Id`, `ChatId`, NULL, 0, `NotificationsEnabled`
                FROM `TelegramSettings`
                WHERE `ChatId` IS NOT NULL AND `ChatId` != ''
            ");

            migrationBuilder.DropColumn(
                name: "ChatId",
                table: "TelegramSettings");

            migrationBuilder.DropColumn(
                name: "NotificationsEnabled",
                table: "TelegramSettings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ChatId",
                table: "TelegramSettings",
                type: "varchar(64)",
                maxLength: 64,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "NotificationsEnabled",
                table: "TelegramSettings",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql(@"
                UPDATE `TelegramSettings` ts
                JOIN `TelegramChatConfigs` tc ON tc.`TelegramSettingsId` = ts.`Id` AND tc.`NotificationType` = 0
                SET ts.`ChatId` = tc.`ChatId`, ts.`NotificationsEnabled` = tc.`NotificationsEnabled`
            ");

            migrationBuilder.DropTable(
                name: "TelegramChatConfigs");
        }
    }
}
