using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc;

namespace Academy.Api.Middleware;

public sealed class RateLimitingMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly RequestDelegate _next;
    private readonly PartitionedRateLimiter<HttpContext> _limiter;

    public RateLimitingMiddleware(RequestDelegate next, PartitionedRateLimiter<HttpContext> limiter)
    {
        _next = next;
        _limiter = limiter;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        using var lease = await _limiter.AcquireAsync(context, 1, context.RequestAborted);
        if (lease.IsAcquired)
        {
            await _next(context);
            return;
        }

        var problemDetails = new ProblemDetails
        {
            Type = "https://httpstatuses.com/429",
            Title = "Too many requests",
            Status = StatusCodes.Status429TooManyRequests,
            Detail = "Rate limit exceeded.",
            Instance = context.Request.Path
        };

        problemDetails.Extensions["traceId"] = context.TraceIdentifier;

        context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.Response.WriteAsJsonAsync(
            problemDetails,
            JsonOptions,
            "application/problem+json");
    }
}
