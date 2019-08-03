using Microsoft.EntityFrameworkCore.Migrations;

namespace Vibechat.Web.Migrations
{
    public partial class fixdeviceId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeviceId",
                table: "Conversations");

            migrationBuilder.AddColumn<string>(
                name: "DeviceId",
                table: "UsersConversations",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeviceId",
                table: "UsersConversations");

            migrationBuilder.AddColumn<string>(
                name: "DeviceId",
                table: "Conversations",
                nullable: true);
        }
    }
}
