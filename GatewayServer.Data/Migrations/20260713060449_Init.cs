using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GatewayServer.Data.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "app_user",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    username = table.Column<string>(type: "text", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    role = table.Column<string>(type: "text", nullable: false),
                    is_deleted = table.Column<short>(type: "smallint", nullable: false),
                    modifier_name = table.Column<string>(type: "text", nullable: false),
                    modify_date = table.Column<long>(type: "bigint", nullable: false),
                    creator_name = table.Column<string>(type: "text", nullable: false),
                    create_date = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_app_user", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "cluster",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    cluster_code = table.Column<string>(type: "text", nullable: false),
                    cluster_name = table.Column<string>(type: "text", nullable: false),
                    load_balancing_policy = table.Column<string>(type: "text", nullable: false),
                    enabled_health_check = table.Column<short>(type: "smallint", nullable: false),
                    health_check_interval = table.Column<int>(type: "integer", nullable: false),
                    health_check_timeout = table.Column<int>(type: "integer", nullable: false),
                    health_check_policy = table.Column<string>(type: "text", nullable: false),
                    health_check_path = table.Column<string>(type: "text", nullable: false),
                    is_deleted = table.Column<short>(type: "smallint", nullable: false),
                    remark = table.Column<string>(type: "text", nullable: false),
                    modifier_name = table.Column<string>(type: "text", nullable: false),
                    modify_date = table.Column<long>(type: "bigint", nullable: false),
                    creator_name = table.Column<string>(type: "text", nullable: false),
                    create_date = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cluster", x => x.id);
                });

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

            migrationBuilder.CreateTable(
                name: "destination",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    cluster_code = table.Column<string>(type: "text", nullable: false),
                    address = table.Column<string>(type: "text", nullable: false),
                    health_check_path = table.Column<string>(type: "text", nullable: false),
                    is_deleted = table.Column<short>(type: "smallint", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    modifier_name = table.Column<string>(type: "text", nullable: false),
                    modify_date = table.Column<long>(type: "bigint", nullable: false),
                    creator_name = table.Column<string>(type: "text", nullable: false),
                    create_date = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_destination", x => x.id);
                });

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

            migrationBuilder.CreateTable(
                name: "route",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    route_name = table.Column<string>(type: "text", nullable: false),
                    cluster_code = table.Column<string>(type: "text", nullable: false),
                    match_path = table.Column<string>(type: "text", nullable: false),
                    match_methods = table.Column<string>(type: "text", nullable: false),
                    transforms = table.Column<string>(type: "text", nullable: false),
                    is_deleted = table.Column<short>(type: "smallint", nullable: false),
                    remark = table.Column<string>(type: "text", nullable: false),
                    modifier_name = table.Column<string>(type: "text", nullable: false),
                    modify_date = table.Column<long>(type: "bigint", nullable: false),
                    creator_name = table.Column<string>(type: "text", nullable: false),
                    create_date = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_route", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_app_user_username",
                table: "app_user",
                column: "username",
                unique: true,
                filter: "is_deleted = 0");

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
                name: "app_user");

            migrationBuilder.DropTable(
                name: "cluster");

            migrationBuilder.DropTable(
                name: "config_snapshot");

            migrationBuilder.DropTable(
                name: "destination");

            migrationBuilder.DropTable(
                name: "instance_status");

            migrationBuilder.DropTable(
                name: "route");
        }
    }
}
