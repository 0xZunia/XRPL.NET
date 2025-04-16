using XRPL.NET.Models.Transactions.Common;
using XRPL.NET.Models.Transactions.NFToken;
using XRPL.NET.Transactions.Factory;

namespace XRPL.NET.Transactions.Builders;

/// <summary>
/// Builder for creating NFToken accept offer transactions.
/// </summary>
public class NFTokenAcceptOfferBuilder : BuilderBase<NFTokenAcceptOfferBuilder, NFTokenAcceptOfferTransaction>
{
    /// <summary>
    /// Sets the NFToken sell offer to accept.
    /// </summary>
    /// <param name="sellOffer">The sell offer ID to accept.</param>
    /// <returns>The builder for method chaining.</returns>
    public NFTokenAcceptOfferBuilder WithSellOffer(string sellOffer)
    {
        Transaction.SellOffer = sellOffer;
        return this;
    }

    /// <summary>
    /// Sets the NFToken buy offer to accept.
    /// </summary>
    /// <param name="buyOffer">The buy offer ID to accept.</param>
    /// <returns>The builder for method chaining.</returns>
    public NFTokenAcceptOfferBuilder WithBuyOffer(string buyOffer)
    {
        Transaction.BuyOffer = buyOffer;
        return this;
    }

    /// <summary>
    /// Validates the NFToken accept offer transaction.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the transaction is invalid.</exception>
    protected override void Validate()
    {
        base.Validate();

        if (string.IsNullOrEmpty(Transaction.SellOffer) && string.IsNullOrEmpty(Transaction.BuyOffer))
        {
            throw new InvalidOperationException("Either SellOffer or BuyOffer must be specified for an NFToken accept offer transaction.");
        }

        if (!string.IsNullOrEmpty(Transaction.SellOffer) && !string.IsNullOrEmpty(Transaction.BuyOffer))
        {
            throw new InvalidOperationException("Cannot specify both SellOffer and BuyOffer in a single NFToken accept offer transaction.");
        }
    }
}