using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Vibechat.Web.Migrations
{
    public partial class chatevents : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EventId",
                table: "Messages",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ChatEvents",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    ActorId = table.Column<string>(nullable: true),
                    UserInvolvedId = table.Column<string>(nullable: true),
                    EventType = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatEvents_AspNetUsers_ActorId",
                        column: x => x.ActorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChatEvents_AspNetUsers_UserInvolvedId",
                        column: x => x.UserInvolvedId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_EventId",
                table: "Messages",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatEvents_ActorId",
                table: "ChatEvents",
                column: "ActorId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatEvents_UserInvolvedId",
                table: "ChatEvents",
                column: "UserInvolvedId");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_ChatEvents_EventId",
                table: "Messages",
                column: "EventId",
                principalTable: "ChatEvents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_ChatEvents_EventId",
                table: "Messages");

            migrationBuilder.DropTable(
                name: "ChatEvents");

            migrationBuilder.DropIndex(
                name: "IX_Messages_EventId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "EventId",
                table: "Messages");
        }
    }
}
