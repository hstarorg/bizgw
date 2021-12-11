using GatewayServer;
using GatewayServer.AsyncProxyConfig.ConfigHelper;
using GatewayServer.AsyncProxyConfig.ProxyAsyncProvider;
using GatewayServer.Middlewares;
using GatewayServer.Utils;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging();
// 添加controllers
builder.Services.AddControllers();
// 获取反向代理配置
//builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
builder.Services.AddReverseProxy().LoadFromAsyncProvider(AsyncConfigHelperType.DB, (succeed, ex) =>
{
    if (succeed)
    {
        Console.WriteLine("读取配置成功");
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

// 允许外层请求跨域
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

GlobalConfig.AuthCode = Environment.GetEnvironmentVariable("AutCode") ?? GatewayUtil.GenerateRandomString();
logger.LogInformation("AuthCode={0}", GlobalConfig.AuthCode);

app.Run();