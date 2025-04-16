namespace XRPL.NET.Core.Constants;

/// <summary>
/// Defines the field types and their encoding information for the XRP Ledger binary format.
/// Each field in the XRP Ledger has a type and a field code which together form a unique identifier.
/// </summary>
public static class FieldTypes
{
    /// <summary>
    /// Represents the different data types that can be encoded in the XRP Ledger binary format.
    /// </summary>
    public enum SerializedType
    {
        Uint16 = 1,
        Uint32 = 2,
        Uint64 = 3,
        Uint128 = 4,
        Uint256 = 5,
        Hash128 = 6,
        Hash160 = 7,
        Hash256 = 8,
        Amount = 9,
        Blob = 10,
        AccountId = 11,
        StObject = 12,
        StArray = 13,
        Uint8 = 14,
        PathSet = 15,
        Vector256 = 16,
        Issue = 17,
        Currency = 18,
        Transaction = 19
    }

    /// <summary>
    /// Maps field names to their type and field code for serialization.
    /// </summary>
    public static readonly Dictionary<string, (SerializedType Type, int FieldCode)> Fields = new()
    {
        // Common fields
        { "TransactionType", (SerializedType.Uint16, 2) },
        { "Flags", (SerializedType.Uint32, 2) },
        { "Sequence", (SerializedType.Uint32, 4) },
        { "PreviousTxnID", (SerializedType.Hash256, 4) },
        { "LedgerSequence", (SerializedType.Uint32, 6) },
        { "CloseTime", (SerializedType.Uint32, 7) },
        { "AccountTxnID", (SerializedType.Hash256, 8) },
        { "Account", (SerializedType.AccountId, 1) },
        { "Owner", (SerializedType.AccountId, 2) },
        { "Destination", (SerializedType.AccountId, 3) },
        { "Issuer", (SerializedType.AccountId, 4) },
        { "Target", (SerializedType.AccountId, 7) },
        { "RegularKey", (SerializedType.AccountId, 8) },
        
        // Payment fields
        { "Amount", (SerializedType.Amount, 1) },
        { "Balance", (SerializedType.Amount, 2) },
        { "SendMax", (SerializedType.Amount, 9) },
        { "DeliverMin", (SerializedType.Amount, 11) },
        
        // Path-related fields
        { "Paths", (SerializedType.PathSet, 1) },
        
        // Limit-related fields
        { "LimitAmount", (SerializedType.Amount, 3) },
        { "TakerPays", (SerializedType.Amount, 4) },
        { "TakerGets", (SerializedType.Amount, 5) },
        
        // Offer-related fields
        { "OfferSequence", (SerializedType.Uint32, 25) },
        { "BookDirectory", (SerializedType.Uint256, 16) },
        { "BookNode", (SerializedType.Uint64, 17) },
        { "OwnerNode", (SerializedType.Uint64, 18) },
        
        // Expiration and timing fields
        { "Expiration", (SerializedType.Uint32, 10) },
        { "CancelAfter", (SerializedType.Uint32, 20) },
        { "FinishAfter", (SerializedType.Uint32, 21) },
        { "SourceTag", (SerializedType.Uint32, 3) },
        { "DestinationTag", (SerializedType.Uint32, 14) },
        
        // Fee-related fields
        { "Fee", (SerializedType.Amount, 8) },
        { "TakerGetsCurrency", (SerializedType.Hash160, 6) },
        { "TakerGetsIssuer", (SerializedType.Hash160, 7) },
        { "TakerPaysCurrency", (SerializedType.Hash160, 8) },
        { "TakerPaysIssuer", (SerializedType.Hash160, 9) },
        
        // Signature fields
        { "SigningPubKey", (SerializedType.Blob, 3) },
        { "TxnSignature", (SerializedType.Blob, 4) },
        { "Signature", (SerializedType.Blob, 6) },
        { "SignerEntries", (SerializedType.StArray, 3) },
        { "SignerQuorum", (SerializedType.Uint32, 15) },
        { "SignerWeight", (SerializedType.Uint16, 3) },
        
        // NFT fields
        { "NFTokenID", (SerializedType.Hash256, 10) },
        { "NFTokenTaxon", (SerializedType.Uint32, 18) },
        { "NFTokenURI", (SerializedType.Blob, 19) },
        
        // AMM fields
        { "AMMAccount", (SerializedType.AccountId, 22) },
        { "Asset", (SerializedType.StObject, 2) },
        { "Asset2", (SerializedType.StObject, 3) },
        { "TradingFee", (SerializedType.Uint16, 10) },
        
        // XChain fields
        { "XChainBridge", (SerializedType.StObject, 10) },
        { "XChainClaimID", (SerializedType.Uint64, 20) },
        { "XChainAccountCreateCount", (SerializedType.Uint32, 30) },
        { "XChainAccountClaimCount", (SerializedType.Uint32, 31) },
        
        // Hook fields
        { "Hook", (SerializedType.StObject, 12) },
        { "Hooks", (SerializedType.StArray, 13) },
        { "HookOn", (SerializedType.Blob, 16) },
        { "HookNamespace", (SerializedType.Hash256, 17) },
        { "HookParameters", (SerializedType.StArray, 18) },
        
        // Memo fields
        { "Memos", (SerializedType.StArray, 9) },
        { "MemoType", (SerializedType.Blob, 12) },
        { "MemoData", (SerializedType.Blob, 13) },
        { "MemoFormat", (SerializedType.Blob, 14) }
    };
    
    /// <summary>
    /// Gets the field encoding information for a given field name.
    /// </summary>
    /// <param name="fieldName">The name of the field.</param>
    /// <returns>A tuple containing the serialized type and field code.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the field name is not recognized.</exception>
    public static (SerializedType Type, int FieldCode) GetFieldEncoding(string fieldName)
    {
        if (!Fields.TryGetValue(fieldName, out var fieldInfo))
        {
            throw new KeyNotFoundException($"Field name not found: {fieldName}");
        }
        
        return fieldInfo;
    }
    
    /// <summary>
    /// Calculates the field ID from the type and field code.
    /// </summary>
    /// <param name="type">The serialized type.</param>
    /// <param name="fieldCode">The field code.</param>
    /// <returns>The combined field ID used in binary serialization.</returns>
    public static int CalculateFieldId(SerializedType type, int fieldCode)
    {
        if (fieldCode <= 0 || fieldCode > 255)
        {
            throw new ArgumentOutOfRangeException(nameof(fieldCode), "Field code must be between 1 and 255.");
        }
        
        return ((int)type << 16) | fieldCode;
    }
}