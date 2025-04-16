using System.Text.Json.Serialization;
using XRPL.NET.Core.Constants;

namespace XRPL.NET.Models.Transactions.PaymentChannel;

/// <summary>
/// Represents a PaymentChannelClaim transaction, which claims funds from a payment channel.
/// </summary>
public class PaymentChannelClaimTransaction : TransactionBase
{
    /// <summary>
    /// Gets the transaction type.
    /// </summary>
    [JsonPropertyName("TransactionType")]
    public override string TransactionType => TransactionTypes.PaymentChannelClaim;
    
    /// <summary>
    /// Gets or sets the payment channel to claim from.
    /// </summary>
    [JsonPropertyName("Channel")]
    public string Channel { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the amount to claim from the channel.
    /// </summary>
    [JsonPropertyName("Amount")]
    public string? Amount { get; set; }
    
    /// <summary>
    /// Gets or sets the signature that authorizes the claim.
    /// </summary>
    [JsonPropertyName("Signature")]
    public string? Signature { get; set; }
    
    /// <summary>
    /// Gets or sets the public key used to sign the claim.
    /// </summary>
    [JsonPropertyName("PublicKey")]
    public string? PublicKey { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether to close the channel after claiming.
    /// </summary>
    [JsonIgnore]
    public bool Close
    {
        get => (Flags ?? 0) != 0 && ((Flags & PaymentChannelClaimFlags.Close) != 0);
        set
        {
            if (value)
            {
                Flags = (Flags ?? 0) | PaymentChannelClaimFlags.Close;
            }
            else
            {
                Flags = (Flags ?? 0) & ~PaymentChannelClaimFlags.Close;
            }
        }
    }
}

/// <summary>
/// Defines flags for PaymentChannelClaim transactions.
/// </summary>
public static class PaymentChannelClaimFlags
{
    /// <summary>
    /// Indicates that the channel should be closed after this claim.
    /// </summary>
    public const uint Close = 0x00010000;
}