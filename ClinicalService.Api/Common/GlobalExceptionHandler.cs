using System.Diagnostics;
using ClinicalGrpcService.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace ClinicalService.Api.Common;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (status, title, detail) = exception switch
        {
            // Duplicate MRN etc.: expected business case. The message never holds patient data (ConflictException rule)
            ConflictException => (StatusCodes.Status409Conflict, "Conflict", exception.Message),

            // Bad input that got past validation (e.g. empty organization GUID, paging limits)
            ArgumentException => (StatusCodes.Status400BadRequest, "Invalid request", exception.Message),

            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.", null)
        };

        if (status == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception in {Path}", httpContext.Request.Path);
            Activity.Current?.SetStatus(ActivityStatusCode.Error, exception.Message);
            Activity.Current?.AddException(exception);
        }
        else
        {
            // Client errors: warning, no stack trace, span not marked as failed
            logger.LogWarning("{ExceptionType} in {Path}", exception.GetType().Name, httpContext.Request.Path);
            Activity.Current?.SetTag("error.type", exception.GetType().Name);
        }

        httpContext.Response.StatusCode = status;
        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Title = title,
            Detail = detail,
            Status = status
        }, cancellationToken);

        return true;
    }
}