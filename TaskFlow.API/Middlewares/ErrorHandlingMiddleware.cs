using System.Diagnostics;
using System.Net;
using System.Text.Json;
using TaskFlow.API.Models;
using TaskFlow.Shared.Exceptions;

namespace TaskFlow.API.Middlewares
{
    public class ErrorHandlingMiddleware 
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;
        private readonly IHostEnvironment _host;
        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger, IHostEnvironment host)
        {
            _next = next;
            _logger = logger;
            _host = host;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled error occurred during request processing for path {RequestPath}", context.Request.Path);
                await HandleExceptionAsync(ex, context);
            }
        }

        private async Task HandleExceptionAsync(Exception ex, HttpContext context)
        {
            var traceId = Activity.Current?.Id ?? context.TraceIdentifier;

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var isDev = _host.IsDevelopment();

            var message = isDev ? ex.Message : "An unexpected error occurred.";
            var InnerExceptionMessage = isDev ? ex.InnerException?.Message : null;


            var response = new CustomErrorResponse(context.Response.StatusCode, message, InnerExceptionMessage!, traceId);

            switch (ex)
            {
                case ItemNotFoundException:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    break;
                case BadRequestException:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;
                case ItemAlreadyExistsException:
                    response.StatusCode = (int)HttpStatusCode.Conflict;
                    break;
                case InvalidOperationException:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;
                case UnauthorizedAccessException:
                    {
                        response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        response.ErrorMessage = "Unauthorized";
                    }
                    break;
                default:
                    break;
            }

            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
        }
    }
}
