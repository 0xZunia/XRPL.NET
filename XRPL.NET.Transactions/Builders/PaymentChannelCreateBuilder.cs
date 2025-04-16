using XRPL.NET.Models.Transactions.Common;
using XRPL.NET.Models.Transactions.PaymentChannel;
using XRPL.NET.Transactions.Factory;

namespace XRPL.NET.Transactions.Builders;

/// <summary>
/// Builder for creating payment channel transactions.
/// </summary>
public class PaymentChannelCreateBuilder : BuilderBase<PaymentChannelCreateBuilder, PaymentChannelCreateTransaction>
{
    /// <summary>
    /// Sets the destination account for the payment channel.
    /// </summary>
    /// <param name="destination">The destination account address.</param>
    /// <returns>The builder for method chaining.</returns>
    public PaymentChannelCreateBuilder WithDestination(string destination)
    {
        Transaction.Destination = destination;
        return this;
    }

    /// <summary>
    /// Sets the amount to allocate to the payment channel as XRP.
    /// </summary>
    /// <param name="xrpAmount">The amount of XRP to allocate.</param>
    /// <returns>The builder for method chaining.</returns>
    public PaymentChannelCreateBuilder WithAmount(decimal xrpAmount)
    {
        Transaction.Amount = XrpAmount.ToDrops(xrpAmount);
        return this;
    }

    /// <summary>
    /// Sets the amount to allocate to the payment channel as XRP in drops.
    /// </summary>
    /// <param name="drops">The amount of XRP in drops.</param>
    /// <returns>The builder for method chaining.</returns>
    public PaymentChannelCreateBuilder WithAmountInDrops(string drops)
    {
        Transaction.Amount = drops;
        return this;
    }

    /// <summary>
    /// Sets the public key of the channel. This can authorize drops to be sent from the channel.
    /// </summary>
    /// <param name="publicKey">The public key of the channel.</param>
    /// <returns>The builder for method chaining.</returns>
    public PaymentChannelCreateBuilder WithPublicKey(string publicKey)
    {
        Transaction.PublicKey = publicKey;
        return this;
    }

    /// <summary>
    /// Sets the settlement delay for the payment channel.
    /// </summary>
    /// <param name="settleDelay">The number of seconds to wait before the channel can be closed.</param>
    /// <returns>The builder for method chaining.</returns>
    public PaymentChannelCreateBuilder WithSettleDelay(uint settleDelay)
    {
        Transaction.SettleDelay = settleDelay;
        return this;
    }

    /// <summary>
    /// Validates the payment channel create transaction.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the transaction is invalid.</exception>
    protected override void Validate()
    {
        base.Validate();

        if (string.IsNullOrEmpty(Transaction.Destination))
        {
            throw new InvalidOperationException("Destination is required for a payment channel create transaction.");
        }

        if (string.IsNullOrEmpty(Transaction.Amount))
        {
            throw new InvalidOperationException("Amount is required for a payment channel create transaction.");
        }

        if (string.IsNullOrEmpty(Transaction.PublicKey))
        {
            throw new InvalidOperationException("PublicKey is required for a payment channel create transaction.");
        }
    }
}