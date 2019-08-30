using Microsoft.EntityFrameworkCore.Migrations;

namespace Vibechat.Web.Migrations
{
    public partial class enumattachmentkind : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                "FK_Attachments_AttachmentKinds_AttachmentKindName",
                "Attachments");

            migrationBuilder.DropIndex(
                "IX_Attachments_AttachmentKindName",
                "Attachments");

            migrationBuilder.DropPrimaryKey(
                "PK_AttachmentKinds",
                "AttachmentKinds");

            migrationBuilder.DeleteData(
                "AttachmentKinds",
                "Name",
                "file");

            migrationBuilder.DeleteData(
                "AttachmentKinds",
                "Name",
                "img");

            migrationBuilder.DropColumn(
                "AttachmentKindName",
                "Attachments");

            migrationBuilder.DropColumn(
                "Name",
                "AttachmentKinds");

            migrationBuilder.AddColumn<int>(
                "AttachmentKindKind",
                "Attachments",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                "Kind",
                "AttachmentKinds",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                "PK_AttachmentKinds",
                "AttachmentKinds",
                "Kind");

            migrationBuilder.InsertData(
                "AttachmentKinds",
                "Kind",
                new object[]
                {
                    0,
                    1
                });

            migrationBuilder.CreateIndex(
                "IX_Attachments_AttachmentKindKind",
                "Attachments",
                "AttachmentKindKind");

            migrationBuilder.AddForeignKey(
                "FK_Attachments_AttachmentKinds_AttachmentKindKind",
                "Attachments",
                "AttachmentKindKind",
                "AttachmentKinds",
                principalColumn: "Kind",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                "FK_Attachments_AttachmentKinds_AttachmentKindKind",
                "Attachments");

            migrationBuilder.DropIndex(
                "IX_Attachments_AttachmentKindKind",
                "Attachments");

            migrationBuilder.DropPrimaryKey(
                "PK_AttachmentKinds",
                "AttachmentKinds");

            migrationBuilder.DeleteData(
                "AttachmentKinds",
                "Kind",
                0);

            migrationBuilder.DeleteData(
                "AttachmentKinds",
                "Kind",
                1);

            migrationBuilder.DropColumn(
                "AttachmentKindKind",
                "Attachments");

            migrationBuilder.DropColumn(
                "Kind",
                "AttachmentKinds");

            migrationBuilder.AddColumn<string>(
                "AttachmentKindName",
                "Attachments",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                "Name",
                "AttachmentKinds",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                "PK_AttachmentKinds",
                "AttachmentKinds",
                "Name");

            migrationBuilder.InsertData(
                "AttachmentKinds",
                "Name",
                new object[]
                {
                    "img",
                    "file"
                });

            migrationBuilder.CreateIndex(
                "IX_Attachments_AttachmentKindName",
                "Attachments",
                "AttachmentKindName");

            migrationBuilder.AddForeignKey(
                "FK_Attachments_AttachmentKinds_AttachmentKindName",
                "Attachments",
                "AttachmentKindName",
                "AttachmentKinds",
                principalColumn: "Name",
                onDelete: ReferentialAction.Restrict);
        }
    }
}