namespace XRPL.NET.Core.Exceptions;

/// <summary>
/// Base exception type for all XRP Ledger SDK exceptions.
/// </summary>
/// <remarks>
/// All exceptions thrown by the SDK should derive from this class,
/// allowing client code to catch and handle SDK-specific exceptions.
/// </remarks>
public class XrplException : Exception
{
    /// <summary>
    /// Gets the XRP Ledger error code, if applicable.
    /// </summary>
    /// <remarks>
    /// This property will be set when the exception represents an error returned
    /// by the XRP Ledger itself. For SDK-internal errors, this may be null.
    /// </remarks>
    public string? ErrorCode { get; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="XrplException"/> class.
    /// </summary>
    public XrplException() : base() { }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="XrplException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public XrplException(string message) : base(message) { }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="XrplException"/> class with a specified error message
    /// and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public XrplException(string message, Exception innerException) 
        : base(message, innerException) { }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="XrplException"/> class with a specified error message
    /// and XRP Ledger error code.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="errorCode">The XRP Ledger specific error code.</param>
    public XrplException(string message, string errorCode) 
        : base(message)
    {
        ErrorCode = errorCode;
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="XrplException"/> class with a specified error message,
    /// XRP Ledger error code, and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="errorCode">The XRP Ledger specific error code.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public XrplException(string message, string errorCode, Exception innerException) 
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}