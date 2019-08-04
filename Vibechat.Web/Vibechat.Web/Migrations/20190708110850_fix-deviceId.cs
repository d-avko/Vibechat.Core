using Microsoft.EntityFrameworkCore.Migrations;

namespace Vibechat.Web.Migrations
{
    public partial class fixdeviceId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                "DeviceId",
                "Conversations");

            migrationBuilder.AddColumn<string>(
                "DeviceId",
                "UsersConversations",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                "DeviceId",
                "UsersConversations");

            migrationBuilder.AddColumn<string>(
                "DeviceId",
                "Conversations",
                nullable: true);
        }
    }
}