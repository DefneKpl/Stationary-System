using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartStationerySystem.Migrations
{
    /// <inheritdoc />
    public partial class AddQueueItemRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_QueueItems_OrderId",
                table: "QueueItems",
                column: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_QueueItems_Orders_OrderId",
                table: "QueueItems",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QueueItems_Orders_OrderId",
                table: "QueueItems");

            migrationBuilder.DropIndex(
                name: "IX_QueueItems_OrderId",
                table: "QueueItems");
        }
    }
}
