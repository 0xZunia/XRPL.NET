using System.Text;
using XRPL.NET.Models.Transactions.AccountSet;
using XRPL.NET.Transactions.Factory;

namespace XRPL.NET.Transactions.Builders;

/// <summary>
/// Builder for creating account set transactions.
/// </summary>
public class AccountSetBuilder : BuilderBase<AccountSetBuilder, AccountSetTransaction>
{
    /// <summary>
    /// Sets the email address hash for the account.
    /// </summary>
    /// <param name="emailHash">The email address hash as a hex string.</param>
    /// <returns>The builder for method chaining.</returns>
    public AccountSetBuilder WithEmailHash(string emailHash)
    {
        Transaction.EmailHash = emailHash;
        return this;
    }
    
    /// <summary>
    /// Sets the email address for the account by hashing it.
    /// </summary>
    /// <param name="email">The email address.</param>
    /// <returns>The builder for method chaining.</returns>
    public AccountSetBuilder WithEmail(string email)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(email.ToLower()));
        Transaction.EmailHash = Convert.ToHexString(hash).ToLower();
        return this;
    }
    
    /// <summary>
    /// Sets the message key for the account.
    /// </summary>
    /// <param name="messageKey">The message key as a hex string.</param>
    /// <returns>The builder for method chaining.</returns>
    public AccountSetBuilder WithMessageKey(string messageKey)
    {
        Transaction.MessageKey = messageKey;
        return this;
    }
    
    /// <summary>
    /// Sets the domain for the account.
    /// </summary>
    /// <param name="domain">The domain name.</param>
    /// <returns>The builder for method chaining.</returns>
    public AccountSetBuilder WithDomain(string domain)
    {
        Transaction.Domain = Convert.ToHexString(Encoding.ASCII.GetBytes(domain)).ToLower();
        return this;
    }
    
    /// <summary>
    /// Sets the transfer rate for the account.
    /// </summary>
    /// <param name="transferRate">The transfer rate as a percentage.</param>
    /// <returns>The builder for method chaining.</returns>
    /// <remarks>
    /// The transfer rate is specified as a percentage of the amount being transferred.
    /// For example, a value of 1.2% would be passed as 1.2.
    /// </remarks>
    public AccountSetBuilder WithTransferRate(decimal transferRate)
    {
        if (transferRate < 0 || transferRate > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(transferRate), "Transfer rate must be between 0 and 100.");
        }
        
        // Convert percentage to billionths
        // 0% = 1,000,000,000 (special case)
        // 1% = 1,010,000,000
        
        if (transferRate == 0)
        {
            Transaction.TransferRate = 0;
        }
        else
        {
            uint rate = 1_000_000_000 + (uint)(transferRate * 10_000_000);
            Transaction.TransferRate = rate;
        }
        
        return this;
    }
    
    /// <summary>
    /// Sets the tick size for the account.
    /// </summary>
    /// <param name="tickSize">The tick size (3-15).</param>
    /// <returns>The builder for method chaining.</returns>
    public AccountSetBuilder WithTickSize(uint tickSize)
    {
        if (tickSize < 3 || tickSize > 15)
        {
            throw new ArgumentOutOfRangeException(nameof(tickSize), "Tick size must be between 3 and 15.");
        }
        
        Transaction.TickSize = tickSize;
        return this;
    }
    
    /// <summary>
    /// Sets whether to require destination tags for incoming payments.
    /// </summary>
    /// <param name="require">Whether to require destination tags.</param>
    /// <returns>The builder for method chaining.</returns>
    public AccountSetBuilder RequireDestinationTag(bool require = true)
    {
        Transaction.RequireDestinationTag = require;
        return this;
    }
    
    /// <summary>
    /// Sets whether to require authorization for incoming trust lines.
    /// </summary>
    /// <param name="require">Whether to require authorization.</param>
    /// <returns>The builder for method chaining.</returns>
    public AccountSetBuilder RequireAuthorization(bool require = true)
    {
        Transaction.RequireAuthorization = require;
        return this;
    }
    
    /// <summary>
    /// Sets whether to disable the master key.
    /// </summary>
    /// <param name="disable">Whether to disable the master key.</param>
    /// <returns>The builder for method chaining.</returns>
    public AccountSetBuilder DisableMasterKey(bool disable = true)
    {
        Transaction.DisableMasterKey = disable;
        return this;
    }
    
    /// <summary>
    /// Sets whether to disallow XRP payments to this account.
    /// </summary>
    /// <param name="disallow">Whether to disallow XRP payments.</param>
    /// <returns>The builder for method chaining.</returns>
    public AccountSetBuilder DisallowXrp(bool disallow = true)
    {
        Transaction.DisallowXrp = disallow;
        return this;
    }
    
    /// <summary>
    /// Sets whether to allow trust line freezing.
    /// </summary>
    /// <param name="allow">Whether to allow trust line freezing.</param>
    /// <returns>The builder for method chaining.</returns>
    public AccountSetBuilder AllowTrustLineFreeze(bool allow = true)
    {
        Transaction.AllowTrustLineFreeze = allow;
        return this;
    }
    
    /// <summary>
    /// Sets whether to globally freeze all issued currencies.
    /// </summary>
    /// <param name="freeze">Whether to freeze all issued currencies.</param>
    /// <returns>The builder for method chaining.</returns>
    public AccountSetBuilder GlobalFreeze(bool freeze = true)
    {
        Transaction.GlobalFreeze = freeze;
        return this;
    }
    
    /// <summary>
    /// Sets whether to permanently give up the ability to freeze trust lines.
    /// </summary>
    /// <param name="noFreeze">Whether to permanently disable freezing.</param>
    /// <returns>The builder for method chaining.</returns>
    public AccountSetBuilder NoFreeze(bool noFreeze = true)
    {
        Transaction.NoFreeze = noFreeze;
        return this;
    }
    
    /// <summary>
    /// Sets whether to enable rippling on this account's trust lines by default.
    /// </summary>
    /// <param name="enable">Whether to enable default rippling.</param>
    /// <returns>The builder for method chaining.</returns>
    public AccountSetBuilder DefaultRipple(bool enable = true)
    {
        Transaction.DefaultRipple = enable;
        return this;
    }
    
    /// <summary>
    /// Sets whether to allow this account to be deleted from the ledger.
    /// </summary>
    /// <param name="allow">Whether to allow account deletion.</param>
    /// <returns>The builder for method chaining.</returns>
    public AccountSetBuilder AllowAccountDelete(bool allow = true)
    {
        Transaction.AllowAccountDelete = allow;
        return this;
    }
    
    /// <summary>
    /// Sets a specific flag value directly.
    /// </summary>
    /// <param name="flag">The flag value to set.</param>
    /// <returns>The builder for method chaining.</returns>
    public AccountSetBuilder SetFlag(uint flag)
    {
        Transaction.SetFlag = flag;
        return this;
    }
    
    /// <summary>
    /// Clears a specific flag value directly.
    /// </summary>
    /// <param name="flag">The flag value to clear.</param>
    /// <returns>The builder for method chaining.</returns>
    public AccountSetBuilder ClearFlag(uint flag)
    {
        Transaction.ClearFlag = flag;
        return this;
    }
}