using System.Text.Json.Serialization;
using XRPL.NET.Core.Constants;

namespace XRPL.NET.Models.Transactions.NFToken;

/// <summary>
/// Represents an NFToken create offer transaction.
/// </summary>
public class NFTokenCreateOfferTransaction : TransactionBase
{
    /// <summary>
    /// Gets the transaction type.
    /// </summary>
    [JsonPropertyName("TransactionType")]
    public override string TransactionType => TransactionTypes.NFTokenCreateOffer;
    
    /// <summary>
    /// Gets or sets the unique identifier of the NFToken.
    /// </summary>
    [JsonPropertyName("NFTokenID")]
    public string TokenId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the amount of the offer.
    /// </summary>
    [JsonPropertyName("Amount")]
    public string Amount { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the destination account for the offer.
    /// </summary>
    [JsonPropertyName("Destination")]
    public string? Destination { get; set; }
    
    /// <summary>
    /// Gets or sets whether this is a sell offer.
    /// </summary>
    [JsonIgnore]
    public bool SellOffer
    {
        get => (Flags ?? 0) != 0 && ((Flags & NFTokenCreateOfferFlags.SellOffer) != 0);
        set
        {
            if (value)
            {
                Flags = (Flags ?? 0) | NFTokenCreateOfferFlags.SellOffer;
            }
            else
            {
                Flags = (Flags ?? 0) & ~NFTokenCreateOfferFlags.SellOffer;
            }
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