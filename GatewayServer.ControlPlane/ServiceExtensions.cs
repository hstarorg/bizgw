using GatewayServer.ControlPlane.Config;
using GatewayServer.ControlPlane.Services;

namespace GatewayServer.ControlPlane
{
    public static class ServiceExtensions
    {
        /// <summary>控制面业务服务(薄 Controller + Service,直连 IDbContextFactory)。</summary>
        public static void UseDalAndBlls(this IServiceCollection services)
        {
            // 配置发布 / 查询
            services.AddScoped<ConfigPublishService>();
            services.AddScoped<ConfigQueryService>();

            // 编辑 CRUD
            services.AddScoped<RouteService>();
            services.AddScoped<ClusterService>();
            services.AddScoped<DestinationService>();
        }
    }
}
