using XRPL.NET.Models.Transactions.Common;
using XRPL.NET.Models.Transactions.NFToken;
using XRPL.NET.Transactions.Factory;

namespace XRPL.NET.Transactions.Builders;

/// <summary>
/// Builder for creating NFToken cancel offer transactions.
/// </summary>
public class NFTokenCancelOfferBuilder : BuilderBase<NFTokenCancelOfferBuilder, NFTokenCancelOfferTransaction>
{
    /// <summary>
    /// Adds an NFToken offer to cancel.
    /// </summary>
    /// <param name="offerToCancel">The ID of the offer to cancel.</param>
    /// <returns>The builder for method chaining.</returns>
    public NFTokenCancelOfferBuilder AddOffer(string offerToCancel)
    {
        Transaction.AddOffer(offerToCancel);
        return this;
    }

    /// <summary>
    /// Adds multiple NFToken offers to cancel.
    /// </summary>
    /// <param name="offersToCancel">The IDs of offers to cancel.</param>
    /// <returns>The builder for method chaining.</returns>
    public NFTokenCancelOfferBuilder AddOffers(IEnumerable<string> offersToCancel)
    {
        foreach (var offer in offersToCancel)
        {
            Transaction.AddOffer(offer);
        }
        return this;
    }

    /// <summary>
    /// Validates the NFToken cancel offer transaction.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the transaction is invalid.</exception>
    protected override void Validate()
    {
        base.Validate();

        if (Transaction.Offers == null || Transaction.Offers.Count == 0)
        {
            throw new InvalidOperationException("At least one offer must be specified for an NFToken cancel offer transaction.");
        }
    }
}