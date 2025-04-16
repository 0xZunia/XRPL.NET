using System.Text.Json.Serialization;
using XRPL.NET.Core.Constants;

namespace XRPL.NET.Models.Transactions.NFToken;

/// <summary>
/// Represents an NFToken mint transaction.
/// </summary>
public class NFTokenMintTransaction : TransactionBase
{
    /// <summary>
    /// Gets the transaction type.
    /// </summary>
    [JsonPropertyName("TransactionType")]
    public override string TransactionType => TransactionTypes.NFTokenMint;
    
    /// <summary>
    /// Gets or sets the URI of the NFToken.
    /// </summary>
    [JsonPropertyName("URI")]
    public string? Uri { get; set; }
    
    /// <summary>
    /// Gets or sets the transfer fee for the NFToken.
    /// </summary>
    [JsonPropertyName("TransferFee")]
    public uint? TransferFee { get; set; }
    
    /// <summary>
    /// Gets or sets the NFToken taxon.
    /// </summary>
    [JsonPropertyName("NFTokenTaxon")]
    public uint TokenTaxon { get; set; }
    
    /// <summary>
    /// Gets or sets whether the token is transferable.
    /// </summary>
    [JsonIgnore]
    public bool Transferable
    {
        get => (Flags ?? 0) != 0 && ((Flags & NFTokenMintFlags.Transferable) != 0);
        set
        {
            if (value)
            {
                Flags = (Flags ?? 0) | NFTokenMintFlags.Transferable;
            }
            else
            {
                Flags = (Flags ?? 0) & ~NFTokenMintFlags.Transferable;
            }
        }
    }
    
    /// <summary>
    /// Gets or sets whether only the issuer can burn the token.
    /// </summary>
    [JsonIgnore]
    public bool OnlyIssuerCanBurn
    {
        get => (Flags ?? 0) != 0 && ((Flags & NFTokenMintFlags.OnlyIssuerCanBurn) != 0);
        set
        {
            if (value)
            {
                Flags = (Flags ?? 0) | NFTokenMintFlags.OnlyIssuerCanBurn;
            }
            else
            {
                Flags = (Flags ?? 0) & ~NFTokenMintFlags.OnlyIssuerCanBurn;
            }
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