using XRPL.NET.Models.Transactions.Common;
using XRPL.NET.Models.Transactions.Escrow;
using XRPL.NET.Transactions.Factory;

namespace XRPL.NET.Transactions.Builders;

/// <summary>
/// Builder for creating escrow cancel transactions.
/// </summary>
public class EscrowCancelBuilder : BuilderBase<EscrowCancelBuilder, EscrowCancelTransaction>
{
    /// <summary>
    /// Sets the owner of the escrow to cancel.
    /// </summary>
    /// <param name="owner">The account address of the escrow owner.</param>
    /// <returns>The builder for method chaining.</returns>
    public EscrowCancelBuilder WithOwner(string owner)
    {
        Transaction.Owner = owner;
        return this;
    }

    /// <summary>
    /// Sets the sequence number of the escrow to cancel.
    /// </summary>
    /// <param name="offerSequence">The sequence number of the escrow.</param>
    /// <returns>The builder for method chaining.</returns>
    public EscrowCancelBuilder WithOfferSequence(uint offerSequence)
    {
        Transaction.OfferSequence = offerSequence;
        return this;
    }

    /// <summary>
    /// Validates the escrow cancel transaction.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the transaction is invalid.</exception>
    protected override void Validate()
    {
        base.Validate();

        if (string.IsNullOrEmpty(Transaction.Owner))
        {
            throw new InvalidOperationException("Owner is required for an escrow cancel transaction.");
        }

        if (Transaction.OfferSequence == 0)
        {
            throw new InvalidOperationException("OfferSequence is required for an escrow cancel transaction.");
        }
    }
}