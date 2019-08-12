using Microsoft.EntityFrameworkCore.Migrations;
using Vibechat.Web.DTO.Messages;

namespace Vibechat.Web.Migrations
{
    public partial class backwardscompat : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                $"UPDATE \"Messages\" AS x SET \"Type\" = 1 WHERE x.\"IsAttachment\"  ");

            migrationBuilder.Sql(
                $"UPDATE \"Messages\" AS x SET \"Type\" = 0 WHERE NOT x.\"IsAttachment\" " +
                $"AND (x.\"ForwardedMessageMessageID\" IS NULL) ");

            migrationBuilder.Sql(
                $"UPDATE \"Messages\" AS x SET \"Type\" = 3 WHERE x.\"ForwardedMessageMessageID\" IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
