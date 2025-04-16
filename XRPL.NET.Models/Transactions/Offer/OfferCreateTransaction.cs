using System.Text.Json.Serialization;
using XRPL.NET.Core.Constants;

namespace XRPL.NET.Models.Transactions.Offer;

/// <summary>
/// Represents an OfferCreate transaction, which creates an offer to exchange currencies on the XRP Ledger's decentralized exchange.
/// </summary>
public class OfferCreateTransaction : TransactionBase
{
    /// <summary>
    /// Gets the transaction type.
    /// </summary>
    [JsonPropertyName("TransactionType")]
    public override string TransactionType => TransactionTypes.OfferCreate;
    
    /// <summary>
    /// Gets or sets the amount of currency being offered.
    /// </summary>
    /// <remarks>
    /// This can be either an XRP amount (as a string) or an issued currency amount (as an object).
    /// </remarks>
    [JsonPropertyName("TakerPays")]
    public object TakerPays { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the amount of currency being requested in exchange.
    /// </summary>
    /// <remarks>
    /// This can be either an XRP amount (as a string) or an issued currency amount (as an object).
    /// </remarks>
    [JsonPropertyName("TakerGets")]
    public object TakerGets { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets a time after which the offer is no longer active.
    /// </summary>
    /// <remarks>
    /// This is specified as seconds since the Ripple Epoch (January 1, 2000 00:00:00 UTC).
    /// </remarks>
    [JsonPropertyName("Expiration")]
    public uint? Expiration { get; set; }
    
    /// <summary>
    /// Gets or sets the sequence number of an existing offer to replace.
    /// </summary>
    /// <remarks>
    /// If specified, the existing offer is canceled when this one is created.
    /// </remarks>
    [JsonPropertyName("OfferSequence")]
    public uint? OfferSequence { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether the offer is passive.
    /// </summary>
    /// <remarks>
    /// A passive offer does not consume offers that exactly match it, allowing multiple offers at the same exchange rate.
    /// </remarks>
    [JsonIgnore]
    public bool Passive
    {
        get => (Flags ?? 0) != 0 && ((Flags & OfferCreateFlags.Passive) != 0);
        set
        {
            if (value)
            {
                Flags = (Flags ?? 0) | OfferCreateFlags.Passive;
            }
            else
            {
                Flags = (Flags ?? 0) & ~OfferCreateFlags.Passive;
            }
        }
    }
    
    /// <summary>
    /// Gets or sets a value indicating whether the offer is an immediate or cancel order.
    /// </summary>
    /// <remarks>
    /// If enabled, the offer is canceled if it cannot be filled immediately. 
    /// Any portion that cannot be immediately filled is canceled.
    /// </remarks>
    [JsonIgnore]
    public bool ImmediateOrCancel
    {
        get => (Flags ?? 0) != 0 && ((Flags & OfferCreateFlags.ImmediateOrCancel) != 0);
        set
        {
            if (value)
            {
                Flags = (Flags ?? 0) | OfferCreateFlags.ImmediateOrCancel;
            }
            else
            {
                Flags = (Flags ?? 0) & ~OfferCreateFlags.ImmediateOrCancel;
            }
        }
    }
    
    /// <summary>
    /// Gets or sets a value indicating whether the offer is a fill or kill order.
    /// </summary>
    /// <remarks>
    /// If enabled, the offer is canceled if it cannot be completely filled immediately.
    /// </remarks>
    [JsonIgnore]
    public bool FillOrKill
    {
        get => (Flags ?? 0) != 0 && ((Flags & OfferCreateFlags.FillOrKill) != 0);
        set
        {
            if (value)
            {
                Flags = (Flags ?? 0) | OfferCreateFlags.FillOrKill;
            }
            else
            {
                Flags = (Flags ?? 0) & ~OfferCreateFlags.FillOrKill;
            }
        }
    }
    
    /// <summary>
    /// Gets or sets a value indicating whether to allow selling XRP.
    /// </summary>
    /// <remarks>
    /// If not enabled and the TakerGets currency is XRP, the offer is canceled if it would result in the account spending XRP.
    /// </remarks>
    [JsonIgnore]
    public bool Sell
    {
        get => (Flags ?? 0) != 0 && ((Flags & OfferCreateFlags.Sell) != 0);
        set
        {
            if (value)
            {
                Flags = (Flags ?? 0) | OfferCreateFlags.Sell;
            }
            else
            {
                Flags = (Flags ?? 0) & ~OfferCreateFlags.Sell;
            }
        }
    }
}

/// <summary>
/// Defines the flags for OfferCreate transactions.
/// </summary>
public static class OfferCreateFlags
{
    /// <summary>
    /// If enabled, the offer does not consume offers that exactly match it.
    /// </summary>
    public const uint Passive = 0x00010000;
    
    /// <summary>
    /// If enabled, the offer is canceled if it cannot be completely filled immediately.
    /// </summary>
    public const uint ImmediateOrCancel = 0x00020000;
    
    /// <summary>
    /// If enabled, the offer is canceled if it cannot be completely filled immediately.
    /// </summary>
    public const uint FillOrKill = 0x00040000;
    
    /// <summary>
    /// If enabled, the offer is interpreted as offering to sell the entire TakerGets amount.
    /// </summary>
    public const uint Sell = 0x00080000;
}