using System.Text.Json.Serialization;

namespace XRPL.NET.Models.Transactions.Common;

/// <summary>
/// Represents a signature that contributes to a multi-signature.
/// </summary>
/// <remarks>
/// Multi-signatures allow for an account to be controlled by multiple parties,
/// where each party contributes a signature to authorize transactions.
/// </remarks>
public class Signer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Signer"/> class.
    /// </summary>
    public Signer()
    {
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Signer"/> class with the specified account and signature.
    /// </summary>
    /// <param name="account">The account contributing the signature.</param>
    /// <param name="txnSignature">The transaction signature.</param>
    public Signer(string account, string txnSignature)
    {
        Account = account;
        TxnSignature = txnSignature;
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Signer"/> class with the specified account, signature, and public key.
    /// </summary>
    /// <param name="account">The account contributing the signature.</param>
    /// <param name="txnSignature">The transaction signature.</param>
    /// <param name="signingPubKey">The public key used to create the signature.</param>
    public Signer(string account, string txnSignature, string signingPubKey)
    {
        Account = account;
        TxnSignature = txnSignature;
        SigningPubKey = signingPubKey;
    }
    
    /// <summary>
    /// Gets or sets the account address of the signer.
    /// </summary>
    [JsonPropertyName("Account")]
    public string Account { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the signature that this signer contributed.
    /// </summary>
    [JsonPropertyName("TxnSignature")]
    public string TxnSignature { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the public key used to create this signature.
    /// </summary>
    [JsonPropertyName("SigningPubKey")]
    public string? SigningPubKey { get; set; }
}