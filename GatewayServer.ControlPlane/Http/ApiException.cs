namespace GatewayServer.ControlPlane.Http
{
    /// <summary>
    /// 领域错误:带 HTTP 状态码 + 信息 + 可选明细,由 <see cref="ApiExceptionMiddleware"/> 转成统一信封。
    /// </summary>
    public sealed class ApiException : Exception
    {
        public int StatusCode { get; }
        public object? Details { get; }

        public ApiException(int statusCode, string message, object? details = null) : base(message)
        {
            StatusCode = statusCode;
            Details = details;
        }

        public static ApiException NotFound(string message = "资源不存在") => new(404, message);
        public static ApiException Conflict(string message) => new(409, message);
        public static ApiException BadRequest(string message, object? details = null) => new(400, message, details);
        public static ApiException Unprocessable(string message, object? details = null) => new(422, message, details);
    }
}
