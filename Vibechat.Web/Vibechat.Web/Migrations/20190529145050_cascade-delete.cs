using Microsoft.EntityFrameworkCore.Migrations;

namespace Vibechat.Web.Migrations
{
    public partial class cascadedelete : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConversationsBans_Conversations_ConversationConvID",
                table: "ConversationsBans");

            migrationBuilder.DropIndex(
                name: "IX_ConversationsBans_ConversationConvID",
                table: "ConversationsBans");

            migrationBuilder.DropColumn(
                name: "ConversationConvID",
                table: "ConversationsBans");

            migrationBuilder.AddColumn<int>(
                name: "BanForeignKey",
                table: "Conversations",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_BanForeignKey",
                table: "Conversations",
                column: "BanForeignKey",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Conversations_ConversationsBans_BanForeignKey",
                table: "Conversations",
                column: "BanForeignKey",
                principalTable: "ConversationsBans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Conversations_ConversationsBans_BanForeignKey",
                table: "Conversations");

            migrationBuilder.DropIndex(
                name: "IX_Conversations_BanForeignKey",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "BanForeignKey",
                table: "Conversations");

            migrationBuilder.AddColumn<int>(
                name: "ConversationConvID",
                table: "ConversationsBans",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConversationsBans_ConversationConvID",
                table: "ConversationsBans",
                column: "ConversationConvID");

            migrationBuilder.AddForeignKey(
                name: "FK_ConversationsBans_Conversations_ConversationConvID",
                table: "ConversationsBans",
                column: "ConversationConvID",
                principalTable: "Conversations",
                principalColumn: "ConvID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
