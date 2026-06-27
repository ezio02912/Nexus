using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.Services.File.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddFileLinkCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_file_links_module_entity_type_entity_id",
                table: "file_links");

            migrationBuilder.AddColumn<string>(
                name: "category",
                table: "file_links",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_file_links_module_entity_type_entity_id_category",
                table: "file_links",
                columns: new[] { "module", "entity_type", "entity_id", "category" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_file_links_module_entity_type_entity_id_category",
                table: "file_links");

            migrationBuilder.DropColumn(
                name: "category",
                table: "file_links");

            migrationBuilder.CreateIndex(
                name: "IX_file_links_module_entity_type_entity_id",
                table: "file_links",
                columns: new[] { "module", "entity_type", "entity_id" });
        }
    }
}
