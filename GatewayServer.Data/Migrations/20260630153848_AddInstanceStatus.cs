using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GatewayServer.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddInstanceStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "instance_status",
                columns: table => new
                {
                    instance_id = table.Column<string>(type: "text", nullable: false),
                    hostname = table.Column<string>(type: "text", nullable: false),
                    applied_config_version = table.Column<long>(type: "bigint", nullable: false),
                    last_reload_at = table.Column<long>(type: "bigint", nullable: false),
                    last_reload_ok = table.Column<bool>(type: "boolean", nullable: false),
                    started_at = table.Column<long>(type: "bigint", nullable: false),
                    last_heartbeat_at = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_instance_status", x => x.instance_id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "instance_status");
        }
    }
}
