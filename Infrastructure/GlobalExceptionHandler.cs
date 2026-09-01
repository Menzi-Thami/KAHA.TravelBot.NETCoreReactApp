using KAHA.TravelBot.NETCoreReactApp.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace KAHA.TravelBot.NETCoreReactApp.Infrastructure;

/// <summary>
/// Central exception-to-HTTP mapping. Typed exceptions thrown by services and
/// clients are translated to the right status code here, so controllers stay
/// free of try/catch boilerplate and no error is ever silently swallowed.
/// </summary>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(
        IProblemDetailsService problemDetailsService,
        ILogger<GlobalExceptionHandler> logger)
    {
        _problemDetailsService = problemDetailsService;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (status, title) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
            ExternalApiException => (StatusCodes.Status502BadGateway, "Upstream service error"),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };

        if (status >= 500)
            _logger.LogError(exception, "Unhandled exception ({Status})", status);
        else
            _logger.LogWarning(exception, "Request failed ({Status}): {Message}", status, exception.Message);

        httpContext.Response.StatusCode = status;

        // Never leak raw exception messages (which can carry connection strings,
        // stack internals, upstream URLs, etc.) to the client on server-side
        // failures. Only the controlled messages on client (4xx) errors are safe
        // to surface; everything >= 500 gets the generic title instead. The full
        // exception is still logged above for diagnostics.
        var detail = status < StatusCodes.Status500InternalServerError
            ? exception.Message
            : title;

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = detail
            }
        });
    }
}
