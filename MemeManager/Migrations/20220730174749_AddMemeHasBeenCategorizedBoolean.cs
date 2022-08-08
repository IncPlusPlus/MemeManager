using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MemeManager.Migrations
{
    public partial class AddMemeHasBeenCategorizedBoolean : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Memes",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasBeenCategorized",
                table: "Memes",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasBeenCategorized",
                table: "Memes");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Memes",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");
        }
    }
}
