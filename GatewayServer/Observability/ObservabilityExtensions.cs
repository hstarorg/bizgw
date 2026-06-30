using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace GatewayServer.Observability
{
    public static class ObservabilityExtensions
    {
        /// <summary>YARP 暴露的 Meter 名,订阅后可拿到转发相关指标。</summary>
        private const string YarpMeterName = "Yarp.ReverseProxy";

        /// <summary>
        /// 按开关接入 OpenTelemetry(traces/metrics/logs,OTLP 导出)。
        /// **一等要求:`Observability:Enabled` 默认 false;关闭时完全不注册 OTel —— 零开销、无需 collector。**
        /// 端点/采样复用 OTel 标准环境变量(OTEL_EXPORTER_OTLP_ENDPOINT、OTEL_TRACES_SAMPLER 等,SDK 自动读取)。
        /// </summary>
        public static IServiceCollection AddObservability(
            this IServiceCollection services, IConfiguration configuration, string defaultServiceName)
        {
            var enabled = configuration.GetValue<bool?>("Observability:Enabled") ?? false;
            if (!enabled)
            {
                return services; // 关闭:不注册任何东西
            }

            var serviceName = Environment.GetEnvironmentVariable("OTEL_SERVICE_NAME") ?? defaultServiceName;
            var instanceId = Environment.GetEnvironmentVariable("INSTANCE_ID")
                ?? Environment.GetEnvironmentVariable("HOSTNAME")
                ?? Environment.MachineName;

            services.AddOpenTelemetry()
                .ConfigureResource(r => r
                    .AddService(serviceName: serviceName)
                    .AddAttributes([new KeyValuePair<string, object>("service.instance.id", instanceId)]))
                .WithTracing(t => t
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter())
                .WithMetrics(m => m
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddMeter(YarpMeterName) // 网关进程的 YARP 转发指标
                    .AddOtlpExporter())
                .WithLogging(l => l
                    .AddOtlpExporter());

            return services;
        }
    }
}
