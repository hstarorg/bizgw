using GatewayServer;
using GatewayServer.ConfigProvider;
using GatewayServer.Middlewares;
using GatewayServer.Utils;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging();
// 注册配置 provider（数据访问 + 拉源，host 不直接依赖 Data）
builder.Services.AddConfigProvider(builder.Configuration);
// 注册 controllers
builder.Services.AddControllers();
// 获取代理配置（从异步 Provider 加载，而非 appsettings）
//builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
builder.Services.AddReverseProxy().LoadFromAsyncProvider((succeed, ex) =>
{
    if (succeed)
    {
        Console.WriteLine("获取配置成功");
    }
    else
    {
        Console.WriteLine("记录日志，加载配置失败 {0}", ex);
        System.Diagnostics.Process.GetCurrentProcess().Kill();
    }
});

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Program");

// 注册控制器
app.MapControllers();

// 健康检查:/healthz = liveness(进程存活即可)，/readyz = readiness(DB 可达 + 首次配置已加载)
app.MapHealthChecks("/healthz", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/readyz", new HealthCheckOptions { Predicate = c => c.Tags.Contains("ready") });

// 配置跨域
app.UseCors(builder =>
{
    builder
         .AllowAnyOrigin() // 允许所有的 origin
         .AllowAnyMethod()
         .AllowAnyHeader();
});

app.UseRouting();
// 使用路由端点
app.UseEndpoints(endpoints =>
{
    endpoints.MapReverseProxy((proxyPipeline) =>
    {
        // 注册日志记录中间件
        proxyPipeline.UseLogRequest();
    });
});

GlobalConfig.AuthCode = Environment.GetEnvironmentVariable("AuthCode") ?? GatewayUtil.GenerateRandomString();
logger.LogInformation("AuthCode={0}", GlobalConfig.AuthCode);

app.Run();
