using XRPL.NET.Models.Transactions.Escrow;
using XRPL.NET.Transactions.Factory;

namespace XRPL.NET.Transactions.Builders;

/// <summary>
/// Builder for creating escrow finish transactions.
/// </summary>
public class EscrowFinishBuilder : BuilderBase<EscrowFinishBuilder, EscrowFinishTransaction>
{
    /// <summary>
    /// Sets the owner of the escrow to finish.
    /// </summary>
    /// <param name="owner">The account address of the escrow owner.</param>
    /// <returns>The builder for method chaining.</returns>
    public EscrowFinishBuilder WithOwner(string owner)
    {
        Transaction.Owner = owner;
        return this;
    }

    /// <summary>
    /// Sets the sequence number of the escrow to finish.
    /// </summary>
    /// <param name="offerSequence">The sequence number of the escrow.</param>
    /// <returns>The builder for method chaining.</returns>
    public EscrowFinishBuilder WithOfferSequence(uint offerSequence)
    {
        Transaction.OfferSequence = offerSequence;
        return this;
    }

    /// <summary>
    /// Sets the condition to fulfill the escrow.
    /// </summary>
    /// <param name="condition">The cryptographic condition in hexadecimal format.</param>
    /// <returns>The builder for method chaining.</returns>
    public EscrowFinishBuilder WithCondition(string condition)
    {
        Transaction.Condition = condition;
        return this;
    }

    /// <summary>
    /// Sets the fulfillment to complete the escrow.
    /// </summary>
    /// <param name="fulfillment">The cryptographic fulfillment in hexadecimal format.</param>
    /// <returns>The builder for method chaining.</returns>
    public EscrowFinishBuilder WithFulfillment(string fulfillment)
    {
        Transaction.Fulfillment = fulfillment;
        return this;
    }

    /// <summary>
    /// Validates the escrow finish transaction.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the transaction is invalid.</exception>
    protected override void Validate()
    {
        base.Validate();

        if (string.IsNullOrEmpty(Transaction.Owner))
        {
            throw new InvalidOperationException("Owner is required for an escrow finish transaction.");
        }

        if (Transaction.OfferSequence == 0)
        {
            throw new InvalidOperationException("OfferSequence is required for an escrow finish transaction.");
        }
    }
}