using System.Text.Json.Serialization;
using XRPL.NET.Core.Constants;
using XRPL.NET.Core.Interfaces;

namespace XRPL.NET.Models.Transactions.Trust;

/// <summary>
/// Represents a TrustSet transaction, which creates or modifies a trust line between two accounts.
/// </summary>
/// <remarks>
/// A trust line represents a limit of how much an account is willing to trust another account in a specific currency.
/// </remarks>
public class TrustSetTransaction : TransactionBase
{
    /// <summary>
    /// Gets the transaction type.
    /// </summary>
    [JsonPropertyName("TransactionType")]
    public override string TransactionType => TransactionTypes.TrustSet;
    
    /// <summary>
    /// Gets or sets the limit amount for the trust line.
    /// </summary>
    /// <remarks>
    /// This must be an issued currency amount, not XRP.
    /// </remarks>
    [JsonPropertyName("LimitAmount")]
    public IssuedCurrencyAmount LimitAmount { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the quality in, which determines the exchange rate when the account receives the currency.
    /// </summary>
    /// <remarks>
    /// Quality is represented as a ratio of the amount received to the amount debited, in billionths.
    /// A value of 1 billion (1,000,000,000) is neutral (1:1 ratio).
    /// </remarks>
    [JsonPropertyName("QualityIn")]
    public uint? QualityIn { get; set; }
    
    /// <summary>
    /// Gets or sets the quality out, which determines the exchange rate when the account sends the currency.
    /// </summary>
    /// <remarks>
    /// Quality is represented as a ratio of the amount received to the amount debited, in billionths.
    /// A value of 1 billion (1,000,000,000) is neutral (1:1 ratio).
    /// </remarks>
    [JsonPropertyName("QualityOut")]
    public uint? QualityOut { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether to set the authorized flag on the trust line.
    /// </summary>
    /// <remarks>
    /// This is only useful if the account requires authorization for others to hold its issued currencies.
    /// </remarks>
    [JsonIgnore]
    public bool SetAuthorized
    {
        get => (Flags ?? 0) != 0 && ((Flags & TrustSetFlags.SetAuth) != 0);
        set
        {
            if (value)
            {
                Flags = (Flags ?? 0) | TrustSetFlags.SetAuth;
            }
            else
            {
                Flags = (Flags ?? 0) & ~TrustSetFlags.SetAuth;
            }
        }
    }
    
    /// <summary>
    /// Gets or sets a value indicating whether to freeze the trust line.
    /// </summary>
    /// <remarks>
    /// This prevents the counter-party from transferring the frozen currency.
    /// </remarks>
    [JsonIgnore]
    public bool SetFreeze
    {
        get => (Flags ?? 0) != 0 && ((Flags & TrustSetFlags.SetFreeze) != 0);
        set
        {
            if (value)
            {
                Flags = (Flags ?? 0) | TrustSetFlags.SetFreeze;
            }
            else
            {
                Flags = (Flags ?? 0) & ~TrustSetFlags.SetFreeze;
            }
        }
    }
    
    /// <summary>
    /// Gets or sets a value indicating whether to remove the freeze from the trust line.
    /// </summary>
    [JsonIgnore]
    public bool ClearFreeze
    {
        get => (Flags ?? 0) != 0 && ((Flags & TrustSetFlags.ClearFreeze) != 0);
        set
        {
            if (value)
            {
                Flags = (Flags ?? 0) | TrustSetFlags.ClearFreeze;
            }
            else
            {
                Flags = (Flags ?? 0) & ~TrustSetFlags.ClearFreeze;
            }
        }
    }
    
    /// <summary>
    /// Gets or sets a value indicating whether the account allows rippling through this trust line.
    /// </summary>
    [JsonIgnore]
    public bool NoRipple
    {
        get => (Flags ?? 0) != 0 && ((Flags & TrustSetFlags.NoRipple) != 0);
        set
        {
            if (value)
            {
                Flags = (Flags ?? 0) | TrustSetFlags.NoRipple;
            }
            else
            {
                Flags = (Flags ?? 0) & ~TrustSetFlags.NoRipple;
            }
        }
    }
}

/// <summary>
/// Defines the flags for TrustSet transactions.
/// </summary>
public static class TrustSetFlags
{
    /// <summary>
    /// Authorize the counterparty to hold currency issued by this account.
    /// </summary>
    public const uint SetAuth = 0x00010000;
    
    /// <summary>
    /// Freeze the trust line.
    /// </summary>
    public const uint SetFreeze = 0x00100000;
    
    /// <summary>
    /// Remove the freeze from the trust line.
    /// </summary>
    public const uint ClearFreeze = 0x00200000;
    
    /// <summary>
    /// Disable rippling through this trust line.
    /// </summary>
    public const uint NoRipple = 0x00020000;
}