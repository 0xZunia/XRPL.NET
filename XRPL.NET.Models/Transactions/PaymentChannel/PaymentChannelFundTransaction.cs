using System.Text.Json.Serialization;
using XRPL.NET.Core.Constants;

namespace XRPL.NET.Models.Transactions.PaymentChannel;

/// <summary>
/// Represents a PaymentChannelFund transaction, which adds funds to an existing payment channel.
/// </summary>
public class PaymentChannelFundTransaction : TransactionBase
{
    /// <summary>
    /// Gets the transaction type.
    /// </summary>
    [JsonPropertyName("TransactionType")]
    public override string TransactionType => TransactionTypes.PaymentChannelFund;
    
    /// <summary>
    /// Gets or sets the payment channel to fund.
    /// </summary>
    [JsonPropertyName("Channel")]
    public string Channel { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the amount to add to the payment channel.
    /// </summary>
    [JsonPropertyName("Amount")]
    public string Amount { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the public key of the channel.
    /// </summary>
    [JsonPropertyName("PublicKey")]
    public string? PublicKey { get; set; }
}