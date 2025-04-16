using System.Globalization;
using System.Text.Json.Serialization;

namespace XRPL.NET.Models.Transactions.Common;

/// <summary>
/// Represents an amount of an issued (non-XRP) currency.
/// </summary>
public class IssuedCurrencyAmount
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IssuedCurrencyAmount"/> class.
    /// </summary>
    public IssuedCurrencyAmount()
    {
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="IssuedCurrencyAmount"/> class with specified currency and value.
    /// </summary>
    /// <param name="currency">The currency code.</param>
    /// <param name="issuer">The issuer account.</param>
    /// <param name="value">The amount value.</param>
    public IssuedCurrencyAmount(string currency, string issuer, decimal value)
    {
        Currency = currency;
        Issuer = issuer;
        Value = value.ToString(CultureInfo.InvariantCulture);
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="IssuedCurrencyAmount"/> class with specified currency and value.
    /// </summary>
    /// <param name="currency">The currency code.</param>
    /// <param name="issuer">The issuer account.</param>
    /// <param name="value">The amount value as a string.</param>
    public IssuedCurrencyAmount(string currency, string issuer, string value)
    {
        Currency = currency;
        Issuer = issuer;
        Value = value;
    }
    
    /// <summary>
    /// Gets or sets the currency code.
    /// </summary>
    /// <remarks>
    /// A 3-letter ISO code like "USD" or a 160-bit hex value like "015841551A748AD2C1F76FF6ECB0CCCD00000000"
    /// representing the currency.
    /// </remarks>
    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the issuer account.
    /// </summary>
    /// <remarks>
    /// The account that issues the currency.
    /// </remarks>
    [JsonPropertyName("issuer")]
    public string Issuer { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the amount value.
    /// </summary>
    /// <remarks>
    /// The amount value as a decimal string.
    /// </remarks>
    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets the amount as a decimal.
    /// </summary>
    /// <returns>The amount as a decimal value.</returns>
    /// <exception cref="FormatException">Thrown when the amount cannot be parsed as a decimal.</exception>
    [JsonIgnore]
    public decimal DecimalValue => decimal.Parse(Value, CultureInfo.InvariantCulture);
    
    /// <summary>
    /// Creates an issued currency amount.
    /// </summary>
    /// <param name="currency">The currency code.</param>
    /// <param name="issuer">The issuer account.</param>
    /// <param name="value">The amount value.</param>
    /// <returns>A new issued currency amount.</returns>
    public static IssuedCurrencyAmount Create(string currency, string issuer, decimal value)
    {
        return new IssuedCurrencyAmount(currency, issuer, value);
    }
    
    /// <summary>
    /// Creates an issued currency amount from a string value.
    /// </summary>
    /// <param name="currency">The currency code.</param>
    /// <param name="issuer">The issuer account.</param>
    /// <param name="value">The amount value as a string.</param>
    /// <returns>A new issued currency amount.</returns>
    public static IssuedCurrencyAmount Create(string currency, string issuer, string value)
    {
        return new IssuedCurrencyAmount(currency, issuer, value);
    }
    
    /// <summary>
    /// Converts the amount to a string representation.
    /// </summary>
    /// <returns>A string representation of the amount.</returns>
    public override string ToString()
    {
        return $"{Value} {Currency}.{Issuer}";
    }
}