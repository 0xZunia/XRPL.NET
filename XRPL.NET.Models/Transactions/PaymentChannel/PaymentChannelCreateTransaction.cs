using System.Text.Json.Serialization;
using XRPL.NET.Core.Constants;

namespace XRPL.NET.Models.Transactions.PaymentChannel;

/// <summary>
/// Represents a PaymentChannelCreate transaction, which creates a new payment channel.
/// </summary>
public class PaymentChannelCreateTransaction : TransactionBase
{
    /// <summary>
    /// Gets the transaction type.
    /// </summary>
    [JsonPropertyName("TransactionType")]
    public override string TransactionType => TransactionTypes.PaymentChannelCreate;
    
    /// <summary>
    /// Gets or sets the destination account for the payment channel.
    /// </summary>
    [JsonPropertyName("Destination")]
    public string Destination { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the amount to allocate to the payment channel.
    /// </summary>
    [JsonPropertyName("Amount")]
    public string Amount { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the public key of the channel.
    /// </summary>
    [JsonPropertyName("PublicKey")]
    public string PublicKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the settlement delay for the payment channel.
    /// </summary>
    [JsonPropertyName("SettleDelay")]
    public uint SettleDelay { get; set; }
}