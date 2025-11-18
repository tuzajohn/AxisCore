namespace AxisCore.Mediator.Exceptions;

/// <summary>
/// Exception thrown when a handler cannot be found for a request.
/// </summary>
public class HandlerNotFoundException : Exception
{
    /// <summary>
    /// Gets the request type that had no handler.
    /// </summary>
    public Type RequestType { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="HandlerNotFoundException"/> class.
    /// </summary>
    /// <param name="requestType">The request type</param>
    public HandlerNotFoundException(Type requestType)
        : base($"No handler registered for request type '{requestType.FullName}'")
    {
        RequestType = requestType;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HandlerNotFoundException"/> class.
    /// </summary>
    /// <param name="requestType">The request type</param>
    /// <param name="message">Custom error message</param>
    public HandlerNotFoundException(Type requestType, string message)
        : base(message)
    {
        RequestType = requestType;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HandlerNotFoundException"/> class.
    /// </summary>
    /// <param name="requestType">The request type</param>
    /// <param name="message">Custom error message</param>
    /// <param name="innerException">Inner exception</param>
    public HandlerNotFoundException(Type requestType, string message, Exception innerException)
        : base(message, innerException)
    {
        RequestType = requestType;
    }
}
