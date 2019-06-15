using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Vibechat.Web.Migrations
{
    public partial class securechats : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EncryptedPayload",
                table: "Messages",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuthKeyId",
                table: "Conversations",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSecure",
                table: "Conversations",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "PublicKeyId",
                table: "Conversations",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Contacts",
                columns: table => new
                {
                    ContactId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<string>(nullable: true),
                    ContactId1 = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contacts", x => x.ContactId);
                    table.ForeignKey(
                        name: "FK_Contacts_AspNetUsers_ContactId1",
                        column: x => x.ContactId1,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Contacts_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PublicKeys",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Modulus = table.Column<string>(nullable: true),
                    Generator = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicKeys", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_PublicKeyId",
                table: "Conversations",
                column: "PublicKeyId");

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_ContactId1",
                table: "Contacts",
                column: "ContactId1");

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_UserId",
                table: "Contacts",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Conversations_PublicKeys_PublicKeyId",
                table: "Conversations",
                column: "PublicKeyId",
                principalTable: "PublicKeys",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Conversations_PublicKeys_PublicKeyId",
                table: "Conversations");

            migrationBuilder.DropTable(
                name: "Contacts");

            migrationBuilder.DropTable(
                name: "PublicKeys");

            migrationBuilder.DropIndex(
                name: "IX_Conversations_PublicKeyId",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "EncryptedPayload",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "AuthKeyId",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "IsSecure",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "PublicKeyId",
                table: "Conversations");
        }
    }
}
