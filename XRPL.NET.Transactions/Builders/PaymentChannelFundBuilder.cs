using XRPL.NET.Models.Transactions.Common;
using XRPL.NET.Models.Transactions.PaymentChannel;
using XRPL.NET.Transactions.Factory;

namespace XRPL.NET.Transactions.Builders;

/// <summary>
/// Builder for creating payment channel fund transactions.
/// </summary>
public class PaymentChannelFundBuilder : BuilderBase<PaymentChannelFundBuilder, PaymentChannelFundTransaction>
{
    /// <summary>
    /// Sets the payment channel to fund.
    /// </summary>
    /// <param name="channelId">The ID of the payment channel to fund.</param>
    /// <returns>The builder for method chaining.</returns>
    public PaymentChannelFundBuilder WithChannel(string channelId)
    {
        Transaction.Channel = channelId;
        return this;
    }

    /// <summary>
    /// Sets the amount to add to the payment channel as XRP.
    /// </summary>
    /// <param name="xrpAmount">The amount of XRP to add.</param>
    /// <returns>The builder for method chaining.</returns>
    public PaymentChannelFundBuilder WithAmount(decimal xrpAmount)
    {
        Transaction.Amount = XrpAmount.ToDrops(xrpAmount);
        return this;
    }

    /// <summary>
    /// Sets the amount to add to the payment channel as XRP in drops.
    /// </summary>
    /// <param name="drops">The amount of XRP in drops.</param>
    /// <returns>The builder for method chaining.</returns>
    public PaymentChannelFundBuilder WithAmountInDrops(string drops)
    {
        Transaction.Amount = drops;
        return this;
    }

    /// <summary>
    /// Sets the public key of the channel, used when adding funds to a channel.
    /// </summary>
    /// <param name="publicKey">The public key of the channel.</param>
    /// <returns>The builder for method chaining.</returns>
    public PaymentChannelFundBuilder WithPublicKey(string publicKey)
    {
        Transaction.PublicKey = publicKey;
        return this;
    }

    /// <summary>
    /// Validates the payment channel fund transaction.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the transaction is invalid.</exception>
    protected override void Validate()
    {
        base.Validate();

        if (string.IsNullOrEmpty(Transaction.Channel))
        {
            throw new InvalidOperationException("Channel is required for a payment channel fund transaction.");
        }

        if (string.IsNullOrEmpty(Transaction.Amount))
        {
            throw new InvalidOperationException("Amount is required for a payment channel fund transaction.");
        }
    }
}