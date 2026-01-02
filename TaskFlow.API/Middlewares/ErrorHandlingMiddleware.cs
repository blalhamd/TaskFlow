using Microsoft.AspNetCore.Mvc;
using System.Net;
using TaskFlow.Shared.Exceptions;

namespace TaskFlow.API.Middlewares
{
    public class ErrorHandlingMiddleware 
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            var statusCode = ex switch
            {
                UnauthorizedAccessException => HttpStatusCode.Unauthorized,
                ItemNotFoundException => HttpStatusCode.NotFound,
                BadRequestException or ArgumentException => HttpStatusCode.BadRequest,
                ItemAlreadyExistsException => HttpStatusCode.Conflict,
                ValidationException => HttpStatusCode.UnprocessableEntity,
                _ => HttpStatusCode.InternalServerError
            };

            var problemDetails = new ProblemDetails
            {
                Status = (int)statusCode,
                Type = $"https://httpstatuses.com/{(int)statusCode}",
                Title = _env.IsDevelopment() ? ex.Message : "An unexpected error occurred.",
                Instance = context.Request.Path,
                Detail = _env.IsDevelopment() ? ex.StackTrace : "Please contact support if the issue persists."
            };

            if (ex is ValidationException valEx)
            {
                problemDetails.Title = "Validation Failed";
                problemDetails.Extensions.Add("errors", valEx.Message);
            }

            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = (int)statusCode;

            await context.Response.WriteAsJsonAsync(problemDetails);
        }
    }
}
