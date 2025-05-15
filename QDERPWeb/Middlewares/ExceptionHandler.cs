using System.Net;
using System.Text.Json;

namespace QD.ERP.Web.Middlewares
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
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred while processing the request.");
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (exception is UnauthorizedAccessException)
            {
                // Handle unauthorized access by redirecting to the login page
                context.Response.Redirect("http://localhost:60232/test/Security/Login");
                return;
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var errorDetails = new
            {
                StatusCode = context.Response.StatusCode,
                Message = "An unexpected error occurred. Please try again later.",
                Detailed = exception.Message // Consider hiding this in production for security
            };

            var result = JsonSerializer.Serialize(errorDetails, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase // Use camelCase for JSON properties
            });

            try
            {
                context.Items["Exception"] = errorDetails; // Pass error details to other middleware if needed
                await context.Response.WriteAsync(result);
            }
            catch (Exception writeEx)
            {
                // Log any issues that occur while writing the response
                Console.WriteLine($"Failed to write error response: {writeEx.Message}");
            }
        }

    }
}
