using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

namespace SRS.API.Filters;

/// <summary>
/// Converts FluentValidation ValidationException to RFC 7807 ProblemDetails response.
/// </summary>
public sealed class ValidationProblemDetailsFilter : IExceptionFilter
{
    private readonly ILogger<ValidationProblemDetailsFilter> _logger;

    public ValidationProblemDetailsFilter(ILogger<ValidationProblemDetailsFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        if (context.Exception is not ValidationException validationException)
            return;

        context.ExceptionHandled = true;
        _logger.LogDebug("Validation failed for request.");

        var errors = validationException.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

        var problem = new ValidationProblemDetails(errors)
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = "One or more validation errors occurred.",
            Status = (int)HttpStatusCode.BadRequest,
            Instance = context.HttpContext.Request.Path
        };

        context.Result = new ObjectResult(problem)
        {
            StatusCode = (int)HttpStatusCode.BadRequest,
            ContentTypes = { "application/problem+json" }
        };
    }
}
