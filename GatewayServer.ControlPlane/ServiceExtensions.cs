using GatewayServer.ControlPlane.BLL;
using GatewayServer.ControlPlane.DAL;

namespace GatewayServer.ControlPlane
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
