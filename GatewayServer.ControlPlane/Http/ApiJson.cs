using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace GatewayServer.ControlPlane.Http
{
    /// <summary>把信封直接写进响应(供异常中间件、cookie 401/403 事件等复用)。中文不转义。</summary>
    public static class ApiJson
    {
        public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
        };

        public static async Task WriteAsync(HttpContext ctx, int status, ApiResponse body)
        {
            if (ctx.Response.HasStarted) return;
            ctx.Response.StatusCode = status;
            ctx.Response.ContentType = "application/json; charset=utf-8";
            await ctx.Response.WriteAsync(JsonSerializer.Serialize(body, Options));
        }
    }
}
