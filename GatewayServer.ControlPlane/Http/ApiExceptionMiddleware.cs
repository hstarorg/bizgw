using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace GatewayServer.ControlPlane.Http
{
    /// <summary>
    /// 全局异常 → 统一信封:<see cref="ApiException"/> 映射为其状态码 + 信息 + 明细;
    /// 其余异常记日志后返回 500 信封(不泄栈)。
    /// </summary>
    public sealed class ApiExceptionMiddleware(RequestDelegate next, ILogger<ApiExceptionMiddleware> logger)
    {
        private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
        };

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (ApiException ex)
            {
                await WriteAsync(context, ex.StatusCode, ApiResponse.Fail(ex.StatusCode, ex.Message, ex.Details));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "未处理异常");
                await WriteAsync(context, 500, ApiResponse.Fail(500, "服务器内部错误"));
            }
        }

        private static async Task WriteAsync(HttpContext ctx, int status, ApiResponse body)
        {
            if (ctx.Response.HasStarted) return;
            ctx.Response.Clear();
            ctx.Response.StatusCode = status;
            ctx.Response.ContentType = "application/json; charset=utf-8";
            await ctx.Response.WriteAsync(JsonSerializer.Serialize(body, Json));
        }
    }
}
