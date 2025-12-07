using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Katino.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedOrdersTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Order_NpCities_RecipientNpCityId",
                table: "Order");

            migrationBuilder.DropForeignKey(
                name: "FK_Order_NpCities_SenderNpCityId",
                table: "Order");

            migrationBuilder.DropForeignKey(
                name: "FK_Order_NpContactPersons_SenderContactPersonId",
                table: "Order");

            migrationBuilder.DropForeignKey(
                name: "FK_Order_NpWarehouses_RecipientNpWarehouseId",
                table: "Order");

            migrationBuilder.DropForeignKey(
                name: "FK_Order_NpWarehouses_SenderNpWarehouseId",
                table: "Order");

            migrationBuilder.DropForeignKey(
                name: "FK_Order_OrderRecipients_OrderRecipientId",
                table: "Order");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderAddressInfo_Order_Id",
                table: "OrderAddressInfo");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderAddressInfo_Order_OrderId",
                table: "OrderAddressInfo");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Order_OrderId",
                table: "OrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderNpOptionsSeats_Order_OrderId",
                table: "OrderNpOptionsSeats");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Order",
                table: "Order");

            migrationBuilder.RenameTable(
                name: "Order",
                newName: "Orders");

            migrationBuilder.RenameIndex(
                name: "IX_Order_SenderNpWarehouseId",
                table: "Orders",
                newName: "IX_Orders_SenderNpWarehouseId");

            migrationBuilder.RenameIndex(
                name: "IX_Order_SenderNpCityId",
                table: "Orders",
                newName: "IX_Orders_SenderNpCityId");

            migrationBuilder.RenameIndex(
                name: "IX_Order_SenderContactPersonId",
                table: "Orders",
                newName: "IX_Orders_SenderContactPersonId");

            migrationBuilder.RenameIndex(
                name: "IX_Order_RecipientNpWarehouseId",
                table: "Orders",
                newName: "IX_Orders_RecipientNpWarehouseId");

            migrationBuilder.RenameIndex(
                name: "IX_Order_RecipientNpCityId",
                table: "Orders",
                newName: "IX_Orders_RecipientNpCityId");

            migrationBuilder.RenameIndex(
                name: "IX_Order_OrderRecipientId",
                table: "Orders",
                newName: "IX_Orders_OrderRecipientId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Orders",
                table: "Orders",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderAddressInfo_Orders_Id",
                table: "OrderAddressInfo",
                column: "Id",
                principalTable: "Orders",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderAddressInfo_Orders_OrderId",
                table: "OrderAddressInfo",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Orders_OrderId",
                table: "OrderItems",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderNpOptionsSeats_Orders_OrderId",
                table: "OrderNpOptionsSeats",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_NpCities_RecipientNpCityId",
                table: "Orders",
                column: "RecipientNpCityId",
                principalTable: "NpCities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_NpCities_SenderNpCityId",
                table: "Orders",
                column: "SenderNpCityId",
                principalTable: "NpCities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_NpContactPersons_SenderContactPersonId",
                table: "Orders",
                column: "SenderContactPersonId",
                principalTable: "NpContactPersons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_NpWarehouses_RecipientNpWarehouseId",
                table: "Orders",
                column: "RecipientNpWarehouseId",
                principalTable: "NpWarehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_NpWarehouses_SenderNpWarehouseId",
                table: "Orders",
                column: "SenderNpWarehouseId",
                principalTable: "NpWarehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_OrderRecipients_OrderRecipientId",
                table: "Orders",
                column: "OrderRecipientId",
                principalTable: "OrderRecipients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderAddressInfo_Orders_Id",
                table: "OrderAddressInfo");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderAddressInfo_Orders_OrderId",
                table: "OrderAddressInfo");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Orders_OrderId",
                table: "OrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderNpOptionsSeats_Orders_OrderId",
                table: "OrderNpOptionsSeats");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_NpCities_RecipientNpCityId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_NpCities_SenderNpCityId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_NpContactPersons_SenderContactPersonId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_NpWarehouses_RecipientNpWarehouseId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_NpWarehouses_SenderNpWarehouseId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_OrderRecipients_OrderRecipientId",
                table: "Orders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Orders",
                table: "Orders");

            migrationBuilder.RenameTable(
                name: "Orders",
                newName: "Order");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_SenderNpWarehouseId",
                table: "Order",
                newName: "IX_Order_SenderNpWarehouseId");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_SenderNpCityId",
                table: "Order",
                newName: "IX_Order_SenderNpCityId");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_SenderContactPersonId",
                table: "Order",
                newName: "IX_Order_SenderContactPersonId");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_RecipientNpWarehouseId",
                table: "Order",
                newName: "IX_Order_RecipientNpWarehouseId");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_RecipientNpCityId",
                table: "Order",
                newName: "IX_Order_RecipientNpCityId");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_OrderRecipientId",
                table: "Order",
                newName: "IX_Order_OrderRecipientId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Order",
                table: "Order",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Order_NpCities_RecipientNpCityId",
                table: "Order",
                column: "RecipientNpCityId",
                principalTable: "NpCities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Order_NpCities_SenderNpCityId",
                table: "Order",
                column: "SenderNpCityId",
                principalTable: "NpCities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Order_NpContactPersons_SenderContactPersonId",
                table: "Order",
                column: "SenderContactPersonId",
                principalTable: "NpContactPersons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Order_NpWarehouses_RecipientNpWarehouseId",
                table: "Order",
                column: "RecipientNpWarehouseId",
                principalTable: "NpWarehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Order_NpWarehouses_SenderNpWarehouseId",
                table: "Order",
                column: "SenderNpWarehouseId",
                principalTable: "NpWarehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Order_OrderRecipients_OrderRecipientId",
                table: "Order",
                column: "OrderRecipientId",
                principalTable: "OrderRecipients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderAddressInfo_Order_Id",
                table: "OrderAddressInfo",
                column: "Id",
                principalTable: "Order",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderAddressInfo_Order_OrderId",
                table: "OrderAddressInfo",
                column: "OrderId",
                principalTable: "Order",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Order_OrderId",
                table: "OrderItems",
                column: "OrderId",
                principalTable: "Order",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderNpOptionsSeats_Order_OrderId",
                table: "OrderNpOptionsSeats",
                column: "OrderId",
                principalTable: "Order",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
