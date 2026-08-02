using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.API.Middlewares.Exceptions;

namespace StudentManagement.API.ExceptionHandlers;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private const string CorrelationHeader = "X-Correlation-ID";

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var correlationId = httpContext.Items[CorrelationHeader]?.ToString();
        var traceId = httpContext.TraceIdentifier;

        var (problemDetails, errorCode) = CreateProblemDetails(httpContext, exception);

        // 1. Enrich Extensions for Client Response
        problemDetails.Extensions["traceId"] = traceId;
        problemDetails.Extensions["errorCode"] = errorCode;

        if (!string.IsNullOrWhiteSpace(correlationId))
        {
            problemDetails.Extensions["correlationId"] = correlationId;
        }

        // 2. Production Structured Logging Strategy
        if (problemDetails.Status >= 500)
        {
            // 🚨 500 Server Errors: Log Full Stack Trace Exception
            _logger.LogError(
                exception,
                "Unhandled exception occurred for {Method} {Path}. TraceId: {TraceId}, ErrorCode: {ErrorCode}",
                httpContext.Request.Method,
                httpContext.Request.Path,
                traceId,
                errorCode);
        }
        else
        {
            // ⚠️ 4xx Business/Domain Errors: Log Clean Warning without noisy StackTraces
            _logger.LogWarning(
                "Request failed for {Method} {Path}. StatusCode: {StatusCode}, ErrorCode: {ErrorCode}, Detail: {Detail}, TraceId: {TraceId}",
                httpContext.Request.Method,
                httpContext.Request.Path,
                problemDetails.Status,
                errorCode,
                problemDetails.Detail,
                traceId);
        }

        httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private static (ProblemDetails Problem, string ErrorCode) CreateProblemDetails(
        HttpContext httpContext,
        Exception exception)
    {
        return exception switch
        {
            ValidationException validationException =>
                (CreateValidationProblemDetails(httpContext, validationException), "VALIDATION_FAILED"),

            StudentNotFoundException =>
                (CreateProblemDetails(httpContext, StatusCodes.Status404NotFound, "Student Not Found", exception.Message), "STUDENT_NOT_FOUND"),

            KeyNotFoundException =>
                (CreateProblemDetails(httpContext, StatusCodes.Status404NotFound, "Resource Not Found", exception.Message), "RESOURCE_NOT_FOUND"),

            UnauthorizedAccessException =>
                (CreateProblemDetails(httpContext, StatusCodes.Status401Unauthorized, "Unauthorized", exception.Message), "UNAUTHORIZED"),

            ForbiddenException =>
                (CreateProblemDetails(httpContext, StatusCodes.Status403Forbidden, "Forbidden", exception.Message), "FORBIDDEN"),

            StudentConcurrencyException =>
                (CreateProblemDetails(httpContext, StatusCodes.Status409Conflict, "Concurrency Conflict", exception.Message), "CONCURRENCY_CONFLICT"),

            CourseFullException =>
                (CreateProblemDetails(httpContext, StatusCodes.Status400BadRequest, "Course Full", exception.Message), "COURSE_FULL"),

            DuplicateEnrollmentException =>
                (CreateProblemDetails(httpContext, StatusCodes.Status409Conflict, "Duplicate Enrollment", exception.Message), "DUPLICATE_ENROLLMENT"),

            _ =>
                (CreateProblemDetails(httpContext, StatusCodes.Status500InternalServerError, "Internal Server Error", "An unexpected error occurred."), "INTERNAL_SERVER_ERROR")
        };
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

    private static ValidationProblemDetails CreateValidationProblemDetails(
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
            Title = "Validation Failed",
            Detail = "One or more validation errors occurred.",
            Instance = httpContext.Request.Path
        };
    }
}