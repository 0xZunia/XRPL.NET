using XRPL.NET.Models.Transactions.Common;
using XRPL.NET.Models.Transactions.Offer;
using XRPL.NET.Transactions.Factory;

namespace XRPL.NET.Transactions.Builders;

/// <summary>
/// Builder for creating offer create transactions.
/// </summary>
public class OfferCreateBuilder : BuilderBase<OfferCreateBuilder, OfferCreateTransaction>
{
    /// <summary>
    /// Sets the amount the taker pays (what you're offering) as XRP.
    /// </summary>
    /// <param name="xrpAmount">The amount of XRP.</param>
    /// <returns>The builder for method chaining.</returns>
    public OfferCreateBuilder TakerPaysXrp(decimal xrpAmount)
    {
        Transaction.TakerPays = XrpAmount.ToDrops(xrpAmount);
        return this;
    }
    
    /// <summary>
    /// Sets the amount the taker pays (what you're offering) as XRP using drops.
    /// </summary>
    /// <param name="drops">The amount of XRP in drops.</param>
    /// <returns>The builder for method chaining.</returns>
    public OfferCreateBuilder TakerPaysDrops(string drops)
    {
        Transaction.TakerPays = drops;
        return this;
    }
    
    /// <summary>
    /// Sets the amount the taker pays (what you're offering) as an issued currency.
    /// </summary>
    /// <param name="currency">The currency code.</param>
    /// <param name="issuer">The issuer account.</param>
    /// <param name="value">The amount value.</param>
    /// <returns>The builder for method chaining.</returns>
    public OfferCreateBuilder TakerPaysIssuedCurrency(string currency, string issuer, decimal value)
    {
        Transaction.TakerPays = new IssuedCurrencyAmount(currency, issuer, value);
        return this;
    }
    
    /// <summary>
    /// Sets the amount the taker pays (what you're offering) as an issued currency.
    /// </summary>
    /// <param name="issuedCurrencyAmount">The issued currency amount.</param>
    /// <returns>The builder for method chaining.</returns>
    public OfferCreateBuilder TakerPaysIssuedCurrency(IssuedCurrencyAmount issuedCurrencyAmount)
    {
        Transaction.TakerPays = issuedCurrencyAmount;
        return this;
    }
    
    /// <summary>
    /// Sets the amount the taker gets (what you're requesting) as XRP.
    /// </summary>
    /// <param name="xrpAmount">The amount of XRP.</param>
    /// <returns>The builder for method chaining.</returns>
    public OfferCreateBuilder TakerGetsXrp(decimal xrpAmount)
    {
        Transaction.TakerGets = XrpAmount.ToDrops(xrpAmount);
        return this;
    }
    
    /// <summary>
    /// Sets the amount the taker gets (what you're requesting) as XRP using drops.
    /// </summary>
    /// <param name="drops">The amount of XRP in drops.</param>
    /// <returns>The builder for method chaining.</returns>
    public OfferCreateBuilder TakerGetsDrops(string drops)
    {
        Transaction.TakerGets = drops;
        return this;
    }
    
    /// <summary>
    /// Sets the amount the taker gets (what you're requesting) as an issued currency.
    /// </summary>
    /// <param name="currency">The currency code.</param>
    /// <param name="issuer">The issuer account.</param>
    /// <param name="value">The amount value.</param>
    /// <returns>The builder for method chaining.</returns>
    public OfferCreateBuilder TakerGetsIssuedCurrency(string currency, string issuer, decimal value)
    {
        Transaction.TakerGets = new IssuedCurrencyAmount(currency, issuer, value);
        return this;
    }
    
    /// <summary>
    /// Sets the amount the taker gets (what you're requesting) as an issued currency.
    /// </summary>
    /// <param name="issuedCurrencyAmount">The issued currency amount.</param>
    /// <returns>The builder for method chaining.</returns>
    public OfferCreateBuilder TakerGetsIssuedCurrency(IssuedCurrencyAmount issuedCurrencyAmount)
    {
        Transaction.TakerGets = issuedCurrencyAmount;
        return this;
    }
    
    /// <summary>
    /// Sets the expiration time for the offer.
    /// </summary>
    /// <param name="expirationTime">The expiration time.</param>
    /// <returns>The builder for method chaining.</returns>
    public OfferCreateBuilder WithExpiration(DateTime expirationTime)
    {
        var rippleEpoch = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        TimeSpan timeSpan = expirationTime.ToUniversalTime() - rippleEpoch;
        uint expiration = (uint)timeSpan.TotalSeconds;
        
        Transaction.Expiration = expiration;
        return this;
    }
    
    /// <summary>
    /// Sets the expiration time for the offer in Ripple epoch seconds.
    /// </summary>
    /// <param name="expirationEpochSeconds">The expiration time in Ripple epoch seconds.</param>
    /// <returns>The builder for method chaining.</returns>
    public OfferCreateBuilder WithExpirationEpoch(uint expirationEpochSeconds)
    {
        Transaction.Expiration = expirationEpochSeconds;
        return this;
    }
    
    /// <summary>
    /// Sets the sequence number of an existing offer to replace.
    /// </summary>
    /// <param name="offerSequence">The sequence number of the offer to replace.</param>
    /// <returns>The builder for method chaining.</returns>
    public OfferCreateBuilder WithOfferSequence(uint offerSequence)
    {
        Transaction.OfferSequence = offerSequence;
        return this;
    }
    
    /// <summary>
    /// Sets the passive flag.
    /// </summary>
    /// <param name="passive">Whether the offer is passive.</param>
    /// <returns>The builder for method chaining.</returns>
    public OfferCreateBuilder Passive(bool passive = true)
    {
        Transaction.Passive = passive;
        return this;
    }
    
    /// <summary>
    /// Sets the immediate or cancel flag.
    /// </summary>
    /// <param name="immediateOrCancel">Whether the offer is immediate or cancel.</param>
    /// <returns>The builder for method chaining.</returns>
    public OfferCreateBuilder ImmediateOrCancel(bool immediateOrCancel = true)
    {
        Transaction.ImmediateOrCancel = immediateOrCancel;
        return this;
    }
    
    /// <summary>
    /// Sets the fill or kill flag.
    /// </summary>
    /// <param name="fillOrKill">Whether the offer is fill or kill.</param>
    /// <returns>The builder for method chaining.</returns>
    public OfferCreateBuilder FillOrKill(bool fillOrKill = true)
    {
        Transaction.FillOrKill = fillOrKill;
        return this;
    }
    
    /// <summary>
    /// Sets the sell flag, which treats the offer as a sell offer.
    /// </summary>
    /// <param name="sell">Whether the offer is a sell offer.</param>
    /// <returns>The builder for method chaining.</returns>
    public OfferCreateBuilder Sell(bool sell = true)
    {
        Transaction.Sell = sell;
        return this;
    }
    
    /// <summary>
    /// Validates the offer create transaction.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the transaction is invalid.</exception>
    protected override void Validate()
    {
        base.Validate();
        
        if (Transaction.TakerPays == null)
        {
            throw new InvalidOperationException("TakerPays is required for an offer create transaction.");
        }
        
        if (Transaction.TakerGets == null)
        {
            throw new InvalidOperationException("TakerGets is required for an offer create transaction.");
        }
        
        if (Transaction.ImmediateOrCancel && Transaction.FillOrKill)
        {
            throw new InvalidOperationException("Cannot set both ImmediateOrCancel and FillOrKill flags.");
        }
    }
}