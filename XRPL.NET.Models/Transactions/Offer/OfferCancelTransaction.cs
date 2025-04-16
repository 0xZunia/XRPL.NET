using System.Text.Json.Serialization;
using XRPL.NET.Core.Constants;

namespace XRPL.NET.Models.Transactions.Offer;

/// <summary>
/// Represents an OfferCancel transaction, which cancels an existing offer in the XRP Ledger's decentralized exchange.
/// </summary>
public class OfferCancelTransaction : TransactionBase
{
    /// <summary>
    /// Gets the transaction type.
    /// </summary>
    [JsonPropertyName("TransactionType")]
    public override string TransactionType => TransactionTypes.OfferCancel;
        
    /// <summary>
    /// Gets or sets the sequence number of the offer to cancel.
    /// </summary>
    /// <remarks>
    /// This is the sequence number of the OfferCreate transaction that created the offer.
    /// </remarks>
    [JsonPropertyName("OfferSequence")]
    public uint OfferSequence { get; set; }
}