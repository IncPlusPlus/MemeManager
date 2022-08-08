using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MemeManager.Migrations
{
    public partial class MoreCategoryRelationStuff : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Memes_Categories_CategoryId",
                table: "Memes");

            migrationBuilder.AlterColumn<int>(
                name: "CategoryId",
                table: "Memes",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddForeignKey(
                name: "FK_Memes_Categories_CategoryId",
                table: "Memes",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Memes_Categories_CategoryId",
                table: "Memes");

            migrationBuilder.AlterColumn<int>(
                name: "CategoryId",
                table: "Memes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Memes_Categories_CategoryId",
                table: "Memes",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
