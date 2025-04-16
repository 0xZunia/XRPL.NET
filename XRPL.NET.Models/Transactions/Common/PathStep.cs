using System.Text.Json.Serialization;

namespace XRPL.NET.Models.Transactions.Common;

/// <summary>
/// Represents a single step in a payment path through the XRP Ledger's decentralized exchange.
/// </summary>
/// <remarks>
/// A path step can include a currency, an issuer, and an account. Any combination of these may be omitted.
/// </remarks>
public class PathStep
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PathStep"/> class.
    /// </summary>
    public PathStep()
    {
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="PathStep"/> class with a currency.
    /// </summary>
    /// <param name="currency">The currency code.</param>
    public PathStep(string currency)
    {
        Currency = currency;
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="PathStep"/> class with a currency and issuer.
    /// </summary>
    /// <param name="currency">The currency code.</param>
    /// <param name="issuer">The issuer account.</param>
    public PathStep(string currency, string issuer)
    {
        Currency = currency;
        Issuer = issuer;
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="PathStep"/> class with a currency, issuer, and account.
    /// </summary>
    /// <param name="currency">The currency code.</param>
    /// <param name="issuer">The issuer account.</param>
    /// <param name="account">The account.</param>
    public PathStep(string currency, string issuer, string account)
    {
        Currency = currency;
        Issuer = issuer;
        Account = account;
    }
    
    /// <summary>
    /// Gets or sets the currency of this path step.
    /// </summary>
    /// <remarks>
    /// A 3-letter ISO code like "USD" or a 160-bit hex value like "015841551A748AD2C1F76FF6ECB0CCCD00000000"
    /// representing the currency. Can be omitted to mean "any currency".
    /// </remarks>
    [JsonPropertyName("currency")]
    public string? Currency { get; set; }
    
    /// <summary>
    /// Gets or sets the issuer account of this path step.
    /// </summary>
    /// <remarks>
    /// The account that issues the currency. Can be omitted to mean "any issuer".
    /// For XRP, this should be omitted.
    /// </remarks>
    [JsonPropertyName("issuer")]
    public string? Issuer { get; set; }
    
    /// <summary>
    /// Gets or sets the account of this path step.
    /// </summary>
    /// <remarks>
    /// The account at this step in the path. Can be omitted if this step involves going through an offer book.
    /// </remarks>
    [JsonPropertyName("account")]
    public string? Account { get; set; }
    
    /// <summary>
    /// Gets a value indicating whether this step involves XRP.
    /// </summary>
    [JsonIgnore]
    public bool IsXrp => Currency == "XRP" && Issuer == null;
    
    /// <summary>
    /// Creates a path step for XRP.
    /// </summary>
    /// <returns>A new path step for XRP.</returns>
    public static PathStep Xrp()
    {
        return new PathStep("XRP");
    }
    
    /// <summary>
    /// Creates a path step for an issued currency.
    /// </summary>
    /// <param name="currency">The currency code.</param>
    /// <param name="issuer">The issuer account.</param>
    /// <returns>A new path step for the issued currency.</returns>
    public static PathStep IssuedCurrency(string currency, string issuer)
    {
        return new PathStep(currency, issuer);
    }
}