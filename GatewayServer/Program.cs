using GatewayServer.AsyncProxyConfig.ConfigHelper;
using GatewayServer.AsyncProxyConfig.ProxyAsyncProvider;
using GatewayServer.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// ���controllers
builder.Services.AddControllers();
// ��ȡ�����������
//builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
builder.Services.AddReverseProxy().LoadFromAsyncProvider(AsyncConfigHelperType.DB, (succeed, ex) =>
{
    if (succeed)
    {
        Console.WriteLine("��ȡ���óɹ�");
    }
    else
    {
        Console.WriteLine("��¼��־����������ʧ�� {0}", ex);
        //System.Diagnostics.Process.GetCurrentProcess().Kill();
    }
});

var app = builder.Build();

// ע�������
app.MapControllers();

// ��������������
app.UseCors(builder =>
{
    builder
         .AllowAnyOrigin() // �������е� origin
         .AllowAnyMethod()
         .AllowAnyHeader();
});

app.UseRouting();
// ʹ��·�ɶ˵�
app.UseEndpoints(endpoints =>
{
    endpoints.MapReverseProxy((proxyPipeline) =>
    {
        // ע����־��¼�м��
        proxyPipeline.UseLogRequest();
    });
});

app.Run();