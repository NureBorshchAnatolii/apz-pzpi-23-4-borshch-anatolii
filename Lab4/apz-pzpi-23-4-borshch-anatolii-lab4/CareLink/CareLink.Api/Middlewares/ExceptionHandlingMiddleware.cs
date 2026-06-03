using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
using CareLink.Api.Models.Responses;

namespace CareLink.Api.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(
            HttpContext context,
            Exception exception)
        {
            var statusCode = HttpStatusCode.InternalServerError;
            var message = "Internal server error.";

            if (exception is ValidationException ||
                exception is ArgumentException ||
                exception is InvalidOperationException)
            {
                statusCode = HttpStatusCode.BadRequest;
                message = GetCleanMessage(exception);
            }
            else if (exception is UnauthorizedAccessException)
            {
                statusCode = HttpStatusCode.Unauthorized;
                message = "Unauthorized.";
            }
            else if (exception is KeyNotFoundException)
            {
                statusCode = HttpStatusCode.NotFound;
                message = GetCleanMessage(exception);
            }

            var response = ApiResponse.Fail(message);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(response));
        }
        
        private static string GetCleanMessage(Exception ex)
        {
            return ex.Message
                .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
                .First();
        }
    }
}