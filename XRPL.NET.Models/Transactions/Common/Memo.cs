using System.Text;
using System.Text.Json.Serialization;

namespace XRPL.NET.Models.Transactions.Common;

/// <summary>
/// Represents a memo that can be included in a transaction.
/// </summary>
/// <remarks>
/// Memos can store arbitrary data in a transaction. The XRP Ledger does not process or validate the contents.
/// The data is stored in the ledger and can be retrieved, but has no other effect on transaction processing.
/// </remarks>
public class Memo
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Memo"/> class.
    /// </summary>
    public Memo()
    {
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Memo"/> class with the specified text.
    /// </summary>
    /// <param name="text">The text content for the memo.</param>
    public Memo(string text)
    {
        if (!string.IsNullOrEmpty(text))
        {
            MemoData = Convert.ToHexString(Encoding.UTF8.GetBytes(text)).ToLower();
            MemoFormat = Convert.ToHexString(Encoding.UTF8.GetBytes("text/plain")).ToLower();
        }
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Memo"/> class with the specified fields.
    /// </summary>
    /// <param name="memoData">The arbitrary data for the memo in hexadecimal.</param>
    /// <param name="memoFormat">The format of the memo in hexadecimal.</param>
    /// <param name="memoType">The type of the memo in hexadecimal.</param>
    public Memo(string? memoData, string? memoFormat = null, string? memoType = null)
    {
        MemoData = memoData;
        MemoFormat = memoFormat;
        MemoType = memoType;
    }
    
    /// <summary>
    /// Gets or sets the memo data in hexadecimal format.
    /// </summary>
    /// <remarks>
    /// The arbitrary data is stored as a hexadecimal string in the ledger.
    /// The XRPL protocol does not validate or interpret this data.
    /// </remarks>
    [JsonPropertyName("MemoData")]
    public string? MemoData { get; set; }
    
    /// <summary>
    /// Gets or sets the memo format in hexadecimal format.
    /// </summary>
    /// <remarks>
    /// This typically contains the MIME type of the memo data, encoded as a hexadecimal string.
    /// For example, "text/plain" would be encoded as "746578742F706C61696E".
    /// </remarks>
    [JsonPropertyName("MemoFormat")]
    public string? MemoFormat { get; set; }
    
    /// <summary>
    /// Gets or sets the memo type in hexadecimal format.
    /// </summary>
    /// <remarks>
    /// This can be used to identify the purpose or meaning of the memo.
    /// </remarks>
    [JsonPropertyName("MemoType")]
    public string? MemoType { get; set; }
    
    /// <summary>
    /// Gets the memo data as a UTF-8 string.
    /// </summary>
    /// <returns>The memo data as a string, or null if the memo data is null or empty.</returns>
    public string? GetMemoDataAsString()
    {
        if (string.IsNullOrEmpty(MemoData))
        {
            return null;
        }
        
        try
        {
            return Encoding.UTF8.GetString(Convert.FromHexString(MemoData));
        }
        catch (Exception)
        {
            return null;
        }
    }
    
    /// <summary>
    /// Gets the memo format as a UTF-8 string.
    /// </summary>
    /// <returns>The memo format as a string, or null if the memo format is null or empty.</returns>
    public string? GetMemoFormatAsString()
    {
        if (string.IsNullOrEmpty(MemoFormat))
        {
            return null;
        }
        
        try
        {
            return Encoding.UTF8.GetString(Convert.FromHexString(MemoFormat));
        }
        catch (Exception)
        {
            return null;
        }
    }
    
    /// <summary>
    /// Gets the memo type as a UTF-8 string.
    /// </summary>
    /// <returns>The memo type as a string, or null if the memo type is null or empty.</returns>
    public string? GetMemoTypeAsString()
    {
        if (string.IsNullOrEmpty(MemoType))
        {
            return null;
        }
        
        try
        {
            return Encoding.UTF8.GetString(Convert.FromHexString(MemoType));
        }
        catch (Exception)
        {
            return null;
        }
    }
}