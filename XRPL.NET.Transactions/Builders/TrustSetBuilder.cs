using XRPL.NET.Core.Interfaces;
using XRPL.NET.Models.Transactions.Trust;
using XRPL.NET.Transactions.Factory;

namespace XRPL.NET.Transactions.Builders;

/// <summary>
/// Builder for creating trust set transactions.
/// </summary>
public class TrustSetBuilder : BuilderBase<TrustSetBuilder, TrustSetTransaction>
{
    /// <summary>
    /// Sets the limit amount for the trust line.
    /// </summary>
    /// <param name="currency">The currency code.</param>
    /// <param name="issuer">The issuer account.</param>
    /// <param name="value">The limit value.</param>
    /// <returns>The builder for method chaining.</returns>
    public TrustSetBuilder WithLimitAmount(string currency, string issuer, decimal value)
    {
        Transaction.LimitAmount = new IssuedCurrencyAmount
        {
            Currency = currency,
            Issuer = issuer,
            Value = value
        };
        return this;
    }
    
    /// <summary>
    /// Sets the limit amount for the trust line.
    /// </summary>
    /// <param name="issuedCurrencyAmount">The issued currency amount.</param>
    /// <returns>The builder for method chaining.</returns>
    public TrustSetBuilder WithLimitAmount(IssuedCurrencyAmount issuedCurrencyAmount)
    {
        Transaction.LimitAmount = issuedCurrencyAmount;
        return this;
    }
    
    /// <summary>
    /// Sets the quality in, which determines the exchange rate when the account receives the currency.
    /// </summary>
    /// <param name="qualityIn">The quality in value.</param>
    /// <returns>The builder for method chaining.</returns>
    /// <remarks>
    /// Quality is represented as a ratio of the amount received to the amount debited, in billionths.
    /// A value of 1 billion (1,000,000,000) is neutral (1:1 ratio).
    /// </remarks>
    public TrustSetBuilder WithQualityIn(decimal qualityIn)
    {
        if (qualityIn < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(qualityIn), "Quality in must be a positive value.");
        }
        
        // Convert to billionths
        uint qualityInValue = (uint)(qualityIn * 1_000_000_000);
        Transaction.QualityIn = qualityInValue;
        return this;
    }
    
    /// <summary>
    /// Sets the quality out, which determines the exchange rate when the account sends the currency.
    /// </summary>
    /// <param name="qualityOut">The quality out value.</param>
    /// <returns>The builder for method chaining.</returns>
    /// <remarks>
    /// Quality is represented as a ratio of the amount received to the amount debited, in billionths.
    /// A value of 1 billion (1,000,000,000) is neutral (1:1 ratio).
    /// </remarks>
    public TrustSetBuilder WithQualityOut(decimal qualityOut)
    {
        if (qualityOut < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(qualityOut), "Quality out must be a positive value.");
        }
        
        // Convert to billionths
        uint qualityOutValue = (uint)(qualityOut * 1_000_000_000);
        Transaction.QualityOut = qualityOutValue;
        return this;
    }
    
    /// <summary>
    /// Sets whether to authorize the trust line.
    /// </summary>
    /// <param name="authorize">Whether to authorize the trust line.</param>
    /// <returns>The builder for method chaining.</returns>
    /// <remarks>
    /// This is only useful if the account requires authorization for others to hold its issued currencies.
    /// </remarks>
    public TrustSetBuilder Authorize(bool authorize = true)
    {
        Transaction.SetAuthorized = authorize;
        return this;
    }
    
    /// <summary>
    /// Sets whether to freeze the trust line.
    /// </summary>
    /// <param name="freeze">Whether to freeze the trust line.</param>
    /// <returns>The builder for method chaining.</returns>
    public TrustSetBuilder Freeze(bool freeze = true)
    {
        if (freeze)
        {
            Transaction.SetFreeze = true;
            Transaction.ClearFreeze = false;
        }
        else
        {
            Transaction.SetFreeze = false;
            Transaction.ClearFreeze = true;
        }
        
        return this;
    }
    
    /// <summary>
    /// Sets whether to disable rippling through this trust line.
    /// </summary>
    /// <param name="noRipple">Whether to disable rippling.</param>
    /// <returns>The builder for method chaining.</returns>
    public TrustSetBuilder NoRipple(bool noRipple = true)
    {
        Transaction.NoRipple = noRipple;
        return this;
    }
    
    /// <summary>
    /// Validates the trust set transaction.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the transaction is invalid.</exception>
    protected override void Validate()
    {
        base.Validate();
        
        if (Transaction.LimitAmount == null || string.IsNullOrEmpty(Transaction.LimitAmount.Currency) || 
            string.IsNullOrEmpty(Transaction.LimitAmount.Issuer))
        {
            throw new InvalidOperationException("Limit amount with currency and issuer is required for a trust set transaction.");
        }
        
        if (Transaction is { SetFreeze: true, ClearFreeze: true })
        {
            throw new InvalidOperationException("Cannot set both SetFreeze and ClearFreeze flags.");
        }
    }
}