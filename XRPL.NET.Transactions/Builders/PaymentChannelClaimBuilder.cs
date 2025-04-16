using XRPL.NET.Models.Transactions.Common;
using XRPL.NET.Models.Transactions.PaymentChannel;
using XRPL.NET.Transactions.Factory;

namespace XRPL.NET.Transactions.Builders;

/// <summary>
/// Builder for creating payment channel claim transactions.
/// </summary>
public class PaymentChannelClaimBuilder : BuilderBase<PaymentChannelClaimBuilder, PaymentChannelClaimTransaction>
{
    /// <summary>
    /// Sets the payment channel to claim from.
    /// </summary>
    /// <param name="channelId">The ID of the payment channel to claim from.</param>
    /// <returns>The builder for method chaining.</returns>
    public PaymentChannelClaimBuilder WithChannel(string channelId)
    {
        Transaction.Channel = channelId;
        return this;
    }

    /// <summary>
    /// Sets the amount to claim from the channel as XRP.
    /// </summary>
    /// <param name="xrpAmount">The amount of XRP to claim.</param>
    /// <returns>The builder for method chaining.</returns>
    public PaymentChannelClaimBuilder WithAmount(decimal xrpAmount)
    {
        Transaction.Amount = XrpAmount.ToDrops(xrpAmount);
        return this;
    }

    /// <summary>
    /// Sets the amount to claim from the channel as XRP in drops.
    /// </summary>
    /// <param name="drops">The amount of XRP in drops.</param>
    /// <returns>The builder for method chaining.</returns>
    public PaymentChannelClaimBuilder WithAmountInDrops(string drops)
    {
        Transaction.Amount = drops;
        return this;
    }

    /// <summary>
    /// Sets the signature that authorizes the claim.
    /// </summary>
    /// <param name="signature">The signature authorizing the claim.</param>
    /// <returns>The builder for method chaining.</returns>
    public PaymentChannelClaimBuilder WithSignature(string signature)
    {
        Transaction.Signature = signature;
        return this;
    }

    /// <summary>
    /// Sets the public key used to sign the claim.
    /// </summary>
    /// <param name="publicKey">The public key used to sign the claim.</param>
    /// <returns>The builder for method chaining.</returns>
    public PaymentChannelClaimBuilder WithPublicKey(string publicKey)
    {
        Transaction.PublicKey = publicKey;
        return this;
    }

    /// <summary>
    /// Sets a flag to close the channel after claiming.
    /// </summary>
    /// <param name="close">Whether to close the channel.</param>
    /// <returns>The builder for method chaining.</returns>
    public PaymentChannelClaimBuilder Close(bool close = true)
    {
        Transaction.Close = close;
        return this;
    }

    /// <summary>
    /// Validates the payment channel claim transaction.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the transaction is invalid.</exception>
    protected override void Validate()
    {
        base.Validate();

        if (string.IsNullOrEmpty(Transaction.Channel))
        {
            throw new InvalidOperationException("Channel is required for a payment channel claim transaction.");
        }
    }
}