using Microsoft.EntityFrameworkCore.Migrations;

namespace Vibechat.Web.Migrations
{
    public partial class lastviewedmessages : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LastViewedMessages",
                columns: table => new
                {
                    UserID = table.Column<string>(nullable: false),
                    ChatID = table.Column<int>(nullable: false),
                    MessageID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LastViewedMessages", x => new { x.ChatID, x.UserID });
                    table.ForeignKey(
                        name: "FK_LastViewedMessages_Conversations_ChatID",
                        column: x => x.ChatID,
                        principalTable: "Conversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LastViewedMessages_Messages_MessageID",
                        column: x => x.MessageID,
                        principalTable: "Messages",
                        principalColumn: "MessageID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LastViewedMessages_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LastViewedMessages_MessageID",
                table: "LastViewedMessages",
                column: "MessageID");

            migrationBuilder.CreateIndex(
                name: "IX_LastViewedMessages_UserID",
                table: "LastViewedMessages",
                column: "UserID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LastViewedMessages");
        }
    }
}
