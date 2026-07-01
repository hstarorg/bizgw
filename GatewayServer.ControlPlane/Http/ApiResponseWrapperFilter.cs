using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GatewayServer.ControlPlane.Http
{
    /// <summary>
    /// 成功路径自动包裹:把 action 返回值套进统一信封。已是信封的不重复包;
    /// 非 2xx / 文件流等结果不动(错误走 <see cref="ApiExceptionMiddleware"/> 与校验工厂)。
    /// </summary>
    public sealed class ApiResponseWrapperFilter : IAsyncResultFilter
    {
        public Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            switch (context.Result)
            {
                // 带 body 的返回:Ok(data) / 直接返回对象 / StatusCode(201, data) 等
                case ObjectResult obj when obj.Value is not ApiResponse:
                    context.Result = new ObjectResult(ApiResponse.Ok(obj.Value)) { StatusCode = obj.StatusCode };
                    break;

                // 无 body 的成功:Ok() / NoContent() 等 → 补一个 data=null 的信封,保持全系统同形
                case StatusCodeResult sc when sc.StatusCode is >= 200 and < 300:
                    context.Result = new ObjectResult(ApiResponse.Ok()) { StatusCode = sc.StatusCode };
                    break;

                case EmptyResult:
                    context.Result = new ObjectResult(ApiResponse.Ok());
                    break;
            }

            return next();
        }
    }
}
