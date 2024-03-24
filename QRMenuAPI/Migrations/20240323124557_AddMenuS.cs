using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QRMenuAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddMenuS : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FoodMenu_Foods_FoodId",
                table: "FoodMenu");

            migrationBuilder.DropForeignKey(
                name: "FK_FoodMenu_Menus_MenuId",
                table: "FoodMenu");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FoodMenu",
                table: "FoodMenu");

            migrationBuilder.RenameTable(
                name: "FoodMenu",
                newName: "FoodMenus");

            migrationBuilder.RenameIndex(
                name: "IX_FoodMenu_FoodId",
                table: "FoodMenus",
                newName: "IX_FoodMenus_FoodId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FoodMenus",
                table: "FoodMenus",
                columns: new[] { "MenuId", "FoodId" });

            migrationBuilder.AddForeignKey(
                name: "FK_FoodMenus_Foods_FoodId",
                table: "FoodMenus",
                column: "FoodId",
                principalTable: "Foods",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FoodMenus_Menus_MenuId",
                table: "FoodMenus",
                column: "MenuId",
                principalTable: "Menus",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FoodMenus_Foods_FoodId",
                table: "FoodMenus");

            migrationBuilder.DropForeignKey(
                name: "FK_FoodMenus_Menus_MenuId",
                table: "FoodMenus");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FoodMenus",
                table: "FoodMenus");

            migrationBuilder.RenameTable(
                name: "FoodMenus",
                newName: "FoodMenu");

            migrationBuilder.RenameIndex(
                name: "IX_FoodMenus_FoodId",
                table: "FoodMenu",
                newName: "IX_FoodMenu_FoodId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FoodMenu",
                table: "FoodMenu",
                columns: new[] { "MenuId", "FoodId" });

            migrationBuilder.AddForeignKey(
                name: "FK_FoodMenu_Foods_FoodId",
                table: "FoodMenu",
                column: "FoodId",
                principalTable: "Foods",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FoodMenu_Menus_MenuId",
                table: "FoodMenu",
                column: "MenuId",
                principalTable: "Menus",
                principalColumn: "Id");
        }
    }
}
