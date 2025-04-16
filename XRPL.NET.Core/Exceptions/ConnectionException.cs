namespace XRPL.NET.Core.Exceptions;

/// <summary>
/// Exception thrown when there's an issue connecting to an XRP Ledger server.
/// </summary>
public class ConnectionException : XrplException
{
    /// <summary>
    /// Gets the server URI that failed to connect.
    /// </summary>
    public Uri? ServerUri { get; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionException"/> class.
    /// </summary>
    public ConnectionException() : base() { }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionException"/> class with a specific message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public ConnectionException(string message) : base(message) { }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionException"/> class with a
    /// specific message and inner exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public ConnectionException(string message, Exception innerException) 
        : base(message, innerException) { }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionException"/> class with a
    /// specific message and server URI.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="serverUri">The URI of the server that failed to connect.</param>
    public ConnectionException(string message, Uri serverUri) 
        : base(message)
    {
        ServerUri = serverUri;
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionException"/> class with a
    /// specific message, server URI, and inner exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="serverUri">The URI of the server that failed to connect.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public ConnectionException(string message, Uri serverUri, Exception innerException) 
        : base(message, innerException)
    {
        ServerUri = serverUri;
    }
}