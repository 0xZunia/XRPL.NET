using XRPL.NET.Models.Transactions.Common;
using XRPL.NET.Models.Transactions.NFToken;
using XRPL.NET.Transactions.Factory;

namespace XRPL.NET.Transactions.Builders;

/// <summary>
/// Builder for creating NFToken offer transactions.
/// </summary>
public class NFTokenCreateOfferBuilder : BuilderBase<NFTokenCreateOfferBuilder, NFTokenCreateOfferTransaction>
{
    /// <summary>
    /// Sets the NFToken ID for the offer.
    /// </summary>
    /// <param name="tokenId">The unique identifier of the NFToken.</param>
    /// <returns>The builder for method chaining.</returns>
    public NFTokenCreateOfferBuilder WithTokenId(string tokenId)
    {
        Transaction.TokenId = tokenId;
        return this;
    }

    /// <summary>
    /// Sets the destination account for the offer.
    /// </summary>
    /// <param name="destination">The account address of the offer recipient.</param>
    /// <returns>The builder for method chaining.</returns>
    public NFTokenCreateOfferBuilder WithDestination(string destination)
    {
        Transaction.Destination = destination;
        return this;
    }

    /// <summary>
    /// Sets the amount of the offer in XRP.
    /// </summary>
    /// <param name="xrpAmount">The amount of XRP for the offer.</param>
    /// <returns>The builder for method chaining.</returns>
    public NFTokenCreateOfferBuilder WithAmount(decimal xrpAmount)
    {
        Transaction.Amount = XrpAmount.ToDrops(xrpAmount);
        return this;
    }

    /// <summary>
    /// Sets the amount of the offer in drops.
    /// </summary>
    /// <param name="drops">The amount in drops.</param>
    /// <returns>The builder for method chaining.</returns>
    public NFTokenCreateOfferBuilder WithAmountInDrops(string drops)
    {
        Transaction.Amount = drops;
        return this;
    }

    /// <summary>
    /// Indicates this is a sell offer.
    /// </summary>
    /// <param name="isSellOffer">Whether this is a sell offer.</param>
    /// <returns>The builder for method chaining.</returns>
    public NFTokenCreateOfferBuilder SellOffer(bool isSellOffer = true)
    {
        if (isSellOffer)
        {
            Transaction.Flags |= NFTokenCreateOfferFlags.SellOffer;
        }
        else
        {
            Transaction.Flags &= ~NFTokenCreateOfferFlags.SellOffer;
        }
        return this;
    }

    /// <summary>
    /// Validates the NFToken create offer transaction.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the transaction is invalid.</exception>
    protected override void Validate()
    {
        base.Validate();

        if (string.IsNullOrEmpty(Transaction.TokenId))
        {
            throw new InvalidOperationException("TokenId is required for an NFToken create offer transaction.");
        }

        if (string.IsNullOrEmpty(Transaction.Amount))
        {
            throw new InvalidOperationException("Amount is required for an NFToken create offer transaction.");
        }
    }
}

/// <summary>
/// Defines flags for NFToken create offer transactions.
/// </summary>
public static class NFTokenCreateOfferFlags
{
    /// <summary>
    /// Indicates this is a sell offer.
    /// </summary>
    public const uint SellOffer = 0x00000001;
}