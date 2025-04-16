using System.Text.Json.Serialization;
using XRPL.NET.Core.Constants;

namespace XRPL.NET.Models.Transactions.AccountSet;

/// <summary>
/// Represents an AccountSet transaction, which modifies the properties of an account in the XRP Ledger.
/// </summary>
public class AccountSetTransaction : TransactionBase
{
    /// <summary>
    /// Gets the transaction type.
    /// </summary>
    [JsonPropertyName("TransactionType")]
    public override string TransactionType => TransactionTypes.AccountSet;
    
    /// <summary>
    /// Gets or sets the email address hash to set for this account.
    /// </summary>
    [JsonPropertyName("EmailHash")]
    public string? EmailHash { get; set; }
    
    /// <summary>
    /// Gets or sets the message key to set for this account.
    /// </summary>
    [JsonPropertyName("MessageKey")]
    public string? MessageKey { get; set; }
    
    /// <summary>
    /// Gets or sets the domain to set for this account.
    /// </summary>
    [JsonPropertyName("Domain")]
    public string? Domain { get; set; }
    
    /// <summary>
    /// Gets or sets the transfer rate to set for this account.
    /// </summary>
    /// <remarks>
    /// The transfer rate is specified in billionths of a unit, where 1,000,000,000 represents 0% fee.
    /// For example, a value of 1,020,000,000 represents a 2% fee.
    /// </remarks>
    [JsonPropertyName("TransferRate")]
    public uint? TransferRate { get; set; }
    
    /// <summary>
    /// Gets or sets the tick size to set for this account.
    /// </summary>
    /// <remarks>
    /// The tick size is the smallest increment by which offers made by this account can be priced.
    /// Valid values are 3-15, where the value represents the number of decimal places to ignore.
    /// </remarks>
    [JsonPropertyName("TickSize")]
    public uint? TickSize { get; set; }
    
    /// <summary>
    /// Gets or sets the account flags to set (enable).
    /// </summary>
    [JsonPropertyName("SetFlag")]
    public uint? SetFlag { get; set; }
    
    /// <summary>
    /// Gets or sets the account flags to clear (disable).
    /// </summary>
    [JsonPropertyName("ClearFlag")]
    public uint? ClearFlag { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether to require destination tags on incoming payments.
    /// </summary>
    [JsonIgnore]
    public bool? RequireDestinationTag
    {
        get => GetFlagValue(AccountSetFlags.RequireDestTag);
        set => SetFlagValue(AccountSetFlags.RequireDestTag, value);
    }
    
    /// <summary>
    /// Gets or sets a value indicating whether to require authorization for incoming trust lines.
    /// </summary>
    [JsonIgnore]
    public bool? RequireAuthorization
    {
        get => GetFlagValue(AccountSetFlags.RequireAuth);
        set => SetFlagValue(AccountSetFlags.RequireAuth, value);
    }
    
    /// <summary>
    /// Gets or sets a value indicating whether to disable the master key.
    /// </summary>
    [JsonIgnore]
    public bool? DisableMasterKey
    {
        get => GetFlagValue(AccountSetFlags.DisableMaster);
        set => SetFlagValue(AccountSetFlags.DisableMaster, value);
    }
    
    /// <summary>
    /// Gets or sets a value indicating whether to disallow XRP payments to this account.
    /// </summary>
    [JsonIgnore]
    public bool? DisallowXrp
    {
        get => GetFlagValue(AccountSetFlags.DisallowXRP);
        set => SetFlagValue(AccountSetFlags.DisallowXRP, value);
    }
    
    /// <summary>
    /// Gets or sets a value indicating whether to allow trustlines in the account's name to be frozen.
    /// </summary>
    [JsonIgnore]
    public bool? AllowTrustLineFreeze
    {
        get => GetFlagValue(AccountSetFlags.AllowTrustLineFreeze);
        set => SetFlagValue(AccountSetFlags.AllowTrustLineFreeze, value);
    }
    
    /// <summary>
    /// Gets or sets a value indicating whether to freeze all trustlines issued by this account.
    /// </summary>
    [JsonIgnore]
    public bool? GlobalFreeze
    {
        get => GetFlagValue(AccountSetFlags.GlobalFreeze);
        set => SetFlagValue(AccountSetFlags.GlobalFreeze, value);
    }
    
    /// <summary>
    /// Gets or sets a value indicating whether to permanently set the No-Freeze flag, making this account unable to freeze trustlines again.
    /// </summary>
    [JsonIgnore]
    public bool? NoFreeze
    {
        get => GetFlagValue(AccountSetFlags.NoFreeze);
        set => SetFlagValue(AccountSetFlags.NoFreeze, value);
    }
    
    /// <summary>
    /// Gets or sets a value indicating whether to permanently enable the DefaultRipple flag.
    /// </summary>
    [JsonIgnore]
    public bool? DefaultRipple
    {
        get => GetFlagValue(AccountSetFlags.DefaultRipple);
        set => SetFlagValue(AccountSetFlags.DefaultRipple, value);
    }
    
    /// <summary>
    /// Gets or sets a value indicating whether to allow this account to delete itself from the ledger.
    /// </summary>
    [JsonIgnore]
    public bool? AllowAccountDelete
    {
        get => GetFlagValue(AccountSetFlags.AllowAccountDelete);
        set => SetFlagValue(AccountSetFlags.AllowAccountDelete, value);
    }
    
    /// <summary>
    /// Gets the value of a specific account flag.
    /// </summary>
    /// <param name="flag">The flag to check.</param>
    /// <returns>True if the flag is set, false if it is cleared, or null if it is not explicitly set or cleared.</returns>
    private bool? GetFlagValue(uint flag)
    {
        if (SetFlag == flag)
        {
            return true;
        }
        
        if (ClearFlag == flag)
        {
            return false;
        }
        
        return null;
    }
    
    /// <summary>
    /// Sets the value of a specific account flag.
    /// </summary>
    /// <param name="flag">The flag to set or clear.</param>
    /// <param name="value">True to set the flag, false to clear it, or null to leave it unchanged.</param>
    private void SetFlagValue(uint flag, bool? value)
    {
        if (value.HasValue)
        {
            if (value.Value)
            {
                SetFlag = flag;
                
                // If we were previously clearing this flag, remove that
                if (ClearFlag == flag)
                {
                    ClearFlag = null;
                }
            }
            else
            {
                ClearFlag = flag;
                
                // If we were previously setting this flag, remove that
                if (SetFlag == flag)
                {
                    SetFlag = null;
                }
            }
        }
        else
        {
            // Remove both SetFlag and ClearFlag if they match this flag
            if (SetFlag == flag)
            {
                SetFlag = null;
            }
            
            if (ClearFlag == flag)
            {
                ClearFlag = null;
            }
        }
    }
}

/// <summary>
/// Defines the account flags that can be set with an AccountSet transaction.
/// </summary>
public static class AccountSetFlags
{
    /// <summary>
    /// Require a destination tag for incoming payments.
    /// </summary>
    public const uint RequireDestTag = 1;
    
    /// <summary>
    /// Require authorization for users to hold balances issued by this account.
    /// </summary>
    public const uint RequireAuth = 2;
    
    /// <summary>
    /// Disallow use of the master key pair for signing transactions.
    /// </summary>
    public const uint DisableMaster = 4;
    
    /// <summary>
    /// Disallow incoming XRP payments from other accounts.
    /// </summary>
    public const uint DisallowXRP = 3;
    
    /// <summary>
    /// Enable freezing of trustlines issued by this account.
    /// </summary>
    public const uint AllowTrustLineFreeze = 6;
    
    /// <summary>
    /// Freeze all assets issued by this account.
    /// </summary>
    public const uint GlobalFreeze = 7;
    
    /// <summary>
    /// Permanently give up the ability to freeze individual trustlines or all trustlines.
    /// </summary>
    public const uint NoFreeze = 8;
    
    /// <summary>
    /// Enable rippling on this account's trustlines by default.
    /// </summary>
    public const uint DefaultRipple = 9;
    
    /// <summary>
    /// Allow this account to be deleted.
    /// </summary>
    public const uint AllowAccountDelete = 10;
}