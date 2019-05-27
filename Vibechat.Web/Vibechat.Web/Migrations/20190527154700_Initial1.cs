using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Vibechat.Web.Migrations
{
    public partial class Initial1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConversationsBans",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    BannedUserId = table.Column<string>(nullable: true),
                    ConversationConvID = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConversationsBans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConversationsBans_AspNetUsers_BannedUserId",
                        column: x => x.BannedUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConversationsBans_Conversations_ConversationConvID",
                        column: x => x.ConversationConvID,
                        principalTable: "Conversations",
                        principalColumn: "ConvID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UsersBans",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    BannedUserId = table.Column<string>(nullable: true),
                    BannedById = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsersBans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsersBans_AspNetUsers_BannedById",
                        column: x => x.BannedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UsersBans_AspNetUsers_BannedUserId",
                        column: x => x.BannedUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConversationsBans_BannedUserId",
                table: "ConversationsBans",
                column: "BannedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ConversationsBans_ConversationConvID",
                table: "ConversationsBans",
                column: "ConversationConvID");

            migrationBuilder.CreateIndex(
                name: "IX_UsersBans_BannedById",
                table: "UsersBans",
                column: "BannedById");

            migrationBuilder.CreateIndex(
                name: "IX_UsersBans_BannedUserId",
                table: "UsersBans",
                column: "BannedUserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConversationsBans");

            migrationBuilder.DropTable(
                name: "UsersBans");
        }
    }
}
