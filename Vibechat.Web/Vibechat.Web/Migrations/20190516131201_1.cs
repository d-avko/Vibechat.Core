using Microsoft.EntityFrameworkCore.Migrations;

namespace Vibechat.Web.Migrations
{
    public partial class _1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatorId",
                table: "Conversations",
                nullable: true);


            migrationBuilder.CreateIndex(
                name: "IX_Conversations_CreatorId",
                table: "Conversations",
                column: "CreatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Conversations_AspNetUsers_CreatorId",
                table: "Conversations",
                column: "CreatorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Conversations_AspNetUsers_CreatorId",
                table: "Conversations");

            migrationBuilder.DropIndex(
                name: "IX_Conversations_CreatorId",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "Conversations");

        }
    }
}
