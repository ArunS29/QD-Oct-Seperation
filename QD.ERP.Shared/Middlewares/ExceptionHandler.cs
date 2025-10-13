using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using QD.ERP.Shared.Exceptions; // Make sure to import the custom exception namespace
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace QD.ERP.Shared.Middlewares
{
    public class ExceptionHandler
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandler> _logger;

        public ExceptionHandler(RequestDelegate next, ILogger<ExceptionHandler> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            if (httpContext == null)
                throw new ArgumentNullException(nameof(httpContext));

            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred.");
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            // 🔁 Redirect if the license is expired
            if (exception is LicenseExpiredException)
            {
                context.Response.Redirect("/pulse/Security/LicenseActivation");
                return;
            }

            // 🔁 Redirect if unauthorized access (optional)
            if (exception is UnauthorizedAccessException)
            {
                context.Response.Redirect("/Aicon/security/login");
                return;
            }

            // Default: return JSON error response
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var errorDetails = new
            {
                StatusCode = context.Response.StatusCode,
                Message = "An unexpected error occurred. Please try again later.",
                Detailed = exception.Message
            };

            var result = JsonSerializer.Serialize(errorDetails, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            try
            {
                context.Items["Exception"] = errorDetails;
                await context.Response.WriteAsync(result);
            }
            catch (Exception writeEx)
            {
                Console.WriteLine($"Failed to write error response: {writeEx.Message}");
            }
        }
    }
}
