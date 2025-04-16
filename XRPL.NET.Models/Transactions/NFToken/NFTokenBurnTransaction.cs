using System.Text.Json.Serialization;
using XRPL.NET.Core.Constants;

namespace XRPL.NET.Models.Transactions.NFToken;

/// <summary>
/// Represents an NFToken burn transaction.
/// </summary>
public class NFTokenBurnTransaction : TransactionBase
{
    /// <summary>
    /// Gets the transaction type.
    /// </summary>
    [JsonPropertyName("TransactionType")]
    public override string TransactionType => TransactionTypes.NFTokenBurn;
    
    /// <summary>
    /// Gets or sets the unique identifier of the NFToken to burn.
    /// </summary>
    [JsonPropertyName("NFTokenID")]
    public string TokenId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the owner of the NFToken being burned.
    /// </summary>
    [JsonPropertyName("Owner")]
    public string? Owner { get; set; }
}