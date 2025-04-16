using XRPL.NET.Models.Transactions.Common;
using XRPL.NET.Models.Transactions.NFToken;
using XRPL.NET.Transactions.Factory;

namespace XRPL.NET.Transactions.Builders;

/// <summary>
/// Builder for creating NFToken burn transactions.
/// </summary>
public class NFTokenBurnBuilder : BuilderBase<NFTokenBurnBuilder, NFTokenBurnTransaction>
{
    /// <summary>
    /// Sets the NFToken ID to burn.
    /// </summary>
    /// <param name="tokenId">The unique identifier of the NFToken to burn.</param>
    /// <returns>The builder for method chaining.</returns>
    public NFTokenBurnBuilder WithTokenId(string tokenId)
    {
        Transaction.TokenId = tokenId;
        return this;
    }

    /// <summary>
    /// Sets the owner of the NFToken being burned.
    /// </summary>
    /// <param name="owner">The account address of the NFToken owner.</param>
    /// <returns>The builder for method chaining.</returns>
    public NFTokenBurnBuilder WithOwner(string owner)
    {
        Transaction.Owner = owner;
        return this;
    }

    /// <summary>
    /// Validates the NFToken burn transaction.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the transaction is invalid.</exception>
    protected override void Validate()
    {
        base.Validate();

        if (string.IsNullOrEmpty(Transaction.TokenId))
        {
            throw new InvalidOperationException("TokenId is required for an NFToken burn transaction.");
        }
    }
}