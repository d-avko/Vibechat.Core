using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Vibechat.Web.Migrations
{
    public partial class setattachmentsmessagesdk : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropForeignKey(
            //    name: "FK_Messages_Attachments_AttachmentInfoAttachmentID",
            //    table: "Messages");

            //migrationBuilder.DropIndex(
            //    name: "IX_Messages_AttachmentInfoAttachmentID",
            //    table: "Messages");

            //migrationBuilder.DropColumn(
            //    name: "AttachmentInfoAttachmentID",
            //    table: "Messages");

            //migrationBuilder.AlterColumn<int>(
            //    name: "AttachmentID",
            //    table: "Attachments",
            //    nullable: false,
            //    oldClrType: typeof(int))
            //    .OldAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddForeignKey(
                name: "FK_Attachments_Messages_MessageId",
                table: "Attachments",
                column: "MessageId",
                principalTable: "Messages",
                principalColumn: "MessageID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attachments_Messages_AttachmentID",
                table: "Attachments");

            migrationBuilder.AddColumn<int>(
                name: "AttachmentInfoAttachmentID",
                table: "Messages",
                nullable: true);

            //migrationBuilder.AlterColumn<int>(
            //    name: "AttachmentID",
            //    table: "Attachments",
            //    nullable: false,
            //    oldClrType: typeof(int))
            //    .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            migrationBuilder.CreateIndex(
                name: "IX_Messages_AttachmentInfoAttachmentID",
                table: "Messages",
                column: "AttachmentInfoAttachmentID");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Attachments_AttachmentInfoAttachmentID",
                table: "Messages",
                column: "AttachmentInfoAttachmentID",
                principalTable: "Attachments",
                principalColumn: "AttachmentID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
