using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GatewayServer.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddConfigSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "config_snapshot",
                columns: table => new
                {
                    version = table.Column<long>(type: "bigint", nullable: false),
                    doc = table.Column<string>(type: "jsonb", nullable: false),
                    schema_version = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    published_at = table.Column<long>(type: "bigint", nullable: false),
                    published_by = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_config_snapshot", x => x.version);
                });

            migrationBuilder.CreateIndex(
                name: "IX_config_snapshot_is_active",
                table: "config_snapshot",
                column: "is_active",
                unique: true,
                filter: "is_active");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "config_snapshot");
        }
    }
}
