using System.Text.Json.Serialization;
using XRPL.NET.Core.Constants;

namespace XRPL.NET.Models.Transactions.NFToken;

/// <summary>
/// Represents an NFToken accept offer transaction.
/// </summary>
public class NFTokenAcceptOfferTransaction : TransactionBase
{
    /// <summary>
    /// Gets the transaction type.
    /// </summary>
    [JsonPropertyName("TransactionType")]
    public override string TransactionType => TransactionTypes.NFTokenAcceptOffer;
    
    /// <summary>
    /// Gets or sets the sell offer to accept.
    /// </summary>
    [JsonPropertyName("NFTokenSellOffer")]
    public string? SellOffer { get; set; }
    
    /// <summary>
    /// Gets or sets the buy offer to accept.
    /// </summary>
    [JsonPropertyName("NFTokenBuyOffer")]
    public string? BuyOffer { get; set; }
}