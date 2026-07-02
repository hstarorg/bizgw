using GatewayServer.ControlPlane.BLL;
using GatewayServer.ControlPlane.Config;
using GatewayServer.ControlPlane.DAL;
using GatewayServer.ControlPlane.Services;

namespace GatewayServer.ControlPlane
{
    public static class ServiceExtensions
    {
        public static void UseDalAndBlls(this IServiceCollection services)
        {
            // 配置发布 / 查询
            services.AddScoped<ConfigPublishService>();
            services.AddScoped<ConfigQueryService>();

            // 编辑 CRUD 服务
            services.AddScoped<RouteService>();

            // TODO(7.2b): 用真实 ClusterService/DestinationService 替换以下占位 DAL/BLL
            services.AddScoped<ClusterDal>();
            services.AddScoped<ClusterBll>();
        }
    }
}
