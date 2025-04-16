namespace XRPL.NET.Core.Exceptions;

/// <summary>
/// Exception thrown when transaction validation fails.
/// </summary>
public class ValidationException : XrplException
{
    /// <summary>
    /// Gets the validation errors that caused this exception.
    /// </summary>
    public IReadOnlyDictionary<string, string>? ValidationErrors { get; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class.
    /// </summary>
    public ValidationException() : base() { }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class with a specific message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public ValidationException(string message) : base(message) { }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class with a
    /// specific message and inner exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public ValidationException(string message, Exception innerException) 
        : base(message, innerException) { }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class with a
    /// specific message and validation errors.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="validationErrors">A dictionary of field names and associated error messages.</param>
    public ValidationException(string message, Dictionary<string, string> validationErrors) 
        : base(message)
    {
        ValidationErrors = validationErrors;
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class with a
    /// specific message, validation errors, and inner exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="validationErrors">A dictionary of field names and associated error messages.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public ValidationException(string message, Dictionary<string, string> validationErrors, Exception innerException) 
        : base(message, innerException)
    {
        ValidationErrors = validationErrors;
    }
}