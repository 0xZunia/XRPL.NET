using XRPL.NET.Core.Utils;

namespace XRPL.NET.Core.Interfaces;

/// <summary>
/// Provides methods for subscribing to real-time events on the XRP Ledger.
/// </summary>
public interface ISubscriptionClient
{
    /// <summary>
    /// Gets a value indicating whether the subscription client is connected.
    /// </summary>
    bool IsConnected { get; }
    
    /// <summary>
    /// Gets a value indicating whether the client is currently subscribed to any streams.
    /// </summary>
    bool IsSubscribed { get; }
    
    /// <summary>
    /// Gets the list of streams currently subscribed to.
    /// </summary>
    IReadOnlyList<string> ActiveSubscriptions { get; }
    
    /// <summary>
    /// Event raised when a ledger closes.
    /// </summary>
    event EventHandler<LedgerClosedEventArgs>? LedgerClosed;
    
    /// <summary>
    /// Event raised when a validated transaction is received.
    /// </summary>
    event EventHandler<TransactionEventArgs>? TransactionReceived;
    
    /// <summary>
    /// Event raised when a validation message is received.
    /// </summary>
    event EventHandler<ValidationEventArgs>? ValidationReceived;
    
    /// <summary>
    /// Event raised when a path finding update is received.
    /// </summary>
    event EventHandler<PathFindEventArgs>? PathFindUpdate;
    
    /// <summary>
    /// Event raised when an order book update is received.
    /// </summary>
    event EventHandler<OrderBookEventArgs>? OrderBookUpdate;
    
    /// <summary>
    /// Subscribes to one or more streams asynchronously.
    /// </summary>
    /// <param name="streams">The streams to subscribe to.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<Result<bool>> SubscribeAsync(
        IEnumerable<SubscriptionStream> streams, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Subscribes to the ledger stream asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<Result<bool>> SubscribeLedgerAsync(
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Subscribes to transactions for a specific account asynchronously.
    /// </summary>
    /// <param name="accountId">The account ID to subscribe to.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<Result<bool>> SubscribeAccountAsync(
        string accountId, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Subscribes to transactions for multiple accounts asynchronously.
    /// </summary>
    /// <param name="accountIds">The account IDs to subscribe to.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<Result<bool>> SubscribeAccountsAsync(
        IEnumerable<string> accountIds, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Subscribes to an order book asynchronously.
    /// </summary>
    /// <param name="baseCurrency">The base currency code.</param>
    /// <param name="baseIssuer">The base currency issuer (not needed for XRP).</param>
    /// <param name="quoteCurrency">The quote currency code.</param>
    /// <param name="quoteIssuer">The quote currency issuer (not needed for XRP).</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<Result<bool>> SubscribeOrderBookAsync(
        string baseCurrency, 
        string? baseIssuer, 
        string quoteCurrency, 
        string? quoteIssuer, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Unsubscribes from one or more streams asynchronously.
    /// </summary>
    /// <param name="streams">The streams to unsubscribe from.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<Result<bool>> UnsubscribeAsync(
        IEnumerable<SubscriptionStream> streams, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Unsubscribes from all streams asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<Result<bool>> UnsubscribeAllAsync(
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents the different subscription streams available.
/// </summary>
public enum SubscriptionStream
{
    /// <summary>
    /// Subscribe to ledger close events.
    /// </summary>
    Ledger,
    
    /// <summary>
    /// Subscribe to all validated transactions.
    /// </summary>
    Transactions,
    
    /// <summary>
    /// Subscribe to all validations.
    /// </summary>
    Validations,
    
    /// <summary>
    /// Subscribe to pathfinding information.
    /// </summary>
    PathFind,
    
    /// <summary>
    /// Subscribe to order book changes.
    /// </summary>
    OrderBook
}

/// <summary>
/// Event arguments for ledger closed events.
/// </summary>
public class LedgerClosedEventArgs : EventArgs
{
    /// <summary>
    /// Gets or sets the ledger index.
    /// </summary>
    public uint LedgerIndex { get; set; }
    
    /// <summary>
    /// Gets or sets the ledger hash.
    /// </summary>
    public string LedgerHash { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the ledger close time as a DateTime.
    /// </summary>
    public DateTime CloseTime { get; set; }
    
    /// <summary>
    /// Gets or sets the total coins in the ledger.
    /// </summary>
    public string TotalCoins { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the reserve base.
    /// </summary>
    public ulong ReserveBase { get; set; }
    
    /// <summary>
    /// Gets or sets the reserve increment.
    /// </summary>
    public ulong ReserveIncrement { get; set; }
    
    /// <summary>
    /// Gets or sets the fee base.
    /// </summary>
    public ulong FeeBase { get; set; }
}

/// <summary>
/// Event arguments for transaction events.
/// </summary>
public class TransactionEventArgs : EventArgs
{
    /// <summary>
    /// Gets or sets the transaction.
    /// </summary>
    public object Transaction { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the transaction metadata.
    /// </summary>
    public object? Meta { get; set; }
    
    /// <summary>
    /// Gets or sets the engine result.
    /// </summary>
    public string EngineResult { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the transaction ID.
    /// </summary>
    public string TransactionId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the ledger index.
    /// </summary>
    public uint LedgerIndex { get; set; }
    
    /// <summary>
    /// Gets or sets the affected account.
    /// </summary>
    public string? Account { get; set; }
    
    /// <summary>
    /// Gets or sets the type of the transaction.
    /// </summary>
    public string? TransactionType { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether the transaction is validated.
    /// </summary>
    public bool Validated { get; set; }
}

/// <summary>
/// Event arguments for validation events.
/// </summary>
public class ValidationEventArgs : EventArgs
{
    /// <summary>
    /// Gets or sets the ledger index.
    /// </summary>
    public uint LedgerIndex { get; set; }
    
    /// <summary>
    /// Gets or sets the ledger hash.
    /// </summary>
    public string LedgerHash { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the validator public key.
    /// </summary>
    public string ValidatorPublicKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the time of the validation.
    /// </summary>
    public DateTime ValidationTime { get; set; }
}

/// <summary>
/// Event arguments for path finding events.
/// </summary>
public class PathFindEventArgs : EventArgs
{
    /// <summary>
    /// Gets or sets the source account.
    /// </summary>
    public string SourceAccount { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the destination account.
    /// </summary>
    public string DestinationAccount { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the destination amount.
    /// </summary>
    public object DestinationAmount { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the list of paths.
    /// </summary>
    public List<object> Paths { get; set; } = new();
}

/// <summary>
/// Event arguments for order book events.
/// </summary>
public class OrderBookEventArgs : EventArgs
{
    /// <summary>
    /// Gets or sets the base currency.
    /// </summary>
    public string BaseCurrency { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the base issuer.
    /// </summary>
    public string? BaseIssuer { get; set; }
    
    /// <summary>
    /// Gets or sets the quote currency.
    /// </summary>
    public string QuoteCurrency { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the quote issuer.
    /// </summary>
    public string? QuoteIssuer { get; set; }
    
    /// <summary>
    /// Gets or sets the list of bids.
    /// </summary>
    public List<Order> Bids { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the list of asks.
    /// </summary>
    public List<Order> Asks { get; set; } = new();
}

/// <summary>
/// Represents an order in the order book.
/// </summary>
public class Order
{
    /// <summary>
    /// Gets or sets the account placing the order.
    /// </summary>
    public string Account { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the price of the order.
    /// </summary>
    public decimal Price { get; set; }
    
    /// <summary>
    /// Gets or sets the amount of the order.
    /// </summary>
    public decimal Amount { get; set; }
    
    /// <summary>
    /// Gets or sets the offer ID.
    /// </summary>
    public string OfferId { get; set; } = string.Empty;
}