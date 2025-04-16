using System.Text.Json.Serialization;
using XRPL.NET.Core.Constants;
using XRPL.NET.Models.Transactions.Common;

namespace XRPL.NET.Models.Transactions.Payment;

/// <summary>
/// Represents a payment transaction on the XRP Ledger.
/// </summary>
/// <remarks>
/// A Payment transaction represents a transfer of value from one account to another.
/// Payments can be in XRP or in issued currencies.
/// </remarks>
public class PaymentTransaction : TransactionBase
{
    /// <summary>
    /// Gets the transaction type.
    /// </summary>
    [JsonPropertyName("TransactionType")]
    public override string TransactionType => TransactionTypes.Payment;
    
    /// <summary>
    /// Gets or sets the address of the account receiving the payment.
    /// </summary>
    [JsonPropertyName("Destination")]
    public string Destination { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the amount to deliver to the destination.
    /// </summary>
    /// <remarks>
    /// For XRP, this is a string representing the number of drops.
    /// For issued currencies, this is an object with currency, issuer, and value fields.
    /// </remarks>
    [JsonPropertyName("Amount")]
    public object Amount { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the maximum amount to send.
    /// </summary>
    /// <remarks>
    /// For direct XRP-to-XRP payments, this is equal to the Amount.
    /// For currency exchange payments, this is the maximum amount to send.
    /// </remarks>
    [JsonPropertyName("SendMax")]
    public object? SendMax { get; set; }
    
    /// <summary>
    /// Gets or sets the minimum amount to deliver to the destination.
    /// </summary>
    /// <remarks>
    /// If the payment cannot deliver at least this amount, it fails.
    /// This is used for partial payments.
    /// </remarks>
    [JsonPropertyName("DeliverMin")]
    public object? DeliverMin { get; set; }
    
    /// <summary>
    /// Gets or sets the payment paths to use.
    /// </summary>
    /// <remarks>
    /// Paths define the series of trusted issuer relationships to use in currency exchange.
    /// </remarks>
    [JsonPropertyName("Paths")]
    public List<List<PathStep>>? Paths { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether to use the partial payment flag.
    /// </summary>
    /// <remarks>
    /// If true, the payment can deliver less than the full amount.
    /// </remarks>
    [JsonIgnore]
    public bool PartialPayment
    {
        get => (Flags ?? 0) != 0 && ((Flags & PaymentFlags.PartialPayment) != 0);
        set
        {
            if (value)
            {
                Flags = (Flags ?? 0) | PaymentFlags.PartialPayment;
            }
            else
            {
                Flags = (Flags ?? 0) & ~PaymentFlags.PartialPayment;
            }
        }
    }
    
    /// <summary>
    /// Gets or sets a value indicating whether to use the limit quality flag.
    /// </summary>
    /// <remarks>
    /// If true, the payment won't cross offers with worse quality than the best offer.
    /// </remarks>
    [JsonIgnore]
    public bool LimitQuality
    {
        get => (Flags ?? 0) != 0 && ((Flags & PaymentFlags.LimitQuality) != 0);
        set
        {
            if (value)
            {
                Flags = (Flags ?? 0) | PaymentFlags.LimitQuality;
            }
            else
            {
                Flags = (Flags ?? 0) & ~PaymentFlags.LimitQuality;
            }
        }
    }
    
    /// <summary>
    /// Gets or sets a value indicating whether to use the no direct ripple flag.
    /// </summary>
    /// <remarks>
    /// If true, the payment won't use a direct path between source and destination.
    /// </remarks>
    [JsonIgnore]
    public bool NoDirectRipple
    {
        get => (Flags ?? 0) != 0 && ((Flags & PaymentFlags.NoDirectRipple) != 0);
        set
        {
            if (value)
            {
                Flags = (Flags ?? 0) | PaymentFlags.NoDirectRipple;
            }
            else
            {
                Flags = (Flags ?? 0) & ~PaymentFlags.NoDirectRipple;
            }
        }
    }
    
    /// <summary>
    /// Adds a path to the payment.
    /// </summary>
    /// <param name="path">The path to add.</param>
    public void AddPath(List<PathStep> path)
    {
        Paths ??= new List<List<PathStep>>();
        Paths.Add(path);
    }
}

/// <summary>
/// Defines the flags for payment transactions.
/// </summary>
public static class PaymentFlags
{
    /// <summary>
    /// If set, do not reject the payment if it cannot be fully filled.
    /// </summary>
    public const uint PartialPayment = 0x00020000;
    
    /// <summary>
    /// If set, do not use the direct path between source and destination.
    /// </summary>
    public const uint NoDirectRipple = 0x00010000;
    
    /// <summary>
    /// If set, limit the paths used to those with the highest quality.
    /// </summary>
    public const uint LimitQuality = 0x00040000;
}