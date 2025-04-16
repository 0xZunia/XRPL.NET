using System.Text.Json.Serialization;
using XRPL.NET.Core.Constants;

namespace XRPL.NET.Models.Transactions.Escrow;

/// <summary>
/// Represents an EscrowCancel transaction, which cancels an existing escrow.
/// </summary>
public class EscrowCancelTransaction : TransactionBase
{
    /// <summary>
    /// Gets the transaction type.
    /// </summary>
    [JsonPropertyName("TransactionType")]
    public override string TransactionType => TransactionTypes.EscrowCancel;
    
    /// <summary>
    /// Gets or sets the account that created the escrow (owner).
    /// </summary>
    [JsonPropertyName("Owner")]
    public string Owner { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the sequence number of the escrow to cancel.
    /// </summary>
    [JsonPropertyName("OfferSequence")]
    public uint OfferSequence { get; set; }
}