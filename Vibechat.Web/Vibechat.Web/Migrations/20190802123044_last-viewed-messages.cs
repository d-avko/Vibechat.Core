using Microsoft.EntityFrameworkCore.Migrations;

namespace Vibechat.Web.Migrations
{
    public partial class lastviewedmessages : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                "LastViewedMessages",
                table => new
                {
                    UserID = table.Column<string>(),
                    ChatID = table.Column<int>(),
                    MessageID = table.Column<int>()
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LastViewedMessages", x => new {x.ChatID, x.UserID});
                    table.ForeignKey(
                        "FK_LastViewedMessages_Conversations_ChatID",
                        x => x.ChatID,
                        "Chats",
                        "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        "FK_LastViewedMessages_Messages_MessageID",
                        x => x.MessageID,
                        "Messages",
                        "MessageID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        "FK_LastViewedMessages_AspNetUsers_UserID",
                        x => x.UserID,
                        "AspNetUsers",
                        "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                "IX_LastViewedMessages_MessageID",
                "LastViewedMessages",
                "MessageID");

            migrationBuilder.CreateIndex(
                "IX_LastViewedMessages_UserID",
                "LastViewedMessages",
                "UserID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                "LastViewedMessages");
        }
    }
}