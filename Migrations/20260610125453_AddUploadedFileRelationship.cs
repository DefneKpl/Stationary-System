using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartStationerySystem.Migrations
{
    /// <inheritdoc />
    public partial class AddUploadedFileRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrderId",
                table: "UploadedFiles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_UploadedFiles_OrderId",
                table: "UploadedFiles",
                column: "OrderId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UploadedFiles_Orders_OrderId",
                table: "UploadedFiles",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UploadedFiles_Orders_OrderId",
                table: "UploadedFiles");

            migrationBuilder.DropIndex(
                name: "IX_UploadedFiles_OrderId",
                table: "UploadedFiles");

            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "UploadedFiles");
        }
    }
}
