using XRPL.NET.Transactions.Builders;

namespace XRPL.NET.Transactions.Factory;

/// <summary>
/// Factory for creating transaction builders.
/// </summary>
/// <remarks>
/// This factory provides a convenient entry point for creating different types of transactions
/// using a fluent builder pattern.
/// </remarks>
public static class TransactionFactory
{
    /// <summary>
    /// Creates a payment transaction builder.
    /// </summary>
    /// <returns>A payment transaction builder.</returns>
    public static PaymentBuilder Payment()
    {
        return new PaymentBuilder();
    }
    
    /// <summary>
    /// Creates an account set transaction builder.
    /// </summary>
    /// <returns>An account set transaction builder.</returns>
    public static AccountSetBuilder AccountSet()
    {
        return new AccountSetBuilder();
    }
    
    /// <summary>
    /// Creates a trust set transaction builder.
    /// </summary>
    /// <returns>A trust set transaction builder.</returns>
    public static TrustSetBuilder TrustSet()
    {
        return new TrustSetBuilder();
    }
    
    /// <summary>
    /// Creates an offer create transaction builder.
    /// </summary>
    /// <returns>An offer create transaction builder.</returns>
    public static OfferCreateBuilder OfferCreate()
    {
        return new OfferCreateBuilder();
    }
    
    /// <summary>
    /// Creates an offer cancel transaction builder.
    /// </summary>
    /// <returns>An offer cancel transaction builder.</returns>
    public static OfferCancelBuilder OfferCancel()
    {
        return new OfferCancelBuilder();
    }
    
    /// <summary>
    /// Creates an escrow create transaction builder.
    /// </summary>
    /// <returns>An escrow create transaction builder.</returns>
    public static EscrowCreateBuilder EscrowCreate()
    {
        return new EscrowCreateBuilder();
    }
    
    /// <summary>
    /// Creates an escrow finish transaction builder.
    /// </summary>
    /// <returns>An escrow finish transaction builder.</returns>
    public static EscrowFinishBuilder EscrowFinish()
    {
        return new EscrowFinishBuilder();
    }
    
    /// <summary>
    /// Creates an escrow cancel transaction builder.
    /// </summary>
    /// <returns>An escrow cancel transaction builder.</returns>
    public static EscrowCancelBuilder EscrowCancel()
    {
        return new EscrowCancelBuilder();
    }
    
    /// <summary>
    /// Creates a payment channel create transaction builder.
    /// </summary>
    /// <returns>A payment channel create transaction builder.</returns>
    public static PaymentChannelCreateBuilder PaymentChannelCreate()
    {
        return new PaymentChannelCreateBuilder();
    }
    
    /// <summary>
    /// Creates a payment channel fund transaction builder.
    /// </summary>
    /// <returns>A payment channel fund transaction builder.</returns>
    public static PaymentChannelFundBuilder PaymentChannelFund()
    {
        return new PaymentChannelFundBuilder();
    }
    
    /// <summary>
    /// Creates a payment channel claim transaction builder.
    /// </summary>
    /// <returns>A payment channel claim transaction builder.</returns>
    public static PaymentChannelClaimBuilder PaymentChannelClaim()
    {
        return new PaymentChannelClaimBuilder();
    }
    
    /// <summary>
    /// Creates an NFT mint transaction builder.
    /// </summary>
    /// <returns>An NFT mint transaction builder.</returns>
    public static NFTokenMintBuilder NFTokenMint()
    {
        return new NFTokenMintBuilder();
    }
    
    /// <summary>
    /// Creates an NFT burn transaction builder.
    /// </summary>
    /// <returns>An NFT burn transaction builder.</returns>
    public static NFTokenBurnBuilder NFTokenBurn()
    {
        return new NFTokenBurnBuilder();
    }
    
    /// <summary>
    /// Creates an NFT create offer transaction builder.
    /// </summary>
    /// <returns>An NFT create offer transaction builder.</returns>
    public static NFTokenCreateOfferBuilder NFTokenCreateOffer()
    {
        return new NFTokenCreateOfferBuilder();
    }
    
    /// <summary>
    /// Creates an NFT accept offer transaction builder.
    /// </summary>
    /// <returns>An NFT accept offer transaction builder.</returns>
    public static NFTokenAcceptOfferBuilder NFTokenAcceptOffer()
    {
        return new NFTokenAcceptOfferBuilder();
    }
    
    /// <summary>
    /// Creates an NFT cancel offer transaction builder.
    /// </summary>
    /// <returns>An NFT cancel offer transaction builder.</returns>
    public static NFTokenCancelOfferBuilder NFTokenCancelOffer()
    {
        return new NFTokenCancelOfferBuilder();
    }
}