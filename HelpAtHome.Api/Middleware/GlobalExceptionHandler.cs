using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace HelpAtHome.Api.Middleware
{
    /// <summary>
    /// Catches all unhandled exceptions and returns a RFC 7807 ProblemDetails response.
    /// Registered via builder.Services.AddExceptionHandler<GlobalExceptionHandler>()
    /// and app.UseExceptionHandler() in Program.cs.
    /// </summary>
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        private readonly IHostEnvironment _env;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment env)
        {
            _logger = logger;
            _env = env;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext context, Exception exception, CancellationToken ct)
        {
            var (statusCode, title) = MapException(exception);

            _logger.LogError(
                exception,
                "Unhandled exception [{StatusCode}] {Method} {Path} — {Message}",
                statusCode,
                context.Request.Method,
                context.Request.Path,
                exception.Message);

            var problem = new ProblemDetails
            {
                Status   = statusCode,
                Title    = title,
                Type     = $"https://httpstatuses.com/{statusCode}",
                Instance = $"{context.Request.Method} {context.Request.Path}"
            };

            // Expose stack trace only in Development — never in production
            if (_env.IsDevelopment())
                problem.Detail = exception.ToString();

            context.Response.StatusCode = statusCode;
            await context.Response.WriteAsJsonAsync(problem, ct);

            return true; // exception handled — stop propagation
        }

        private static (int statusCode, string title) MapException(Exception ex) => ex switch
        {
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            KeyNotFoundException        => (StatusCodes.Status404NotFound,     "Resource not found"),
            ArgumentException           => (StatusCodes.Status400BadRequest,   ex.Message),
            InvalidOperationException   => (StatusCodes.Status400BadRequest,   ex.Message),
            _                           => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
        };
    }
}
