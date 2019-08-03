using Microsoft.EntityFrameworkCore.Migrations;

namespace Vibechat.Web.Migrations
{
    public partial class addfilesize : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AttachmentKinds",
                column: "Name",
                value: "file");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AttachmentKinds",
                keyColumn: "Name",
                keyValue: "file");
        }
    }
}
