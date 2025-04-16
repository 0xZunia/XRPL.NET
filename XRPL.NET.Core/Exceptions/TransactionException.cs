namespace XRPL.NET.Core.Exceptions;

/// <summary>
/// Exception thrown when a transaction encounters an error during submission or processing.
/// </summary>
public class TransactionException : XrplException
{
    /// <summary>
    /// Gets the transaction ID of the failed transaction, if available.
    /// </summary>
    public string? TransactionId { get; }
    
    /// <summary>
    /// Gets the result code from the XRP Ledger for the failed transaction, if available.
    /// </summary>
    public string? ResultCode { get; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="TransactionException"/> class.
    /// </summary>
    public TransactionException() : base() { }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="TransactionException"/> class with a specific message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public TransactionException(string message) : base(message) { }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="TransactionException"/> class with a
    /// specific message and inner exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public TransactionException(string message, Exception innerException) 
        : base(message, innerException) { }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="TransactionException"/> class with a
    /// specific message, transaction ID, and result code.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="transactionId">The ID of the failed transaction.</param>
    /// <param name="resultCode">The result code from the XRP Ledger.</param>
    public TransactionException(string message, string transactionId, string resultCode) 
        : base(message)
    {
        TransactionId = transactionId;
        ResultCode = resultCode;
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="TransactionException"/> class with a
    /// specific message, transaction ID, result code, and inner exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="transactionId">The ID of the failed transaction.</param>
    /// <param name="resultCode">The result code from the XRP Ledger.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public TransactionException(string message, string transactionId, string resultCode, Exception innerException) 
        : base(message, innerException)
    {
        TransactionId = transactionId;
        ResultCode = resultCode;
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="TransactionException"/> class with a
    /// specific message, error code, transaction ID, and result code.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="errorCode">The XRP Ledger error code.</param>
    /// <param name="transactionId">The ID of the failed transaction.</param>
    /// <param name="resultCode">The result code from the XRP Ledger.</param>
    public TransactionException(string message, string errorCode, string transactionId, string resultCode) 
        : base(message, errorCode)
    {
        TransactionId = transactionId;
        ResultCode = resultCode;
    }
}