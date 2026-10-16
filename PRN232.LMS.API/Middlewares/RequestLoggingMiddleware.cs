using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace PRN232.LMS.API.Middlewares
{
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
            var stopwatch = Stopwatch.StartNew();
            var path = context.Request.Path;
            var fullPath = path + context.Request.QueryString;
            var method = context.Request.Method;

            if (context.Request.Headers.TryGetValue("X-Request-Id", out var requestId) && !string.IsNullOrWhiteSpace(requestId))
            {
                context.Response.Headers["X-Request-Id"] = requestId.ToString();
            }

            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();
                var statusCode = context.Response.StatusCode;
                var elapsedMs = stopwatch.ElapsedMilliseconds;

                _logger.LogInformation(
                    "[HTTP {Method}] {Path} returned {StatusCode} in {Elapsed} ms",
                    method,
                    fullPath,
                    statusCode,
                    elapsedMs);
            }
        }
    }
}
