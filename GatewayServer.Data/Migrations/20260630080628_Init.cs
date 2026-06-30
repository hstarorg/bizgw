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
                name: "cluster",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    cluster_code = table.Column<string>(type: "text", nullable: false),
                    cluster_name = table.Column<string>(type: "text", nullable: false),
                    load_balancing_policy = table.Column<string>(type: "text", nullable: false),
                    enabled_helth_check = table.Column<short>(type: "smallint", nullable: false),
                    helth_check_interval = table.Column<int>(type: "integer", nullable: false),
                    helth_check_timeout = table.Column<int>(type: "integer", nullable: false),
                    helth_check_policy = table.Column<string>(type: "text", nullable: false),
                    helth_check_path = table.Column<string>(type: "text", nullable: false),
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
                name: "destination",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    cluster_code = table.Column<string>(type: "text", nullable: false),
                    address = table.Column<string>(type: "text", nullable: false),
                    helth_check_path = table.Column<string>(type: "text", nullable: false),
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cluster");

            migrationBuilder.DropTable(
                name: "destination");

            migrationBuilder.DropTable(
                name: "route");
        }
    }
}
