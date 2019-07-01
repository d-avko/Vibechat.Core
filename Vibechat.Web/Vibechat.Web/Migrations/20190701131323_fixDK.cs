using Microsoft.EntityFrameworkCore.Migrations;

namespace Vibechat.Web.Migrations
{
    public partial class fixDK : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Messages_ConversationID",
                table: "Messages",
                column: "ConversationID");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Conversations_ConversationID",
                table: "Messages",
                column: "ConversationID",
                principalTable: "Conversations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Conversations_ConversationID",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_ConversationID",
                table: "Messages");
        }
    }
}
