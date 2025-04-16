using XRPL.NET.Core.Utils;

namespace XRPL.NET.Core.Interfaces;

/// <summary>
/// Provides methods for submitting and tracking transactions on the XRP Ledger.
/// </summary>
public interface ITransactionClient
{
    /// <summary>
    /// Submits a signed transaction to the XRP Ledger asynchronously.
    /// </summary>
    /// <param name="signedTransaction">The signed transaction in blob format.</param>
    /// <param name="failHard">If true, the transaction will not be applied to the open ledger if it fails the preliminary checks.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<Result<SubmitResult>> SubmitTransactionAsync(
        string signedTransaction, 
        bool failHard = false, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Submits a transaction to be signed and submitted to the XRP Ledger asynchronously.
    /// </summary>
    /// <param name="transaction">The transaction object.</param>
    /// <param name="secret">The secret key to sign the transaction with.</param>
    /// <param name="failHard">If true, the transaction will not be applied to the open ledger if it fails the preliminary checks.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<Result<SubmitResult>> SubmitAsync(
        object transaction, 
        string secret, 
        bool failHard = false, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the status of a transaction asynchronously.
    /// </summary>
    /// <param name="transactionId">The transaction ID (hash).</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<Result<TransactionStatus>> GetTransactionStatusAsync(
        string transactionId, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Waits for a transaction to be validated asynchronously.
    /// </summary>
    /// <param name="transactionId">The transaction ID (hash).</param>
    /// <param name="timeout">The maximum time to wait for the transaction to be validated.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<Result<TransactionStatus>> WaitForTransactionAsync(
        string transactionId, 
        TimeSpan? timeout = null, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verifies that a transaction exists on the ledger asynchronously.
    /// </summary>
    /// <param name="transactionId">The transaction ID (hash).</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<Result<bool>> VerifyTransactionAsync(
        string transactionId, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Calculates a transaction fee asynchronously.
    /// </summary>
    /// <param name="transaction">The transaction object.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<Result<ulong>> CalculateFeeAsync(
        object transaction, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Estimates the transaction fee for the current network conditions asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<Result<FeeEstimate>> EstimateFeeAsync(
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents the result of submitting a transaction.
/// </summary>
public class SubmitResult
{
    /// <summary>
    /// Gets or sets the transaction ID (hash).
    /// </summary>
    public string TransactionId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the preliminary result of the transaction.
    /// </summary>
    public string EngineResult { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the human-readable explanation of the preliminary result.
    /// </summary>
    public string EngineResultMessage { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets a value indicating whether the transaction was successfully submitted to the network.
    /// </summary>
    /// <remarks>
    /// Note that this does not guarantee that the transaction will be validated and included in a ledger.
    /// </remarks>
    public bool Accepted { get; set; }
    
    /// <summary>
    /// Gets or sets the sequence number used by the transaction.
    /// </summary>
    public uint? Sequence { get; set; }
    
    /// <summary>
    /// Gets or sets the fee charged for the transaction in drops.
    /// </summary>
    public ulong? Fee { get; set; }
}

/// <summary>
/// Represents the status of a transaction.
/// </summary>
public class TransactionStatus
{
    /// <summary>
    /// Gets or sets the transaction ID (hash).
    /// </summary>
    public string TransactionId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets a value indicating whether the transaction has been validated.
    /// </summary>
    public bool Validated { get; set; }
    
    /// <summary>
    /// Gets or sets the result of the transaction.
    /// </summary>
    public string? Result { get; set; }
    
    /// <summary>
    /// Gets or sets the human-readable explanation of the result.
    /// </summary>
    public string? ResultMessage { get; set; }
    
    /// <summary>
    /// Gets or sets the ledger index where the transaction was included, if validated.
    /// </summary>
    public uint? LedgerIndex { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether the transaction is included in a ledger.
    /// </summary>
    public bool InLedger { get; set; }
    
    /// <summary>
    /// Gets or sets the date when the transaction was validated, if available.
    /// </summary>
    public DateTime? Date { get; set; }
    
    /// <summary>
    /// Gets a value indicating whether the transaction was successful.
    /// </summary>
    public bool IsSuccessful => Validated && Result == "tesSUCCESS";
}

/// <summary>
/// Represents a fee estimate.
/// </summary>
public class FeeEstimate
{
    /// <summary>
    /// Gets or sets the minimum fee in drops.
    /// </summary>
    public ulong MinimumFee { get; set; }
    
    /// <summary>
    /// Gets or sets the median fee in drops.
    /// </summary>
    public ulong MedianFee { get; set; }
    
    /// <summary>
    /// Gets or sets the open ledger fee in drops.
    /// </summary>
    public ulong OpenLedgerFee { get; set; }
    
    /// <summary>
    /// Gets or sets the recommended fee for standard transactions in drops.
    /// </summary>
    /// <remarks>
    /// This is typically the open ledger fee multiplied by the configured fee multiplier.
    /// </remarks>
    public ulong RecommendedFee { get; set; }
}