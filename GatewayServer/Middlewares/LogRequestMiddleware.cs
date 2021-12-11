namespace GatewayServer.Middlewares
{
    public static class LogRequestMiddleware
    {
        public static void UseLogRequest(this IReverseProxyApplicationBuilder rpaBuilder)
        {
            var LogRequest = (HttpContext ctx) =>
            {
                Console.WriteLine("{0} {1} {2} {3} {4}", ctx.Request.Path, ctx.Request.Query, ctx.Request.QueryString, ctx.Request.Method, "Hello");
            };


            var LogResponse = (HttpContext ctx) =>
            {
            };

            rpaBuilder.Use(async (ctx, next) =>
            {
                LogRequest(ctx);
                await next();
                LogResponse(ctx);
            });
        }
    }
}
