using XRPL.NET.Models.Transactions.Offer;
using XRPL.NET.Transactions.Factory;

namespace XRPL.NET.Transactions.Builders;

/// <summary>
/// Builder for creating offer cancel transactions.
/// </summary>
public class OfferCancelBuilder : BuilderBase<OfferCancelBuilder, OfferCancelTransaction>
{
    /// <summary>
    /// Sets the sequence number of the offer to cancel.
    /// </summary>
    /// <param name="offerSequence">The sequence number of the offer to cancel.</param>
    /// <returns>The builder for method chaining.</returns>
    public OfferCancelBuilder WithOfferSequence(uint offerSequence)
    {
        Transaction.OfferSequence = offerSequence;
        return this;
    }
        
    /// <summary>
    /// Validates the offer cancel transaction.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the transaction is invalid.</exception>
    protected override void Validate()
    {
        base.Validate();
            
        if (Transaction.OfferSequence == 0)
        {
            throw new InvalidOperationException("OfferSequence is required for an offer cancel transaction.");
        }
    }
}