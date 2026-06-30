using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GatewayServer.Data
{
    /// <summary>
    /// 设计期(dotnet ef)用的工厂。迁移针对 PostgreSQL 生成；
    /// `migrations add` 不需连库；`database update` 连 ConnectionString 指向的库。
    /// </summary>
    public class GatewayDbContextDesignTimeFactory : IDesignTimeDbContextFactory<GatewayDbContext>
    {
        public GatewayDbContext CreateDbContext(string[] args)
        {
            var connectionString = Environment.GetEnvironmentVariable("ConnectionString")
                ?? "Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=gatewaydb";
            var options = new DbContextOptionsBuilder<GatewayDbContext>()
                .UseNpgsql(connectionString)
                .Options;
            return new GatewayDbContext(options);
        }
    }
}
