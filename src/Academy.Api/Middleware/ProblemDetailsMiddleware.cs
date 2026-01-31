using System.Text.Json;
using System.Text.Json.Serialization;
using Academy.Application.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Academy.Api.Middleware;

public sealed class ProblemDetailsMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly RequestDelegate _next;
    private readonly ILogger<ProblemDetailsMiddleware> _logger;

    public ProblemDetailsMiddleware(RequestDelegate next, ILogger<ProblemDetailsMiddleware> logger)
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
            if (context.Response.HasStarted)
            {
                throw;
            }

            var traceId = context.TraceIdentifier;
            _logger.LogError(ex, "Unhandled exception. TraceId: {TraceId}", traceId);

            context.Response.Clear();
            if (ex is ValidationException validationException)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                var problemDetails = CreateValidationProblemDetails(context, validationException, traceId);
                await context.Response.WriteAsJsonAsync(problemDetails, JsonOptions, "application/problem+json");
                return;
            }

            var statusCode = GetStatusCode(ex);
            context.Response.StatusCode = statusCode;

            var problem = new ProblemDetails
            {
                Type = GetTypeForStatusCode(statusCode),
                Title = GetTitleForStatusCode(statusCode),
                Status = statusCode,
                Detail = GetDetail(ex, statusCode),
                Instance = context.Request.Path
            };

            problem.Extensions["traceId"] = traceId;

            await context.Response.WriteAsJsonAsync(problem, JsonOptions, "application/problem+json");
        }
    }

    private static ValidationProblemDetails CreateValidationProblemDetails(
        HttpContext context,
        ValidationException exception,
        string traceId)
    {
        var errors = exception.Errors
            .GroupBy(e => string.IsNullOrWhiteSpace(e.PropertyName) ? string.Empty : e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

        var details = new ValidationProblemDetails(errors)
        {
            Type = GetTypeForStatusCode(StatusCodes.Status400BadRequest),
            Title = "Validation failed",
            Status = StatusCodes.Status400BadRequest,
            Detail = "One or more validation errors occurred.",
            Instance = context.Request.Path
        };

        details.Extensions["traceId"] = traceId;
        return details;
    }

    private static int GetStatusCode(Exception ex)
    {
        if (ex is TenantScopeException || ex is ForbiddenException || ex is UnauthorizedAccessException)
        {
            return StatusCodes.Status403Forbidden;
        }

        if (ex is NotFoundException || ex is KeyNotFoundException)
        {
            return StatusCodes.Status404NotFound;
        }

        if (IsClientError(ex))
        {
            return StatusCodes.Status400BadRequest;
        }

        return StatusCodes.Status500InternalServerError;
    }

    private static bool IsClientError(Exception ex)
    {
        if (ex is ArgumentException)
        {
            return true;
        }

        if (ex is InvalidOperationException && ex.Data.Contains("IsClientError"))
        {
            return true;
        }

        return false;
    }

    private static string GetTypeForStatusCode(int statusCode)
        => $"https://httpstatuses.com/{statusCode}";

    private static string GetTitleForStatusCode(int statusCode)
        => statusCode switch
        {
            StatusCodes.Status400BadRequest => "Bad request",
            StatusCodes.Status403Forbidden => "Forbidden",
            StatusCodes.Status404NotFound => "Not found",
            StatusCodes.Status500InternalServerError => "Unexpected error",
            _ => "Error"
        };

    private static string GetDetail(Exception ex, int statusCode)
    {
        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            return "An unexpected error occurred.";
        }

        if (statusCode == StatusCodes.Status404NotFound)
        {
            return string.IsNullOrWhiteSpace(ex.Message) ? "Resource not found." : ex.Message;
        }

        return string.IsNullOrWhiteSpace(ex.Message) ? "Request could not be processed." : ex.Message;
    }
}
