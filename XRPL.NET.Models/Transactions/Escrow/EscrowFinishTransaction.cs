using System.Text.Json.Serialization;
using XRPL.NET.Core.Constants;

namespace XRPL.NET.Models.Transactions.Escrow;
/// <summary>
/// Represents an EscrowFinish transaction, which completes a previously created escrow.
/// </summary>
public class EscrowFinishTransaction : TransactionBase
{
    /// <summary>
    /// Gets the transaction type.
    /// </summary>
    [JsonPropertyName("TransactionType")]
    public override string TransactionType => TransactionTypes.EscrowFinish;
    /// <summary>
    /// Gets or sets the account that created the escrow (owner).
    /// </summary>
    [JsonPropertyName("Owner")]
    public string Owner { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the sequence number of the escrow to finish.
    /// </summary>
    [JsonPropertyName("OfferSequence")]
    public uint OfferSequence { get; set; }

    /// <summary>
    /// Gets or sets the cryptographic condition to fulfill the escrow.
    /// </summary>
    [JsonPropertyName("Condition")]
    public string? Condition { get; set; }

    /// <summary>
    /// Gets or sets the fulfillment of the cryptographic condition.
    /// </summary>
    [JsonPropertyName("Fulfillment")]
    public string? Fulfillment { get; set; }
}