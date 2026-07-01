namespace GatewayServer.ControlPlane.Http
{
    /// <summary>
    /// 全系统统一响应信封:<c>{ code, message, data }</c>。code=0 成功,非 0 失败(默认取 HTTP 状态)。
    /// </summary>
    public sealed class ApiResponse
    {
        public int Code { get; init; }
        public string Message { get; init; } = "ok";
        public object? Data { get; init; }

        public static ApiResponse Ok(object? data = null) => new() { Code = 0, Message = "ok", Data = data };

        public static ApiResponse Fail(int code, string message, object? data = null)
            => new() { Code = code, Message = message, Data = data };
    }
}
