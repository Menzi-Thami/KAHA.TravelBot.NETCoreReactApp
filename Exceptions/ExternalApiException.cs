namespace KAHA.TravelBot.NETCoreReactApp.Exceptions;

/// <summary>
/// Thrown when a downstream/external API call fails (bad status, unreadable
/// payload, etc.). Translated to an HTTP 502 by <c>GlobalExceptionHandler</c>.
/// </summary>
public sealed class ExternalApiException : Exception
{
    public ExternalApiException(string message) : base(message) { }

    public ExternalApiException(string message, Exception innerException)
        : base(message, innerException) { }
}
