using System.Text.Encodings.Web;
using System.Text.Unicode;
using GatewayServer.ControlPlane;
using GatewayServer.ControlPlane.Auth;
using GatewayServer.ControlPlane.Http;
using GatewayServer.ControlPlane.Services;
using GatewayServer.Data;
using GatewayServer.Data.Entities;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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

// 认证 / 当前用户 / 口令哈希 / 用户服务
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddSingleton<IPasswordHasher<UserEntity>, PasswordHasher<UserEntity>>();
builder.Services.AddScoped<UserService>();

// Cookie 认证:未认证/越权对 /api 返回 401/403 信封(不 302 跳转)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.Cookie.Name = "bizgw.auth";
        o.Cookie.HttpOnly = true;
        o.Cookie.SameSite = SameSiteMode.Lax;
        o.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // 内网 HTTP 也可下发;有 TLS 时自动 Secure
        o.ExpireTimeSpan = TimeSpan.FromDays(7);
        o.SlidingExpiration = true;
        o.Events.OnRedirectToLogin = ctx =>
            ApiJson.WriteAsync(ctx.Response.HttpContext, 401, ApiResponse.Fail(401, "未登录"));
        o.Events.OnRedirectToAccessDenied = ctx =>
            ApiJson.WriteAsync(ctx.Response.HttpContext, 403, ApiResponse.Fail(403, "无权限"));
    });
// 默认全需登录;[AllowAnonymous] 放行(status/setup/login + 后续静态壳)
builder.Services.AddAuthorization(o =>
    o.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
