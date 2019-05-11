using Microsoft.EntityFrameworkCore.Migrations;

namespace Vibechat.Web.Migrations
{
    public partial class AttachmentsAsIndividualMessages : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MessageAttachmentDataModel_Messages_MessageDataModelMessageID",
                table: "MessageAttachmentDataModel");

            migrationBuilder.DropIndex(
                name: "IX_MessageAttachmentDataModel_MessageDataModelMessageID",
                table: "MessageAttachmentDataModel");

            migrationBuilder.DropColumn(
                name: "MessageDataModelMessageID",
                table: "MessageAttachmentDataModel");

            migrationBuilder.AddColumn<int>(
                name: "AttachmentInfoAttachmentID",
                table: "Messages",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAttachment",
                table: "Messages",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Messages_AttachmentInfoAttachmentID",
                table: "Messages",
                column: "AttachmentInfoAttachmentID");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_MessageAttachmentDataModel_AttachmentInfoAttachmentID",
                table: "Messages",
                column: "AttachmentInfoAttachmentID",
                principalTable: "MessageAttachmentDataModel",
                principalColumn: "AttachmentID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_MessageAttachmentDataModel_AttachmentInfoAttachmentID",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_AttachmentInfoAttachmentID",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "AttachmentInfoAttachmentID",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "IsAttachment",
                table: "Messages");

            migrationBuilder.AddColumn<int>(
                name: "MessageDataModelMessageID",
                table: "MessageAttachmentDataModel",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MessageAttachmentDataModel_MessageDataModelMessageID",
                table: "MessageAttachmentDataModel",
                column: "MessageDataModelMessageID");

            migrationBuilder.AddForeignKey(
                name: "FK_MessageAttachmentDataModel_Messages_MessageDataModelMessageID",
                table: "MessageAttachmentDataModel",
                column: "MessageDataModelMessageID",
                principalTable: "Messages",
                principalColumn: "MessageID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
