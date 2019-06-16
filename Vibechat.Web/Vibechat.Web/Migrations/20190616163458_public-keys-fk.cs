using Microsoft.EntityFrameworkCore.Migrations;

namespace Vibechat.Web.Migrations
{
    public partial class publickeysfk : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Conversations_PublicKeys_PublicKeyId",
                table: "Conversations");

            migrationBuilder.DropIndex(
                name: "IX_Conversations_PublicKeyId",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "PublicKeyId",
                table: "Conversations");

            migrationBuilder.AddColumn<int>(
                name: "ChatConvID",
                table: "PublicKeys",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PublicKeys_ChatConvID",
                table: "PublicKeys",
                column: "ChatConvID");

            migrationBuilder.AddForeignKey(
                name: "FK_PublicKeys_Conversations_ChatConvID",
                table: "PublicKeys",
                column: "ChatConvID",
                principalTable: "Conversations",
                principalColumn: "ConvID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PublicKeys_Conversations_ChatConvID",
                table: "PublicKeys");

            migrationBuilder.DropIndex(
                name: "IX_PublicKeys_ChatConvID",
                table: "PublicKeys");

            migrationBuilder.DropColumn(
                name: "ChatConvID",
                table: "PublicKeys");

            migrationBuilder.AddColumn<int>(
                name: "PublicKeyId",
                table: "Conversations",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_PublicKeyId",
                table: "Conversations",
                column: "PublicKeyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Conversations_PublicKeys_PublicKeyId",
                table: "Conversations",
                column: "PublicKeyId",
                principalTable: "PublicKeys",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
