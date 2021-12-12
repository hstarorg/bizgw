using GatewayServer.ConfigrationAPI.BLL;
using GatewayServer.ConfigrationAPI.DAL;

namespace GatewayServer.ConfigrationAPI
{
    public static class ServiceExtensions
    {
        public static void UseDalAndBlls(this IServiceCollection services)
        {
            // 注册所有的 DAL
            services.AddScoped<ClusterDal>();

            // 注册所有的 BLL
            services.AddScoped<ClusterBll>();

        }
    }
}
