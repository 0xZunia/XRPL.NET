using System.Text.Json.Serialization;
using XRPL.NET.Models.Transactions.Common;

namespace XRPL.NET.Models.Transactions;

/// <summary>
/// Base class for all XRP Ledger transactions.
/// </summary>
public abstract class TransactionBase
{
    /// <summary>
    /// Gets the transaction type.
    /// </summary>
    [JsonPropertyName("TransactionType")]
    public abstract string TransactionType { get; }
    
    /// <summary>
    /// Gets or sets the account that initiated the transaction.
    /// </summary>
    [JsonPropertyName("Account")]
    public string Account { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the transaction sequence number.
    /// This number must be exactly 1 greater than the last sequence number for the account.
    /// </summary>
    [JsonPropertyName("Sequence")]
    public uint? Sequence { get; set; }
    
    /// <summary>
    /// Gets or sets the transaction fee in drops of XRP.
    /// </summary>
    [JsonPropertyName("Fee")]
    public string? Fee { get; set; }
    
    /// <summary>
    /// Gets or sets the bitwise flags for the transaction.
    /// </summary>
    [JsonPropertyName("Flags")]
    public uint? Flags { get; set; }
    
    /// <summary>
    /// Gets or sets the tagged account ID to forward payments to.
    /// </summary>
    [JsonPropertyName("DestinationTag")]
    public uint? DestinationTag { get; set; }
    
    /// <summary>
    /// Gets or sets the tagged account ID the payment originated from.
    /// </summary>
    [JsonPropertyName("SourceTag")]
    public uint? SourceTag { get; set; }
    
    /// <summary>
    /// Gets or sets the time after which the transaction is no longer valid.
    /// </summary>
    [JsonPropertyName("LastLedgerSequence")]
    public uint? LastLedgerSequence { get; set; }
    
    /// <summary>
    /// Gets or sets the public key used to sign this transaction.
    /// </summary>
    [JsonPropertyName("SigningPubKey")]
    public string? SigningPubKey { get; set; }
    
    /// <summary>
    /// Gets or sets the signature that verifies this transaction as authorized by the account.
    /// </summary>
    [JsonPropertyName("TxnSignature")]
    public string? TxnSignature { get; set; }
    
    /// <summary>
    /// Gets or sets a hash value identifying another transaction.
    /// If provided, this transaction is only valid if the sending account's previously-sent transaction matches the provided hash.
    /// </summary>
    [JsonPropertyName("AccountTxnID")]
    public string? AccountTxnID { get; set; }
    
    /// <summary>
    /// Gets or sets the memos to include with the transaction.
    /// </summary>
    [JsonPropertyName("Memos")]
    public List<Memo>? Memos { get; set; }
    
    /// <summary>
    /// Gets or sets the list of signers for a multi-signed transaction.
    /// </summary>
    [JsonPropertyName("Signers")]
    public List<Signer>? Signers { get; set; }
    
    /// <summary>
    /// Gets or sets the ticket number to use in place of a sequence number.
    /// </summary>
    [JsonPropertyName("TicketSequence")]
    public uint? TicketSequence { get; set; }
    
    /// <summary>
    /// Gets or sets the date the transaction was created.
    /// </summary>
    /// <remarks>
    /// This property is only used when submitting a transaction.
    /// It is not included in the final ledger.
    /// </remarks>
    [JsonIgnore]
    public DateTimeOffset? Date { get; set; }
    
    /// <summary>
    /// Gets the hash (ID) of the transaction.
    /// </summary>
    /// <remarks>
    /// This property is only available after the transaction has been submitted or retrieved from the ledger.
    /// </remarks>
    [JsonPropertyName("hash")]
    public string? Hash { get; set; }
    
    /// <summary>
    /// Adds a memo to the transaction.
    /// </summary>
    /// <param name="memo">The memo to add.</param>
    public void AddMemo(Memo memo)
    {
        Memos ??= new List<Memo>();
        Memos.Add(memo);
    }
    
    /// <summary>
    /// Adds a text memo to the transaction.
    /// </summary>
    /// <param name="text">The text to add as a memo.</param>
    public void AddMemo(string text)
    {
        AddMemo(new Memo(text));
    }
    
    /// <summary>
    /// Adds a signer to the transaction for multi-signing.
    /// </summary>
    /// <param name="signer">The signer to add.</param>
    public void AddSigner(Signer signer)
    {
        Signers ??= new List<Signer>();
        Signers.Add(signer);
    }
    
    /// <summary>
    /// Sets the transaction to expire after a specific number of ledgers.
    /// </summary>
    /// <param name="ledgerOffset">The number of ledgers after which the transaction expires.</param>
    /// <param name="currentLedgerIndex">The current ledger index. If not provided, the transaction will not be set to expire.</param>
    /// <returns>True if the expiration was set; otherwise, false.</returns>
    public bool SetExpiration(uint ledgerOffset, uint? currentLedgerIndex)
    {
        if (currentLedgerIndex.HasValue)
        {
            LastLedgerSequence = currentLedgerIndex.Value + ledgerOffset;
            return true;
        }
        
        return false;
    }
}