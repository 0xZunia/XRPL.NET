using System.Text.Json.Serialization;
using XRPL.NET.Core.Constants;

namespace XRPL.NET.Models.Transactions.Escrow;

/// <summary>
/// Represents an EscrowCreate transaction, which creates an escrow arrangement.
/// </summary>
public class EscrowCreateTransaction : TransactionBase
{
    /// <summary>
    /// Gets the transaction type.
    /// </summary>
    [JsonPropertyName("TransactionType")]
    public override string TransactionType => TransactionTypes.EscrowCreate;
    
    /// <summary>
    /// Gets or sets the destination account for the escrowed payment.
    /// </summary>
    [JsonPropertyName("Destination")]
    public string Destination { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the amount to escrow.
    /// </summary>
    [JsonPropertyName("Amount")]
    public string Amount { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the time when the escrow can be finished.
    /// </summary>
    [JsonPropertyName("FinishAfter")]
    public uint? FinishAfter { get; set; }
    
    /// <summary>
    /// Gets or sets the time after which the escrow can be canceled.
    /// </summary>
    [JsonPropertyName("CancelAfter")]
    public uint? CancelAfter { get; set; }
    
    /// <summary>
    /// Gets or sets the cryptographic condition for finishing the escrow.
    /// </summary>
    [JsonPropertyName("Condition")]
    public string? Condition { get; set; }
}