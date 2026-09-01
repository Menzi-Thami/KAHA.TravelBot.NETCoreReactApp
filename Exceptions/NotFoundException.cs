namespace KAHA.TravelBot.NETCoreReactApp.Exceptions;

/// <summary>
/// Thrown when a requested resource cannot be found. Translated to an
/// HTTP 404 by <c>GlobalExceptionHandler</c> — services never return null
/// to signal "not found".
/// </summary>
public sealed class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}
