using System.Text.Encodings.Web;
using System.Text.Unicode;
using GatewayServer.ControlPlane;
using GatewayServer.ControlPlane.Http;
using GatewayServer.Data;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// 统一响应:全局自动包裹信封 + 模型校验 400 也出信封;JSON 不转义中文
builder.Services.AddControllers(o => o.Filters.Add<ApiResponseWrapperFilter>())
    .AddJsonOptions(o => o.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All))
    .ConfigureApiBehaviorOptions(o =>
    {
        o.InvalidModelStateResponseFactory = ctx =>
        {
            var errors = ctx.ModelState
                .Where(kv => kv.Value?.Errors.Count > 0)
                .SelectMany(kv => kv.Value!.Errors.Select(e => new { field = kv.Key, message = e.ErrorMessage }))
                .ToArray();
            return new ObjectResult(ApiResponse.Fail(400, "参数校验失败", new { errors })) { StatusCode = 400 };
        };
    });
// 注册数据访问（与网关共用 GatewayDbContext）
builder.Services.AddGatewayData(builder.Configuration);
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// 注册 dal 和 bll
builder.Services.UseDalAndBlls();

var app = builder.Build();

// 全局异常 → 统一信封(尽早,包住整条管线)
app.UseMiddleware<ApiExceptionMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
