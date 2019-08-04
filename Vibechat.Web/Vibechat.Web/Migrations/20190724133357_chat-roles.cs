using Microsoft.EntityFrameworkCore.Migrations;

namespace Vibechat.Web.Migrations
{
    public partial class chatroles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                "FK_Conversations_AspNetUsers_CreatorId",
                "Conversations");

            migrationBuilder.DropIndex(
                "IX_Conversations_CreatorId",
                "Conversations");

            migrationBuilder.DropColumn(
                "CreatorId",
                "Conversations");

            migrationBuilder.CreateTable(
                "Roles",
                table => new
                {
                    Id = table.Column<int>()
                },
                constraints: table => { table.PrimaryKey("PK_Roles", x => x.Id); });

            migrationBuilder.CreateTable(
                "ChatRoles",
                table => new
                {
                    ChatId = table.Column<int>(),
                    UserId = table.Column<string>(),
                    RoleId = table.Column<int>()
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatRoles", x => new {x.ChatId, x.UserId});
                    table.ForeignKey(
                        "FK_ChatRoles_Conversations_ChatId",
                        x => x.ChatId,
                        "Conversations",
                        "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        "FK_ChatRoles_Roles_RoleId",
                        x => x.RoleId,
                        "Roles",
                        "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        "FK_ChatRoles_AspNetUsers_UserId",
                        x => x.UserId,
                        "AspNetUsers",
                        "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                "Roles",
                "Id",
                new object[]
                {
                    0,
                    1,
                    2
                });

            migrationBuilder.CreateIndex(
                "IX_ChatRoles_RoleId",
                "ChatRoles",
                "RoleId");

            migrationBuilder.CreateIndex(
                "IX_ChatRoles_UserId",
                "ChatRoles",
                "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                "ChatRoles");

            migrationBuilder.DropTable(
                "Roles");

            migrationBuilder.AddColumn<string>(
                "CreatorId",
                "Conversations",
                nullable: true);

            migrationBuilder.CreateIndex(
                "IX_Conversations_CreatorId",
                "Conversations",
                "CreatorId");

            migrationBuilder.AddForeignKey(
                "FK_Conversations_AspNetUsers_CreatorId",
                "Conversations",
                "CreatorId",
                "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}