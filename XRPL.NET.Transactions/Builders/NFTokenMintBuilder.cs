using XRPL.NET.Models.Transactions.Common;
using XRPL.NET.Models.Transactions.NFToken;
using XRPL.NET.Transactions.Factory;

namespace XRPL.NET.Transactions.Builders;

/// <summary>
/// Builder for creating NFToken mint transactions.
/// </summary>
public class NFTokenMintBuilder : BuilderBase<NFTokenMintBuilder, NFTokenMintTransaction>
{
    /// <summary>
    /// Sets the URI of the NFT.
    /// </summary>
    /// <param name="uri">The URI of the NFT, encoded as a hexadecimal string.</param>
    /// <returns>The builder for method chaining.</returns>
    public NFTokenMintBuilder WithUri(string uri)
    {
        Transaction.Uri = uri;
        return this;
    }

    /// <summary>
    /// Sets the transfer fee for the NFT.
    /// </summary>
    /// <param name="transferFee">The transfer fee as a percentage (0-50).</param>
    /// <returns>The builder for method chaining.</returns>
    public NFTokenMintBuilder WithTransferFee(decimal transferFee)
    {
        if (transferFee < 0 || transferFee > 50)
        {
            throw new ArgumentOutOfRangeException(nameof(transferFee), "Transfer fee must be between 0 and 50 percent.");
        }

        // Convert percentage to billionths (0% = 0, 50% = 50000000)
        Transaction.TransferFee = (uint)(transferFee * 1_000_000);
        return this;
    }

    /// <summary>
    /// Sets the NFToken taxon.
    /// </summary>
    /// <param name="taxon">The NFToken taxon.</param>
    /// <returns>The builder for method chaining.</returns>
    public NFTokenMintBuilder WithTaxon(uint taxon)
    {
        Transaction.TokenTaxon = taxon;
        return this;
    }

    /// <summary>
    /// Indicates that the NFT is transferable.
    /// </summary>
    /// <param name="transferable">Whether the NFT is transferable.</param>
    /// <returns>The builder for method chaining.</returns>
    public NFTokenMintBuilder Transferable(bool transferable = true)
    {
        if (transferable)
        {
            Transaction.Flags |= NFTokenMintFlags.Transferable;
        }
        else
        {
            Transaction.Flags &= ~NFTokenMintFlags.Transferable;
        }
        return this;
    }

    /// <summary>
    /// Indicates that the NFT can only be burned by the issuer.
    /// </summary>
    /// <param name="onlyIssuer">Whether only the issuer can burn the NFT.</param>
    /// <returns>The builder for method chaining.</returns>
    public NFTokenMintBuilder OnlyIssuerCanBurn(bool onlyIssuer = true)
    {
        if (onlyIssuer)
        {
            Transaction.Flags |= NFTokenMintFlags.OnlyIssuerCanBurn;
        }
        else
        {
            Transaction.Flags &= ~NFTokenMintFlags.OnlyIssuerCanBurn;
        }
        return this;
    }

    /// <summary>
    /// Validates the NFToken mint transaction.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the transaction is invalid.</exception>
    protected override void Validate()
    {
        base.Validate();

        if (Transaction.TokenTaxon == 0)
        {
            throw new InvalidOperationException("TokenTaxon is required for an NFToken mint transaction.");
        }
    }
}

/// <summary>
/// Defines flags for NFToken mint transactions.
/// </summary>
public static class NFTokenMintFlags
{
    /// <summary>
    /// Indicates the token is transferable.
    /// </summary>
    public const uint Transferable = 0x00000001;

    /// <summary>
    /// Indicates only the issuer can burn the token.
    /// </summary>
    public const uint OnlyIssuerCanBurn = 0x00000002;
}