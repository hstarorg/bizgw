using GatewayServer.ProxyConfigProviders.DbProxyConfigProvider;
using Yarp.ReverseProxy.Configuration;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));


var app = builder.Build();


app.MapPost("/reload", async (ctx) =>
{
    // 1. ��У�� TOKEN

    // 2. �п�
    var dcp = app.Services.GetService<IProxyConfigProvider>() as DbProxyConfigProvider;
    if (dcp == null)
    {
        return;
    }

    // 3. ʵ��ִ�� Reload
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