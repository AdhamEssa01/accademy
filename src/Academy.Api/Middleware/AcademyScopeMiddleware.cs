using System.Text.Json;
using System.Text.Json.Serialization;
using Academy.Api.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Academy.Api.Middleware;

public sealed class AcademyScopeMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly RequestDelegate _next;

    public AcademyScopeMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var academyId = context.User.GetAcademyId();
            if (!academyId.HasValue)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                var problem = new ProblemDetails
                {
                    Title = "Missing academy scope",
                    Status = StatusCodes.Status403Forbidden,
                    Type = "https://httpstatuses.com/403",
                    Detail = "Academy scope is required for authenticated requests.",
                    Instance = context.Request.Path
                };
                problem.Extensions["traceId"] = context.TraceIdentifier;

                await context.Response.WriteAsJsonAsync(problem, JsonOptions, "application/problem+json");
                return;
            }
        }

        await _next(context);
    }
}