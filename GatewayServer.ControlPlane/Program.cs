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

// 本地开发:加载 .env(从当前目录向上查找);NoClobber = 已存在的环境变量优先,.env 只兜底
DotNetEnv.Env.TraversePath().NoClobber().Load();

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

// Cookie 票据密钥:env AuthCookieKey(32 字节的 base64,生成:openssl rand -base64 32)。必填 —— 缺失直接启动失败。
var cookieKeyB64 = builder.Configuration["AuthCookieKey"];
if (string.IsNullOrEmpty(cookieKeyB64))
{
    throw new InvalidOperationException("必须配置环境变量 AuthCookieKey(32 字节的 base64,生成:openssl rand -base64 32)。");
}
byte[] cookieKey;
try { cookieKey = Convert.FromBase64String(cookieKeyB64); }
catch (FormatException) { throw new InvalidOperationException("AuthCookieKey 不是合法的 base64。"); }

// Cookie 认证:未认证/越权对 /api 返回 401/403 信封(不 302 跳转)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        // 固定密钥加密票据(替代 DataProtection):登录态不再依赖容器本地 key ring
        o.TicketDataFormat = new FixedKeyTicketFormat(cookieKey);
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

// 静态托管:web-ui 构建产物(容器构建时拷入 wwwroot);本地开发走 Vite dev server,不依赖此处
app.UseDefaultFiles();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // Vite 产物带内容 hash → 长缓存;index.html 等非 hash 文件不缓存(否则发新版拿不到新资源)
        ctx.Context.Response.Headers.CacheControl =
            ctx.Context.Request.Path.StartsWithSegments("/assets")
                ? "public,max-age=31536000,immutable"
                : "no-cache";
    },
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// /api/* 未命中任何 controller → 信封 404(绝不能回退成 index.html)
app.MapFallback("/api/{**path}", (HttpContext ctx) =>
    ApiJson.WriteAsync(ctx, 404, ApiResponse.Fail(404, "接口不存在"))).AllowAnonymous();

// SPA 回退:其余路径回 index.html(匿名 —— 登录/初始化引导由前端调 /api/auth/status 决定)
app.MapFallbackToFile("index.html", new StaticFileOptions
{
    OnPrepareResponse = ctx => ctx.Context.Response.Headers.CacheControl = "no-cache",
}).AllowAnonymous();

app.Run();
