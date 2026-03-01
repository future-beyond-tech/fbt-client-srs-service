using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace SRS.API.Middleware;

/// <summary>
/// Returns RFC 7807 ProblemDetails for exceptions. Does not log PII (phone/address).
/// </summary>
public sealed class ProblemDetailsExceptionMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    private readonly RequestDelegate _next;
    private readonly ILogger<ProblemDetailsExceptionMiddleware> _logger;

    public ProblemDetailsExceptionMiddleware(
        RequestDelegate next,
        ILogger<ProblemDetailsExceptionMiddleware> logger)
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
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var (statusCode, title, detail) = ex switch
        {
            KeyNotFoundException => (HttpStatusCode.NotFound, "Not Found", ex.Message),
            ArgumentException => (HttpStatusCode.BadRequest, "Bad Request", ex.Message),
            InvalidOperationException => (HttpStatusCode.Conflict, "Conflict", ex.Message),
            ApplicationException => (HttpStatusCode.BadGateway, "Bad Gateway", ex.Message),
            _ => (HttpStatusCode.InternalServerError, "An error occurred", "An unexpected error occurred.")
        };

        if (statusCode == HttpStatusCode.InternalServerError)
            _logger.LogError(ex, "Unhandled exception. {ExceptionType}", ex.GetType().Name);
        else
            _logger.LogWarning("Request failed with {StatusCode}: {Title}", (int)statusCode, title);

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;

        var problem = new ProblemDetails
        {
            Type = GetProblemType(statusCode),
            Title = title,
            Status = (int)statusCode,
            Detail = detail,
            Instance = context.Request.Path
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(problem, JsonOptions));
    }

    private static string GetProblemType(HttpStatusCode statusCode)
    {
        return statusCode switch
        {
            HttpStatusCode.NotFound => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            HttpStatusCode.BadRequest => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            HttpStatusCode.Conflict => "https://tools.ietf.org/html/rfc7231#section-6.5.8",
            HttpStatusCode.BadGateway => "https://tools.ietf.org/html/rfc7231#section-6.6.3",
            _ => "https://tools.ietf.org/html/rfc7231#section-6.6.1"
        };
    }
}
