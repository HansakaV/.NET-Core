using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.API.Middlewares.Exceptions;

namespace StudentManagement.API.ExceptionHandlers;

public sealed class GlobalExceptionHandler
    : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private const string CorrelationHeader = "X-Correlation-ID";

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var corellectionId = httpContext.Items[CorrelationHeader]?.ToString();
        var problemDetails = CreateProblemDetails(
            httpContext,
            exception);

        if (problemDetails.Status >= 500)
        {
            _logger.LogError(
                exception,
                "Unhandled exception occurred. TraceId: {TraceId}",
                httpContext.TraceIdentifier);
        }
        else
        {
            _logger.LogWarning(
                exception,
                "Request failed with status {StatusCode}. TraceId: {TraceId}",
                problemDetails.Status,
                httpContext.TraceIdentifier);
        }
        if (!string.IsNullOrWhiteSpace(corellectionId))
        {
            problemDetails.Extensions["correlationId"] = corellectionId;
        }

        httpContext.Response.StatusCode =
            problemDetails.Status
            ?? StatusCodes.Status500InternalServerError;

        await httpContext.Response.WriteAsJsonAsync(
            problemDetails,
            cancellationToken);

        return true;
    }

    private static ProblemDetails CreateProblemDetails(
        HttpContext httpContext,
        Exception exception)
    {
        var problemDetails = exception switch
        {
            ValidationException validationException =>
                CreateValidationProblemDetails(
                    httpContext,
                    validationException),

            StudentNotFoundException =>
                CreateProblemDetails(
                    httpContext,
                    StatusCodes.Status404NotFound,
                    "Student not found",
                    exception.Message),

            KeyNotFoundException =>
                CreateProblemDetails(
                    httpContext,
                    StatusCodes.Status404NotFound,
                    "Resource not found",
                    exception.Message),

            UnauthorizedAccessException =>
                CreateProblemDetails(
                    httpContext,
                    StatusCodes.Status401Unauthorized,
                    "Unauthorized",
                    exception.Message),

            ForbiddenException =>
                CreateProblemDetails(
                    httpContext,
                    StatusCodes.Status403Forbidden,
                    "Forbidden",
                    exception.Message),

            StudentConcurrencyException =>
                CreateProblemDetails(
                    httpContext,
                    StatusCodes.Status409Conflict,
                    "Concurrency conflict",
                    exception.Message),

            _ =>
                CreateProblemDetails(
                    httpContext,
                    StatusCodes.Status500InternalServerError,
                    "Internal server error",
                    "An unexpected error occurred.")
        };

        problemDetails.Extensions["traceId"] =
            httpContext.TraceIdentifier;

        return problemDetails;
    }

    private static ProblemDetails CreateProblemDetails(
        HttpContext httpContext,
        int statusCode,
        string title,
        string detail)
    {
        return new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };
    }

    private static ValidationProblemDetails
        CreateValidationProblemDetails(
            HttpContext httpContext,
            ValidationException exception)
    {
        var errors = exception.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group
                    .Select(error => error.ErrorMessage)
                    .Distinct()
                    .ToArray());

        return new ValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Validation failed",
            Detail = "One or more validation errors occurred.",
            Instance = httpContext.Request.Path
        };
    }
}