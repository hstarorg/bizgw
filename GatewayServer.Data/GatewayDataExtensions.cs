using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GatewayServer.Data
{
    public static class GatewayDataExtensions
    {
        /// <summary>
        /// 注册 GatewayDbContext 工厂（PostgreSQL）。连接串来自配置 ConnectionString（环境变量优先）。
        /// </summary>
        public static IServiceCollection AddGatewayData(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration["ConnectionString"]
                ?? throw new InvalidOperationException("缺少必需的配置 ConnectionString。");

            services.AddDbContextFactory<GatewayDbContext>(options => options.UseNpgsql(connectionString));
            return services;
        }
    }
}
