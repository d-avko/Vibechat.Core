using Microsoft.EntityFrameworkCore.Migrations;

namespace Vibechat.Web.Migrations
{
    public partial class enumattachmentkind : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attachments_AttachmentKinds_AttachmentKindName",
                table: "Attachments");

            migrationBuilder.DropIndex(
                name: "IX_Attachments_AttachmentKindName",
                table: "Attachments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AttachmentKinds",
                table: "AttachmentKinds");

            migrationBuilder.DeleteData(
                table: "AttachmentKinds",
                keyColumn: "Name",
                keyValue: "file");

            migrationBuilder.DeleteData(
                table: "AttachmentKinds",
                keyColumn: "Name",
                keyValue: "img");

            migrationBuilder.DropColumn(
                name: "AttachmentKindName",
                table: "Attachments");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "AttachmentKinds");

            migrationBuilder.AddColumn<int>(
                name: "AttachmentKindKind",
                table: "Attachments",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Kind",
                table: "AttachmentKinds",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_AttachmentKinds",
                table: "AttachmentKinds",
                column: "Kind");

            migrationBuilder.InsertData(
                table: "AttachmentKinds",
                column: "Kind",
                values: new object[]
                {
                    0,
                    1
                });

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_AttachmentKindKind",
                table: "Attachments",
                column: "AttachmentKindKind");

            migrationBuilder.AddForeignKey(
                name: "FK_Attachments_AttachmentKinds_AttachmentKindKind",
                table: "Attachments",
                column: "AttachmentKindKind",
                principalTable: "AttachmentKinds",
                principalColumn: "Kind",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attachments_AttachmentKinds_AttachmentKindKind",
                table: "Attachments");

            migrationBuilder.DropIndex(
                name: "IX_Attachments_AttachmentKindKind",
                table: "Attachments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AttachmentKinds",
                table: "AttachmentKinds");

            migrationBuilder.DeleteData(
                table: "AttachmentKinds",
                keyColumn: "Kind",
                keyValue: 0);

            migrationBuilder.DeleteData(
                table: "AttachmentKinds",
                keyColumn: "Kind",
                keyValue: 1);

            migrationBuilder.DropColumn(
                name: "AttachmentKindKind",
                table: "Attachments");

            migrationBuilder.DropColumn(
                name: "Kind",
                table: "AttachmentKinds");

            migrationBuilder.AddColumn<string>(
                name: "AttachmentKindName",
                table: "Attachments",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "AttachmentKinds",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AttachmentKinds",
                table: "AttachmentKinds",
                column: "Name");

            migrationBuilder.InsertData(
                table: "AttachmentKinds",
                column: "Name",
                values: new object[]
                {
                    "img",
                    "file"
                });

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_AttachmentKindName",
                table: "Attachments",
                column: "AttachmentKindName");

            migrationBuilder.AddForeignKey(
                name: "FK_Attachments_AttachmentKinds_AttachmentKindName",
                table: "Attachments",
                column: "AttachmentKindName",
                principalTable: "AttachmentKinds",
                principalColumn: "Name",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
