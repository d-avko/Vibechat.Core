using Microsoft.EntityFrameworkCore.Migrations;

namespace Vibechat.Web.Migrations
{
    public partial class RemoveRgbColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PictureBackgroundRgb",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "ProfilePicRgb",
                table: "AspNetUsers");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PictureBackgroundRgb",
                table: "Conversations",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProfilePicRgb",
                table: "AspNetUsers",
                nullable: true);
        }
    }
}
