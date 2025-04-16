using System.Text.Json.Serialization;
using XRPL.NET.Core.Constants;

namespace XRPL.NET.Models.Transactions.NFToken;

/// <summary>
/// Represents an NFToken cancel offer transaction.
/// </summary>
public class NFTokenCancelOfferTransaction : TransactionBase
{
    /// <summary>
    /// Gets the transaction type.
    /// </summary>
    [JsonPropertyName("TransactionType")]
    public override string TransactionType => TransactionTypes.NFTokenCancelOffer;
    
    /// <summary>
    /// Gets the list of offer IDs to cancel.
    /// </summary>
    [JsonPropertyName("NFTokenOffers")]
    public List<string> Offers { get; } = new List<string>();
    
    /// <summary>
    /// Adds an offer to the list of offers to cancel.
    /// </summary>
    /// <param name="offerToCancel">The ID of the offer to cancel.</param>
    public void AddOffer(string offerToCancel)
    {
        if (!string.IsNullOrEmpty(offerToCancel))
        {
            Offers.Add(offerToCancel);
        }
    }
}