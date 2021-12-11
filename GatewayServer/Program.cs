using GatewayServer.ProxyConfigProviders.DbProxyConfigProvider;
using Yarp.ReverseProxy.Configuration;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));


var app = builder.Build();


app.MapPost("/reload", async (ctx) =>
{
    // 1. 先校验 TOKEN

    // 2. 判空
    var dcp = app.Services.GetService<IProxyConfigProvider>() as DbProxyConfigProvider;
    if (dcp == null)
    {
        return;
    }

    // 3. 实际执行 Reload
    await dcp.Reload();

    await ctx.Response.WriteAsync("Refresh ok!");
});

var LogRequest = (HttpContext ctx) =>
{
    Console.WriteLine(ctx.Request.Path);
};


var LogResponse = (HttpContext ctx) =>
{
};

app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapReverseProxy((proxyPipeline) =>
    {
        proxyPipeline.Use(async (ctx, next) =>
        {
            LogRequest(ctx);
            await next();
            LogResponse(ctx);
        });
    });
});

app.Run();