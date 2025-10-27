namespace RadarSystem.WebAPI.Middlewares
{
    /// <summary>
    /// 请求日志中间件
    /// </summary>
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var requestId = Guid.NewGuid().ToString();
            context.Items["RequestId"] = requestId;

            _logger.LogInformation("请求开始: {Method} {Path} - RequestId: {RequestId}",
                context.Request.Method,
                context.Request.Path,
                requestId);

            var sw = System.Diagnostics.Stopwatch.StartNew();

            await _next(context);

            sw.Stop();

            _logger.LogInformation("请求完成: {Method} {Path} - {StatusCode} - {Elapsed}ms - RequestId: {RequestId}",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                sw.ElapsedMilliseconds,
                requestId);
        }
    }
}

