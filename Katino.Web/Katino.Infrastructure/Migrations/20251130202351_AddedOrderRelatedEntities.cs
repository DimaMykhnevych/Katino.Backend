using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Katino.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedOrderRelatedEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "MiddleName",
                table: "NpContactPersons",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "NpOptionsSeats",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    VolumetricWidth = table.Column<double>(type: "double", nullable: false),
                    VolumetricLength = table.Column<double>(type: "double", nullable: false),
                    VolumetricHeight = table.Column<double>(type: "double", nullable: false),
                    Weight = table.Column<double>(type: "double", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NpOptionsSeats", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OrderRecipients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    InstUrl = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    NpContactPersonId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderRecipients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderRecipients_NpContactPersons_NpContactPersonId",
                        column: x => x.NpContactPersonId,
                        principalTable: "NpContactPersons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Order",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    SenderNpWarehouseId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    RecipientNpWarehouseId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    SenderNpCityId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    RecipientNpCityId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    SenderContactPersonId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    OrderRecipientId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PayerType = table.Column<int>(type: "int", nullable: false),
                    PaymentMethod = table.Column<int>(type: "int", nullable: false),
                    SaleType = table.Column<int>(type: "int", nullable: false),
                    CreationDateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    SendUntilDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Weight = table.Column<double>(type: "double", nullable: false),
                    DeliveryType = table.Column<int>(type: "int", nullable: false),
                    SeatsAmount = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Cost = table.Column<double>(type: "double", nullable: false),
                    AfterpaymentOnGoodsCost = table.Column<double>(type: "double", nullable: true),
                    InternetDocumentCreationAttempted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    InternetDocumentRef = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    InternetDocumentIntDocNumber = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Order", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Order_NpCities_RecipientNpCityId",
                        column: x => x.RecipientNpCityId,
                        principalTable: "NpCities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Order_NpCities_SenderNpCityId",
                        column: x => x.SenderNpCityId,
                        principalTable: "NpCities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Order_NpContactPersons_SenderContactPersonId",
                        column: x => x.SenderContactPersonId,
                        principalTable: "NpContactPersons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Order_NpWarehouses_RecipientNpWarehouseId",
                        column: x => x.RecipientNpWarehouseId,
                        principalTable: "NpWarehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Order_NpWarehouses_SenderNpWarehouseId",
                        column: x => x.SenderNpWarehouseId,
                        principalTable: "NpWarehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Order_OrderRecipients_OrderRecipientId",
                        column: x => x.OrderRecipientId,
                        principalTable: "OrderRecipients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OrderAddressInfo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    RecipientAddressNote = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RecipientCity = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RecipientAddressName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RecipientHouse = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RecipientFlat = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OrderId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderAddressInfo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderAddressInfo_Order_Id",
                        column: x => x.Id,
                        principalTable: "Order",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrderAddressInfo_Order_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Order",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    IsCustomTailoring = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Comment = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    ProductVariantId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    OrderId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItems_Order_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Order",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderItems_ProductVariants_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OrderNpOptionsSeats",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    NpOptionsSeatId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    OrderId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderNpOptionsSeats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderNpOptionsSeats_NpOptionsSeats_NpOptionsSeatId",
                        column: x => x.NpOptionsSeatId,
                        principalTable: "NpOptionsSeats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderNpOptionsSeats_Order_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Order",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Order_OrderRecipientId",
                table: "Order",
                column: "OrderRecipientId");

            migrationBuilder.CreateIndex(
                name: "IX_Order_RecipientNpCityId",
                table: "Order",
                column: "RecipientNpCityId");

            migrationBuilder.CreateIndex(
                name: "IX_Order_RecipientNpWarehouseId",
                table: "Order",
                column: "RecipientNpWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_Order_SenderContactPersonId",
                table: "Order",
                column: "SenderContactPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Order_SenderNpCityId",
                table: "Order",
                column: "SenderNpCityId");

            migrationBuilder.CreateIndex(
                name: "IX_Order_SenderNpWarehouseId",
                table: "Order",
                column: "SenderNpWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderAddressInfo_OrderId",
                table: "OrderAddressInfo",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_ProductVariantId",
                table: "OrderItems",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderNpOptionsSeats_NpOptionsSeatId",
                table: "OrderNpOptionsSeats",
                column: "NpOptionsSeatId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderNpOptionsSeats_OrderId",
                table: "OrderNpOptionsSeats",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderRecipients_NpContactPersonId",
                table: "OrderRecipients",
                column: "NpContactPersonId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderAddressInfo");

            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "OrderNpOptionsSeats");

            migrationBuilder.DropTable(
                name: "NpOptionsSeats");

            migrationBuilder.DropTable(
                name: "Order");

            migrationBuilder.DropTable(
                name: "OrderRecipients");

            migrationBuilder.UpdateData(
                table: "NpContactPersons",
                keyColumn: "MiddleName",
                keyValue: null,
                column: "MiddleName",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "MiddleName",
                table: "NpContactPersons",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
