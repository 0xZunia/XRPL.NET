using XRPL.NET.Models.Transactions.Common;
using XRPL.NET.Models.Transactions.Payment;
using XRPL.NET.Transactions.Factory;

namespace XRPL.NET.Transactions.Builders;

/// <summary>
/// Builder for creating payment transactions.
/// </summary>
public class PaymentBuilder : BuilderBase<PaymentBuilder, PaymentTransaction>
{
    /// <summary>
    /// Sets the destination account for the payment.
    /// </summary>
    /// <param name="destination">The destination account address.</param>
    /// <returns>The builder for method chaining.</returns>
    public PaymentBuilder WithDestination(string destination)
    {
        Transaction.Destination = destination;
        return this;
    }
    
    /// <summary>
    /// Sets the amount to send as XRP.
    /// </summary>
    /// <param name="xrpAmount">The amount of XRP to send.</param>
    /// <returns>The builder for method chaining.</returns>
    public PaymentBuilder WithAmount(decimal xrpAmount)
    {
        Transaction.Amount = XrpAmount.ToDrops(xrpAmount);
        return this;
    }
    
    /// <summary>
    /// Sets the amount to send as XRP using drops.
    /// </summary>
    /// <param name="drops">The amount of XRP to send in drops.</param>
    /// <returns>The builder for method chaining.</returns>
    public PaymentBuilder WithAmountInDrops(string drops)
    {
        Transaction.Amount = drops;
        return this;
    }
    
    /// <summary>
    /// Sets the amount to send as an issued currency.
    /// </summary>
    /// <param name="currency">The currency code.</param>
    /// <param name="issuer">The issuer account.</param>
    /// <param name="value">The amount value.</param>
    /// <returns>The builder for method chaining.</returns>
    public PaymentBuilder WithAmount(string currency, string issuer, decimal value)
    {
        Transaction.Amount = new IssuedCurrencyAmount(currency, issuer, value);
        return this;
    }
    
    /// <summary>
    /// Sets the amount to send as an issued currency.
    /// </summary>
    /// <param name="issuedCurrencyAmount">The issued currency amount.</param>
    /// <returns>The builder for method chaining.</returns>
    public PaymentBuilder WithAmount(IssuedCurrencyAmount issuedCurrencyAmount)
    {
        Transaction.Amount = issuedCurrencyAmount;
        return this;
    }
    
    /// <summary>
    /// Sets the maximum amount to send as XRP.
    /// </summary>
    /// <param name="xrpAmount">The maximum amount of XRP to send.</param>
    /// <returns>The builder for method chaining.</returns>
    public PaymentBuilder WithSendMax(decimal xrpAmount)
    {
        Transaction.SendMax = XrpAmount.ToDrops(xrpAmount);
        return this;
    }
    
    /// <summary>
    /// Sets the maximum amount to send as XRP using drops.
    /// </summary>
    /// <param name="drops">The maximum amount of XRP to send in drops.</param>
    /// <returns>The builder for method chaining.</returns>
    public PaymentBuilder WithSendMaxInDrops(string drops)
    {
        Transaction.SendMax = drops;
        return this;
    }
    
    /// <summary>
    /// Sets the maximum amount to send as an issued currency.
    /// </summary>
    /// <param name="currency">The currency code.</param>
    /// <param name="issuer">The issuer account.</param>
    /// <param name="value">The amount value.</param>
    /// <returns>The builder for method chaining.</returns>
    public PaymentBuilder WithSendMax(string currency, string issuer, decimal value)
    {
        Transaction.SendMax = new IssuedCurrencyAmount(currency, issuer, value);
        return this;
    }
    
    /// <summary>
    /// Sets the maximum amount to send as an issued currency.
    /// </summary>
    /// <param name="issuedCurrencyAmount">The issued currency amount.</param>
    /// <returns>The builder for method chaining.</returns>
    public PaymentBuilder WithSendMax(IssuedCurrencyAmount issuedCurrencyAmount)
    {
        Transaction.SendMax = issuedCurrencyAmount;
        return this;
    }
    
    /// <summary>
    /// Sets the minimum amount to deliver as XRP.
    /// </summary>
    /// <param name="xrpAmount">The minimum amount of XRP to deliver.</param>
    /// <returns>The builder for method chaining.</returns>
    public PaymentBuilder WithDeliverMin(decimal xrpAmount)
    {
        Transaction.DeliverMin = XrpAmount.ToDrops(xrpAmount);
        Transaction.PartialPayment = true;
        return this;
    }
    
    /// <summary>
    /// Sets the minimum amount to deliver as XRP using drops.
    /// </summary>
    /// <param name="drops">The minimum amount of XRP to deliver in drops.</param>
    /// <returns>The builder for method chaining.</returns>
    public PaymentBuilder WithDeliverMinInDrops(string drops)
    {
        Transaction.DeliverMin = drops;
        Transaction.PartialPayment = true;
        return this;
    }
    
    /// <summary>
    /// Sets the minimum amount to deliver as an issued currency.
    /// </summary>
    /// <param name="currency">The currency code.</param>
    /// <param name="issuer">The issuer account.</param>
    /// <param name="value">The amount value.</param>
    /// <returns>The builder for method chaining.</returns>
    public PaymentBuilder WithDeliverMin(string currency, string issuer, decimal value)
    {
        Transaction.DeliverMin = new IssuedCurrencyAmount(currency, issuer, value);
        Transaction.PartialPayment = true;
        return this;
    }
    
    /// <summary>
    /// Sets the minimum amount to deliver as an issued currency.
    /// </summary>
    /// <param name="issuedCurrencyAmount">The issued currency amount.</param>
    /// <returns>The builder for method chaining.</returns>
    public PaymentBuilder WithDeliverMin(IssuedCurrencyAmount issuedCurrencyAmount)
    {
        Transaction.DeliverMin = issuedCurrencyAmount;
        Transaction.PartialPayment = true;
        return this;
    }
    
    /// <summary>
    /// Adds payment paths to the transaction.
    /// </summary>
    /// <param name="paths">The payment paths.</param>
    /// <returns>The builder for method chaining.</returns>
    public PaymentBuilder WithPaths(List<List<PathStep>> paths)
    {
        Transaction.Paths = paths;
        return this;
    }
    
    /// <summary>
    /// Adds a single payment path to the transaction.
    /// </summary>
    /// <param name="path">The payment path.</param>
    /// <returns>The builder for method chaining.</returns>
    public PaymentBuilder WithPath(List<PathStep> path)
    {
        Transaction.AddPath(path);
        return this;
    }
    
    /// <summary>
    /// Sets the partial payment flag.
    /// </summary>
    /// <param name="partialPayment">Whether to allow partial payments.</param>
    /// <returns>The builder for method chaining.</returns>
    public PaymentBuilder AllowPartialPayment(bool partialPayment = true)
    {
        Transaction.PartialPayment = partialPayment;
        return this;
    }
    
    /// <summary>
    /// Sets the limit quality flag.
    /// </summary>
    /// <param name="limitQuality">Whether to limit exchanges to the best quality.</param>
    /// <returns>The builder for method chaining.</returns>
    public PaymentBuilder LimitQuality(bool limitQuality = true)
    {
        Transaction.LimitQuality = limitQuality;
        return this;
    }
    
    /// <summary>
    /// Sets the no direct ripple flag.
    /// </summary>
    /// <param name="noDirectRipple">Whether to disallow direct rippling between source and destination.</param>
    /// <returns>The builder for method chaining.</returns>
    public PaymentBuilder NoDirectRipple(bool noDirectRipple = true)
    {
        Transaction.NoDirectRipple = noDirectRipple;
        return this;
    }
    
    /// <summary>
    /// Validates the payment transaction.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the transaction is invalid.</exception>
    protected override void Validate()
    {
        base.Validate();
        
        if (string.IsNullOrEmpty(Transaction.Destination))
        {
            throw new InvalidOperationException("Destination is required for a payment transaction.");
        }
        
        if (Transaction.Amount == null)
        {
            throw new InvalidOperationException("Amount is required for a payment transaction.");
        }
        
        if (Transaction.DeliverMin != null && !Transaction.PartialPayment)
        {
            throw new InvalidOperationException("DeliverMin requires the PartialPayment flag to be set.");
        }
    }
}