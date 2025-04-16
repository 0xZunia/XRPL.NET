using XRPL.NET.Core.Configuration;

namespace XRPL.NET.Core.Interfaces;

/// <summary>
/// Represents the main interface for interacting with the XRP Ledger.
/// </summary>
public interface IXrplClient : IDisposable
{
    /// <summary>
    /// Gets the ledger client for accessing ledger data.
    /// </summary>
    ILedgerClient Ledger { get; }
    
    /// <summary>
    /// Gets the transaction client for submitting and tracking transactions.
    /// </summary>
    ITransactionClient Transactions { get; }
    
    /// <summary>
    /// Gets the subscription client for subscribing to ledger events.
    /// </summary>
    ISubscriptionClient Subscriptions { get; }
    
    /// <summary>
    /// Gets the client options.
    /// </summary>
    XrplClientOptions Options { get; }
    
    /// <summary>
    /// Gets a value indicating whether the client is connected to a server.
    /// </summary>
    bool IsConnected { get; }
    
    /// <summary>
    /// Gets the URI of the currently connected server.
    /// </summary>
    Uri? CurrentServerUri { get; }
    
    /// <summary>
    /// Connects to the XRP Ledger asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ConnectAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Disconnects from the XRP Ledger asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DisconnectAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Reconnects to the XRP Ledger asynchronously, potentially using a different server.
    /// </summary>
    /// <param name="forceSwitch">If true, forces switching to a different server if available.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ReconnectAsync(bool forceSwitch = false, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the server information asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<ServerInfoResponse> GetServerInfoAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the current fee statistics asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<FeeResponse> GetFeeAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Sends a raw command to the server asynchronously.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="command">The command to send.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<TResponse> SendCommandAsync<TResponse>(object command, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents the server information response.
/// </summary>
public class ServerInfoResponse
{
    /// <summary>
    /// Gets or sets the build version.
    /// </summary>
    public string BuildVersion { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the complete ledgers range.
    /// </summary>
    public string CompleteLedgers { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the time the server has been online.
    /// </summary>
    public TimeSpan Uptime { get; set; }
    
    /// <summary>
    /// Gets or sets the validator public key.
    /// </summary>
    public string? ValidatorPublicKey { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether the server is a validator.
    /// </summary>
    public bool IsValidator { get; set; }
    
    /// <summary>
    /// Gets or sets the current load factor.
    /// </summary>
    public decimal LoadFactor { get; set; }
    
    /// <summary>
    /// Gets or sets the host ID.
    /// </summary>
    public string? HostId { get; set; }
    
    /// <summary>
    /// Gets or sets the current validated ledger index.
    /// </summary>
    public uint ValidatedLedgerIndex { get; set; }
    
    /// <summary>
    /// Gets or sets the current ledger index.
    /// </summary>
    public uint CurrentLedgerIndex { get; set; }
}

/// <summary>
/// Represents the fee response.
/// </summary>
public class FeeResponse
{
    /// <summary>
    /// Gets or sets the current base fee in drops.
    /// </summary>
    public ulong BaseFee { get; set; }
    
    /// <summary>
    /// Gets or sets the current minimum fee in drops.
    /// </summary>
    public ulong MinimumFee { get; set; }
    
    /// <summary>
    /// Gets or sets the current median fee in drops.
    /// </summary>
    public ulong MedianFee { get; set; }
    
    /// <summary>
    /// Gets or sets the current open ledger fee in drops.
    /// </summary>
    public ulong OpenLedgerFee { get; set; }
}