using FluentValidation;
using System.Net;
using System.Text.Json;

namespace CollectA.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message, errors) = exception switch
        {
            ValidationException validationException => (
                HttpStatusCode.BadRequest,
                "Erreur de validation.",
                validationException.Errors.Select(e => e.ErrorMessage).ToArray()),
            KeyNotFoundException notFoundException => (
                HttpStatusCode.NotFound,
                notFoundException.Message,
                Array.Empty<string>()),
            InvalidOperationException invalidOperationException => (
                HttpStatusCode.Conflict,
                invalidOperationException.Message,
                Array.Empty<string>()),
            _ => (
                HttpStatusCode.InternalServerError,
                "Une erreur interne est survenue.",
                Array.Empty<string>())
        };

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Erreur interne sur {Path}", context.Request.Path);
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var payload = JsonSerializer.Serialize(new
        {
            message,
            errors
        });

        await context.Response.WriteAsync(payload);
    }
}
