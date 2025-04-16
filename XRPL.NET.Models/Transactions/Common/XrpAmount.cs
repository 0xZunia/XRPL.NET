using System.Globalization;
using System.Numerics;

namespace XRPL.NET.Models.Transactions.Common;

/// <summary>
/// Provides utilities for working with XRP amounts.
/// </summary>
/// <remarks>
/// XRP is represented as an integer number of "drops", where 1 XRP = 1,000,000 drops.
/// </remarks>
public static class XrpAmount
{
    /// <summary>
    /// The number of drops per XRP.
    /// </summary>
    public const long DropsPerXrp = 1_000_000;
    
    /// <summary>
    /// Converts XRP to drops.
    /// </summary>
    /// <param name="xrp">The amount of XRP.</param>
    /// <returns>The equivalent amount in drops, as a string.</returns>
    public static string ToDrops(decimal xrp)
    {
        BigInteger drops = new BigInteger(Math.Round(xrp * DropsPerXrp, 0));
        return drops.ToString(CultureInfo.InvariantCulture);
    }
    
    /// <summary>
    /// Converts XRP to drops.
    /// </summary>
    /// <param name="xrp">The amount of XRP as a string.</param>
    /// <returns>The equivalent amount in drops, as a string.</returns>
    /// <exception cref="FormatException">Thrown when the input is not a valid decimal.</exception>
    public static string ToDrops(string xrp)
    {
        if (!decimal.TryParse(xrp, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal amount))
        {
            throw new FormatException($"Invalid XRP amount: {xrp}");
        }
        
        return ToDrops(amount);
    }
    
    /// <summary>
    /// Converts drops to XRP.
    /// </summary>
    /// <param name="drops">The amount in drops.</param>
    /// <returns>The equivalent amount in XRP.</returns>
    public static decimal ToXrp(BigInteger drops)
    {
        return (decimal)drops / DropsPerXrp;
    }
    
    /// <summary>
    /// Converts drops to XRP.
    /// </summary>
    /// <param name="drops">The amount in drops, as a string.</param>
    /// <returns>The equivalent amount in XRP.</returns>
    /// <exception cref="FormatException">Thrown when the input is not a valid integer.</exception>
    public static decimal ToXrp(string drops)
    {
        if (!BigInteger.TryParse(drops, NumberStyles.Any, CultureInfo.InvariantCulture, out BigInteger amount))
        {
            throw new FormatException($"Invalid drops amount: {drops}");
        }
        
        return ToXrp(amount);
    }
    
    /// <summary>
    /// Formats an XRP amount for display, with 6 decimal places.
    /// </summary>
    /// <param name="xrp">The amount of XRP.</param>
    /// <returns>The formatted XRP amount.</returns>
    public static string Format(decimal xrp)
    {
        return $"{xrp.ToString("0.######", CultureInfo.InvariantCulture)} XRP";
    }
    
    /// <summary>
    /// Formats a drops amount for display, converting to XRP with 6 decimal places.
    /// </summary>
    /// <param name="drops">The amount in drops, as a string.</param>
    /// <returns>The formatted XRP amount.</returns>
    /// <exception cref="FormatException">Thrown when the input is not a valid integer.</exception>
    public static string FormatDrops(string drops)
    {
        return Format(ToXrp(drops));
    }
    
    /// <summary>
    /// Determines whether a string represents a valid XRP amount.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <returns>True if the string represents a valid XRP amount; otherwise, false.</returns>
    public static bool IsValidXrpAmount(string value)
    {
        return decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out _);
    }
    
    /// <summary>
    /// Determines whether a string represents a valid drops amount.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <returns>True if the string represents a valid drops amount; otherwise, false.</returns>
    public static bool IsValidDropsAmount(string value)
    {
        return BigInteger.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out _);
    }
}