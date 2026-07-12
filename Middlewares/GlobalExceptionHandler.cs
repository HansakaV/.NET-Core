using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace StudentManagement.API.Middlewares
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
        {
           _logger.LogError(exception, "An unhandled exception occurred.");

           var statusCode = exception switch
           {
               KeyNotFoundException => StatusCodes.Status404NotFound,
               ArgumentException => StatusCodes.Status400BadRequest,
               UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                _ => StatusCodes.Status500InternalServerError
           };

           var problemDetails = new ProblemDetails
           {
               Type = "https://httpstatuses.com/" + statusCode,
                Title = exception.Message,
                Status = statusCode,
                Detail = exception.StackTrace
           };
           context.Response.StatusCode = statusCode;
           await context.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
           return true;
        }
    }
}