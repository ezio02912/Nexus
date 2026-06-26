using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.Services.Notification.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationRecipientEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "recipient_email",
                table: "notifications",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "recipient_email",
                table: "notifications");
        }
    }
}
