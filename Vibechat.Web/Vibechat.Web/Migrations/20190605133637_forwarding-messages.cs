using Microsoft.EntityFrameworkCore.Migrations;

namespace Vibechat.Web.Migrations
{
    public partial class forwardingmessages : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ForwardedMessageMessageID",
                table: "Messages",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ForwardedMessageMessageID",
                table: "Messages",
                column: "ForwardedMessageMessageID");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Messages_ForwardedMessageMessageID",
                table: "Messages",
                column: "ForwardedMessageMessageID",
                principalTable: "Messages",
                principalColumn: "MessageID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Messages_ForwardedMessageMessageID",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_ForwardedMessageMessageID",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "ForwardedMessageMessageID",
                table: "Messages");
        }
    }
}
