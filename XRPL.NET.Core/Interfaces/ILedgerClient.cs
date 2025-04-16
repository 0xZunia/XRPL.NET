using XRPL.NET.Core.Utils;

namespace XRPL.NET.Core.Interfaces;

/// <summary>
/// Provides methods for interacting with the XRP Ledger to retrieve ledger data.
/// </summary>
public interface ILedgerClient
{
    /// <summary>
    /// Gets information about an account asynchronously.
    /// </summary>
    /// <param name="accountId">The account ID (address).</param>
    /// <param name="ledgerIndex">Optional ledger index. If not specified, uses the current validated ledger.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<Result<AccountInfo>> GetAccountInfoAsync(
        string accountId, 
        uint? ledgerIndex = null, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets an account's transaction history asynchronously.
    /// </summary>
    /// <param name="accountId">The account ID (address).</param>
    /// <param name="limit">Maximum number of transactions to return.</param>
    /// <param name="marker">Pagination marker from a previous request.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<Result<AccountTransactions>> GetAccountTransactionsAsync(
        string accountId, 
        uint? limit = null, 
        object? marker = null, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets an account's balances (both XRP and issued currencies) asynchronously.
    /// </summary>
    /// <param name="accountId">The account ID (address).</param>
    /// <param name="ledgerIndex">Optional ledger index. If not specified, uses the current validated ledger.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<Result<AccountBalances>> GetAccountBalancesAsync(
        string accountId, 
        uint? ledgerIndex = null, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets an account's trust lines asynchronously.
    /// </summary>
    /// <param name="accountId">The account ID (address).</param>
    /// <param name="currency">Optional currency code to filter by.</param>
    /// <param name="limit">Maximum number of trust lines to return.</param>
    /// <param name="marker">Pagination marker from a previous request.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<Result<AccountLines>> GetAccountLinesAsync(
        string accountId, 
        string? currency = null, 
        uint? limit = null, 
        object? marker = null, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets an account's NFTs asynchronously.
    /// </summary>
    /// <param name="accountId">The account ID (address).</param>
    /// <param name="limit">Maximum number of NFTs to return.</param>
    /// <param name="marker">Pagination marker from a previous request.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<Result<AccountNfts>> GetAccountNftsAsync(
        string accountId, 
        uint? limit = null, 
        object? marker = null, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets an account's objects (escrows, offers, etc.) asynchronously.
    /// </summary>
    /// <param name="accountId">The account ID (address).</param>
    /// <param name="objectType">Optional object type to filter by.</param>
    /// <param name="limit">Maximum number of objects to return.</param>
    /// <param name="marker">Pagination marker from a previous request.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<Result<AccountObjects>> GetAccountObjectsAsync(
        string accountId, 
        string? objectType = null, 
        uint? limit = null, 
        object? marker = null, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets information about a transaction asynchronously.
    /// </summary>
    /// <param name="transactionId">The transaction ID (hash).</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<Result<TransactionInfo>> GetTransactionAsync(
        string transactionId, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a ledger asynchronously.
    /// </summary>
    /// <param name="ledgerIndex">The ledger index. If not specified, gets the latest validated ledger.</param>
    /// <param name="includeTxs">Whether to include transaction information.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<Result<LedgerInfo>> GetLedgerAsync(
        uint? ledgerIndex = null, 
        bool includeTxs = false, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a ledger asynchronously by its hash.
    /// </summary>
    /// <param name="ledgerHash">The ledger hash.</param>
    /// <param name="includeTxs">Whether to include transaction information.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<Result<LedgerInfo>> GetLedgerByHashAsync(
        string ledgerHash, 
        bool includeTxs = false, 
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents account information.
/// </summary>
public class AccountInfo
{
    /// <summary>
    /// Gets or sets the account ID (address).
    /// </summary>
    public string AccountId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the account's XRP balance.
    /// </summary>
    public decimal Balance { get; set; }
    
    /// <summary>
    /// Gets or sets the sequence number of the next valid transaction for this account.
    /// </summary>
    public uint Sequence { get; set; }
    
    /// <summary>
    /// Gets or sets the number of objects owned by this account in the ledger.
    /// </summary>
    public uint OwnerCount { get; set; }
    
    /// <summary>
    /// Gets or sets the index of the ledger that was used to retrieve this data.
    /// </summary>
    public uint LedgerIndex { get; set; }
    
    /// <summary>
    /// Gets or sets the flags set on this account.
    /// </summary>
    public uint Flags { get; set; }
    
    /// <summary>
    /// Gets or sets the domain associated with this account.
    /// </summary>
    public string? Domain { get; set; }
    
    /// <summary>
    /// Gets or sets the email address hash associated with this account.
    /// </summary>
    public string? EmailHash { get; set; }
    
    /// <summary>
    /// Gets or sets the regular key associated with this account.
    /// </summary>
    public string? RegularKey { get; set; }
    
    /// <summary>
    /// Gets or sets the transaction ID of the transaction that most recently modified this account.
    /// </summary>
    public string? PreviousTxnId { get; set; }
}

/// <summary>
/// Represents an account's transactions.
/// </summary>
public class AccountTransactions
{
    /// <summary>
    /// Gets or sets the account ID (address).
    /// </summary>
    public string AccountId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the transactions.
    /// </summary>
    public List<TransactionInfo> Transactions { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the ledger index marker for pagination.
    /// </summary>
    public object? Marker { get; set; }
}

/// <summary>
/// Represents account balances.
/// </summary>
public class AccountBalances
{
    /// <summary>
    /// Gets or sets the account ID (address).
    /// </summary>
    public string AccountId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the XRP balance.
    /// </summary>
    public decimal XrpBalance { get; set; }
    
    /// <summary>
    /// Gets or sets the issued currency balances.
    /// </summary>
    public List<IssuedCurrencyAmount> IssuedCurrencies { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the ledger index used to retrieve this data.
    /// </summary>
    public uint LedgerIndex { get; set; }
}

/// <summary>
/// Represents an issued currency amount.
/// </summary>
public class IssuedCurrencyAmount
{
    /// <summary>
    /// Gets or sets the currency code.
    /// </summary>
    public string Currency { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the issuer's account ID.
    /// </summary>
    public string Issuer { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the amount.
    /// </summary>
    public decimal Value { get; set; }
}

/// <summary>
/// Represents account trust lines.
/// </summary>
public class AccountLines
{
    /// <summary>
    /// Gets or sets the account ID (address).
    /// </summary>
    public string AccountId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the trust lines.
    /// </summary>
    public List<TrustLine> Lines { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the ledger index used to retrieve this data.
    /// </summary>
    public uint LedgerIndex { get; set; }
    
    /// <summary>
    /// Gets or sets the marker for pagination.
    /// </summary>
    public object? Marker { get; set; }
}

/// <summary>
/// Represents a trust line.
/// </summary>
public class TrustLine
{
    /// <summary>
    /// Gets or sets the account ID of the counterparty.
    /// </summary>
    public string Account { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the currency code.
    /// </summary>
    public string Currency { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the balance.
    /// </summary>
    public decimal Balance { get; set; }
    
    /// <summary>
    /// Gets or sets the limit set by the account.
    /// </summary>
    public decimal Limit { get; set; }
    
    /// <summary>
    /// Gets or sets the limit set by the counterparty.
    /// </summary>
    public decimal LimitPeer { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether the line is authorized.
    /// </summary>
    public bool Authorized { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether the counterparty's line is authorized.
    /// </summary>
    public bool PeerAuthorized { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether the account has frozen the trust line.
    /// </summary>
    public bool Freeze { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether the counterparty has frozen the trust line.
    /// </summary>
    public bool FreezePeer { get; set; }
    
    /// <summary>
    /// Gets or sets the quality in (rate at which the account accepts the currency).
    /// </summary>
    public decimal? QualityIn { get; set; }
    
    /// <summary>
    /// Gets or sets the quality out (rate at which the account provides the currency).
    /// </summary>
    public decimal? QualityOut { get; set; }
}

/// <summary>
/// Represents account NFTs.
/// </summary>
public class AccountNfts
{
    /// <summary>
    /// Gets or sets the account ID (address).
    /// </summary>
    public string AccountId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the NFTs.
    /// </summary>
    public List<Nft> Nfts { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the ledger index used to retrieve this data.
    /// </summary>
    public uint LedgerIndex { get; set; }
    
    /// <summary>
    /// Gets or sets the marker for pagination.
    /// </summary>
    public object? Marker { get; set; }
}

/// <summary>
/// Represents an NFT.
/// </summary>
public class Nft
{
    /// <summary>
    /// Gets or sets the NFT ID.
    /// </summary>
    public string TokenId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the URI of the NFT.
    /// </summary>
    public string? Uri { get; set; }
    
    /// <summary>
    /// Gets or sets the owner of the NFT.
    /// </summary>
    public string Owner { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the taxon of the NFT.
    /// </summary>
    public uint Taxon { get; set; }
    
    /// <summary>
    /// Gets or sets the NFT sequence number.
    /// </summary>
    public uint Sequence { get; set; }
    
    /// <summary>
    /// Gets or sets the flags of the NFT.
    /// </summary>
    public uint Flags { get; set; }
    
    /// <summary>
    /// Gets or sets the transaction ID that minted this NFT.
    /// </summary>
    public string? MintTxnId { get; set; }
}

/// <summary>
/// Represents account objects.
/// </summary>
public class AccountObjects
{
    /// <summary>
    /// Gets or sets the account ID (address).
    /// </summary>
    public string AccountId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the account objects.
    /// </summary>
    public List<object> Objects { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the ledger index used to retrieve this data.
    /// </summary>
    public uint LedgerIndex { get; set; }
    
    /// <summary>
    /// Gets or sets the marker for pagination.
    /// </summary>
    public object? Marker { get; set; }
}

/// <summary>
/// Represents transaction information.
/// </summary>
public class TransactionInfo
{
    /// <summary>
    /// Gets or sets the transaction ID (hash).
    /// </summary>
    public string TransactionId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the ledger index where this transaction appears.
    /// </summary>
    public uint LedgerIndex { get; set; }
    
    /// <summary>
    /// Gets or sets the transaction data.
    /// </summary>
    public object Transaction { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the transaction metadata.
    /// </summary>
    public object? Meta { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether the transaction is validated.
    /// </summary>
    public bool Validated { get; set; }
    
    /// <summary>
    /// Gets or sets the date when the transaction was included in a ledger.
    /// </summary>
    public DateTime? Date { get; set; }
}

/// <summary>
/// Represents ledger information.
/// </summary>
public class LedgerInfo
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
    /// Gets or sets the parent ledger hash.
    /// </summary>
    public string ParentLedgerHash { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the ledger's close time in Ripple Epoch format.
    /// </summary>
    public uint CloseTime { get; set; }
    
    /// <summary>
    /// Gets or sets the close time as a DateTime.
    /// </summary>
    public DateTime CloseTimeUtc { get; set; }
    
    /// <summary>
    /// Gets or sets the close time resolution in seconds.
    /// </summary>
    public uint CloseTimeResolution { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether the ledger is closed.
    /// </summary>
    public bool Closed { get; set; }
    
    /// <summary>
    /// Gets or sets the total number of XRP drops in existence.
    /// </summary>
    public string TotalCoins { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the transactions in this ledger.
    /// </summary>
    public List<TransactionInfo>? Transactions { get; set; }
}