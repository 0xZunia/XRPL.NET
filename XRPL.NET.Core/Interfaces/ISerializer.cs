using XRPL.NET.Core.Utils;

namespace XRPL.NET.Core.Interfaces;

/// <summary>
/// Provides methods for serializing and deserializing XRP Ledger data.
/// </summary>
public interface ISerializer
{
    /// <summary>
    /// Serializes a transaction to its binary format asynchronously.
    /// </summary>
    /// <param name="transaction">The transaction object to serialize.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<Result<byte[]>> SerializeTransactionAsync(object transaction);
    
    /// <summary>
    /// Serializes a transaction to its binary format as a hex string asynchronously.
    /// </summary>
    /// <param name="transaction">The transaction object to serialize.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<Result<string>> SerializeTransactionToHexAsync(object transaction);
    
    /// <summary>
    /// Deserializes a binary transaction to its object representation asynchronously.
    /// </summary>
    /// <param name="binary">The binary data to deserialize.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<Result<object>> DeserializeTransactionAsync(byte[] binary);
    
    /// <summary>
    /// Deserializes a hex string transaction to its object representation asynchronously.
    /// </summary>
    /// <param name="hex">The hex string to deserialize.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<Result<object>> DeserializeTransactionFromHexAsync(string hex);
    
    /// <summary>
    /// Computes the hash (ID) of a transaction asynchronously.
    /// </summary>
    /// <param name="transaction">The transaction object.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<Result<string>> ComputeTransactionHashAsync(object transaction);
    
    /// <summary>
    /// Computes the hash (ID) of a transaction from its binary representation asynchronously.
    /// </summary>
    /// <param name="binary">The binary transaction data.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<Result<string>> ComputeTransactionHashFromBinaryAsync(byte[] binary);
    
    /// <summary>
    /// Computes the hash (ID) of a transaction from its hex string representation asynchronously.
    /// </summary>
    /// <param name="hex">The hex string transaction data.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<Result<string>> ComputeTransactionHashFromHexAsync(string hex);
    
    /// <summary>
    /// Serializes a ledger object to its binary format asynchronously.
    /// </summary>
    /// <param name="ledgerObject">The ledger object to serialize.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<Result<byte[]>> SerializeLedgerObjectAsync(object ledgerObject);
    
    /// <summary>
    /// Deserializes a binary ledger object to its object representation asynchronously.
    /// </summary>
    /// <param name="binary">The binary data to deserialize.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<Result<object>> DeserializeLedgerObjectAsync(byte[] binary);
    
    /// <summary>
    /// Computes the index of a ledger object asynchronously.
    /// </summary>
    /// <param name="ledgerObject">The ledger object.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<Result<string>> ComputeLedgerObjectIndexAsync(object ledgerObject);
}