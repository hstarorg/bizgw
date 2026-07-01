namespace GatewayServer.ControlPlane.Http
{
    /// <summary>
    /// 全局异常 → 统一信封:<see cref="ApiException"/> 映射为其状态码 + 信息 + 明细;
    /// 其余异常记日志后返回 500 信封(不泄栈)。
    /// </summary>
    public sealed class ApiExceptionMiddleware(RequestDelegate next, ILogger<ApiExceptionMiddleware> logger)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (ApiException ex)
            {
                if (!context.Response.HasStarted) context.Response.Clear();
                await ApiJson.WriteAsync(context, ex.StatusCode, ApiResponse.Fail(ex.StatusCode, ex.Message, ex.Details));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "未处理异常");
                if (!context.Response.HasStarted) context.Response.Clear();
                await ApiJson.WriteAsync(context, 500, ApiResponse.Fail(500, "服务器内部错误"));
            }
        }
    }
}
