using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XRPL.NET.Core.Constants;
using XRPL.NET.Core.Utils;
using XRPL.NET.Core.Interfaces;

namespace XRPL.NET.Serialization;

/// <summary>
/// Handles the serialization of objects to the XRP Ledger binary format.
/// </summary>
public class BinarySerializer : ISerializer
{
    private readonly Dictionary<Type, ITypeSerializer> _typeSerializers = new();
    
    /// <summary>
    /// Initializes a new instance of the <see cref="BinarySerializer"/> class.
    /// </summary>
    public BinarySerializer()
    {
        RegisterDefaultSerializers();
    }
    
    /// <summary>
    /// Registers all default type serializers.
    /// </summary>
    private void RegisterDefaultSerializers()
    {
        // Register serializers for each primitive type
        _typeSerializers[typeof(uint)] = new UInt32Serializer();
        _typeSerializers[typeof(ulong)] = new UInt64Serializer();
        _typeSerializers[typeof(string)] = new StringSerializer();
        _typeSerializers[typeof(byte[])] = new BlobSerializer();
        // Additional serializers would be registered here
    }
    
    /// <summary>
    /// Serializes a transaction to its binary format asynchronously.
    /// </summary>
    /// <param name="transaction">The transaction object to serialize.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task<Result<byte[]>> SerializeTransactionAsync(object transaction)
    {
        try
        {
            // This would be implemented as a non-blocking operation
            // but for simplicity, we'll just call the synchronous method
            var binary = SerializeTransaction(transaction);
            return await Task.FromResult(Result.Success(binary));
        }
        catch (Exception ex)
        {
            return Result.Failure<byte[]>($"Failed to serialize transaction: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Serializes a transaction to its binary format.
    /// </summary>
    /// <param name="transaction">The transaction object to serialize.</param>
    /// <returns>The serialized binary data.</returns>
    private byte[] SerializeTransaction(object transaction)
    {
        // Create a buffer to hold the serialized data
        using var memoryStream = new MemoryStream();
        
        // Get all serializable properties
        var properties = GetSerializableProperties(transaction);
        
        // Sort properties by field ID (type code + field code)
        properties.Sort((a, b) => GetFieldId(a.Key).CompareTo(GetFieldId(b.Key)));
        
        // Serialize each property
        foreach (var property in properties)
        {
            if (property.Value != null)
            {
                SerializeField(memoryStream, property.Key, property.Value);
            }
        }
        
        // Add end marker
        memoryStream.WriteByte(0xE1); // Object end marker
        
        return memoryStream.ToArray();
    }
    
    /// <summary>
    /// Gets the field ID for a field name.
    /// </summary>
    /// <param name="fieldName">The name of the field.</param>
    /// <returns>The field ID.</returns>
    private int GetFieldId(string fieldName)
    {
        var (type, fieldCode) = FieldTypes.GetFieldEncoding(fieldName);
        return FieldTypes.CalculateFieldId(type, fieldCode);
    }
    
    /// <summary>
    /// Gets all serializable properties from an object.
    /// </summary>
    /// <param name="obj">The object to get properties from.</param>
    /// <returns>A list of property name/value pairs.</returns>
    private List<KeyValuePair<string, object>> GetSerializableProperties(object obj)
    {
        var result = new List<KeyValuePair<string, object>>();
        
        // Use reflection to get properties with JsonPropertyName attributes
        var properties = obj.GetType().GetProperties()
            .Where(p => p.GetCustomAttributes(typeof(System.Text.Json.Serialization.JsonPropertyNameAttribute), false).Any());
        
        foreach (var property in properties)
        {
            var attr = property.GetCustomAttributes(typeof(System.Text.Json.Serialization.JsonPropertyNameAttribute), false)
                .Cast<System.Text.Json.Serialization.JsonPropertyNameAttribute>()
                .FirstOrDefault();

            if (attr == null) continue;
            var value = property.GetValue(obj);
            if (value != null)
            {
                result.Add(new KeyValuePair<string, object>(attr.Name, value));
            }
        }
        
        return result;
    }
    
    /// <summary>
    /// Serializes a field to the memory stream.
    /// </summary>
    /// <param name="stream">The memory stream to write to.</param>
    /// <param name="fieldName">The name of the field.</param>
    /// <param name="value">The value to serialize.</param>
    private void SerializeField(MemoryStream stream, string fieldName, object value)
    {
        var (type, fieldCode) = FieldTypes.GetFieldEncoding(fieldName);
        
        // Write field header
        WriteFieldHeader(stream, type, fieldCode);
        
        // Serialize value based on its type
        SerializeValue(stream, value, type);
    }
    
    /// <summary>
    /// Writes a field header to the memory stream.
    /// </summary>
    /// <param name="stream">The memory stream to write to.</param>
    /// <param name="type">The field type.</param>
    /// <param name="fieldCode">The field code.</param>
    private void WriteFieldHeader(MemoryStream stream, FieldTypes.SerializedType type, int fieldCode)
    {
        byte typeCode = (byte)type;
        
        if (typeCode < 16)
        {
            if (fieldCode < 16)
            {
                // Both type and field codes are 4 bits each
                stream.WriteByte((byte)((typeCode << 4) | fieldCode));
            }
            else
            {
                // Type code is 4 bits, field code is 8 bits
                stream.WriteByte((byte)(typeCode << 4));
                stream.WriteByte((byte)fieldCode);
            }
        }
        else
        {
            if (fieldCode < 16)
            {
                // Type code is 8 bits, field code is 4 bits
                stream.WriteByte((byte)fieldCode);
                stream.WriteByte(typeCode);
            }
            else
            {
                // Both type and field codes are 8 bits each
                stream.WriteByte(0);
                stream.WriteByte(typeCode);
                stream.WriteByte((byte)fieldCode);
            }
        }
    }
    
    /// <summary>
    /// Serializes a value to the memory stream based on its type.
    /// </summary>
    /// <param name="stream">The memory stream to write to.</param>
    /// <param name="value">The value to serialize.</param>
    /// <param name="type">The serialized type.</param>
    private void SerializeValue(MemoryStream stream, object value, FieldTypes.SerializedType type)
    {
        // Find the appropriate serializer based on the value's type
        Type valueType = value.GetType();
        
        if (_typeSerializers.TryGetValue(valueType, out var serializer))
        {
            serializer.Serialize(stream, value);
        }
        else if (valueType.IsEnum)
        {
            // Handle enums
            _typeSerializers[typeof(uint)].Serialize(stream, Convert.ToUInt32(value));
        }
        else if (value is System.Collections.IEnumerable enumerable && !(value is string))
        {
            // Handle collections (arrays, lists, etc.)
            SerializeArray(stream, enumerable);
        }
        else
        {
            // Handle complex objects recursively
            var childProps = GetSerializableProperties(value);
            childProps.Sort((a, b) => GetFieldId(a.Key).CompareTo(GetFieldId(b.Key)));
            
            foreach (var prop in childProps)
            {
                if (prop.Value != null)
                {
                    SerializeField(stream, prop.Key, prop.Value);
                }
            }
            
            // Write end marker for the object
            stream.WriteByte(0xE1);
        }
    }
    
    /// <summary>
    /// Serializes an array to the memory stream.
    /// </summary>
    /// <param name="stream">The memory stream to write to.</param>
    /// <param name="array">The array to serialize.</param>
    private void SerializeArray(MemoryStream stream, System.Collections.IEnumerable array)
    {
        // Write array start marker
        stream.WriteByte(0xF0);
        
        foreach (var item in array)
        {
            switch (item)
            {
                case null:
                    continue;
                // Handle each item based on its type
                case ValueType or string:
                    // Primitive type
                    SerializeValue(stream, item, GetSerializedTypeForValue(item));
                    break;
                default:
                {
                    // Complex object
                    var props = GetSerializableProperties(item);
                    props.Sort((a, b) => GetFieldId(a.Key).CompareTo(GetFieldId(b.Key)));
                    
                    foreach (var prop in props)
                    {
                        if (prop.Value != null)
                        {
                            SerializeField(stream, prop.Key, prop.Value);
                        }
                    }
                    
                    // Write end marker for the object
                    stream.WriteByte(0xE1);
                    break;
                }
            }
        }
        
        // Write array end marker
        stream.WriteByte(0xF1);
    }
    
    /// <summary>
    /// Gets the serialized type for a value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The serialized type.</returns>
    private FieldTypes.SerializedType GetSerializedTypeForValue(object value)
    {
        return value switch
        {
            byte or ushort => FieldTypes.SerializedType.Uint16,
            uint => FieldTypes.SerializedType.Uint32,
            ulong => FieldTypes.SerializedType.Uint64,
            string { Length: 64 } => FieldTypes.SerializedType.Hash256,
            string { Length: 40 } => FieldTypes.SerializedType.Hash160,
            string => FieldTypes.SerializedType.Blob,
            byte[] => FieldTypes.SerializedType.Blob,
            _ => throw new NotSupportedException($"Unsupported value type: {value.GetType()}")
        };
    }
    
    /// <summary>
    /// Serializes a transaction to its binary format as a hex string asynchronously.
    /// </summary>
    /// <param name="transaction">The transaction object to serialize.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task<Result<string>> SerializeTransactionToHexAsync(object transaction)
    {
        var result = await SerializeTransactionAsync(transaction);
        return result.IsSuccess ? Result.Success(Convert.ToHexString(result.Value).ToLower()) : Result.Failure<string>(result.Errors);
    }
    
    /// <summary>
    /// Deserializes a binary transaction to its object representation asynchronously.
    /// </summary>
    /// <param name="binary">The binary data to deserialize.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task<Result<object>> DeserializeTransactionAsync(byte[] binary)
    {
        try
        {
            var transaction = DeserializeTransaction(binary);
            return await Task.FromResult(Result.Success<object>(transaction));
        }
        catch (Exception ex)
        {
            return Result.Failure<object>($"Failed to deserialize transaction: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Deserializes a hex string transaction to its object representation asynchronously.
    /// </summary>
    /// <param name="hex">The hex string to deserialize.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task<Result<object>> DeserializeTransactionFromHexAsync(string hex)
    {
        try
        {
            var binary = Convert.FromHexString(hex);
            return await DeserializeTransactionAsync(binary);
        }
        catch (Exception ex)
        {
            return Result.Failure<object>($"Failed to deserialize transaction from hex: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Computes the hash (ID) of a transaction asynchronously.
    /// </summary>
    /// <param name="transaction">The transaction object.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task<Result<string>> ComputeTransactionHashAsync(object transaction)
    {
        // Serialize the transaction first
        var result = await SerializeTransactionAsync(transaction);
        if (result.IsFailure)
        {
            return Result.Failure<string>(result.Errors);
        }
        
        return await ComputeTransactionHashFromBinaryAsync(result.Value);
    }
    
    /// <summary>
    /// Computes the hash (ID) of a transaction from its binary representation asynchronously.
    /// </summary>
    /// <param name="binary">The binary transaction data.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task<Result<string>> ComputeTransactionHashFromBinaryAsync(byte[] binary)
    {
        try
        {
            // Prepend transaction prefix (0x53545800 = "STX\0")
            byte[] prefixedData = new byte[4 + binary.Length];
            Buffer.BlockCopy(new byte[] { 0x53, 0x54, 0x58, 0x00 }, 0, prefixedData, 0, 4);
            Buffer.BlockCopy(binary, 0, prefixedData, 4, binary.Length);
            
            // Compute SHA-512 hash
            using var sha512 = System.Security.Cryptography.SHA512.Create();
            byte[] hash = sha512.ComputeHash(prefixedData);
            
            // Take first 32 bytes of the hash
            var txId = new byte[32];
            Buffer.BlockCopy(hash, 0, txId, 0, 32);
            
            return Task.FromResult(Result.Success(Convert.ToHexString(txId).ToLower()));
        }
        catch (Exception ex)
        {
            return Task.FromResult(Result.Failure<string>($"Failed to compute transaction hash: {ex.Message}"));
        }
    }
    
    /// <summary>
    /// Computes the hash (ID) of a transaction from its hex string representation asynchronously.
    /// </summary>
    /// <param name="hex">The hex string transaction data.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task<Result<string>> ComputeTransactionHashFromHexAsync(string hex)
    {
        try
        {
            var binary = Convert.FromHexString(hex);
            return ComputeTransactionHashFromBinaryAsync(binary);
        }
        catch (Exception ex)
        {
            return Task.FromResult(Result.Failure<string>($"Failed to compute transaction hash: {ex.Message}"));
        }
    }
    
    private byte[] SerializeLedgerObject(object ledgerObject)
    {
        using var memoryStream = new MemoryStream();

        var properties = GetSerializableProperties(ledgerObject);

        properties.Sort((a, b) => GetFieldId(a.Key).CompareTo(GetFieldId(b.Key)));

        foreach (var property in properties)
        {
            if (property.Value != null)
            {
                SerializeField(memoryStream, property.Key, property.Value);
            }
        }

        memoryStream.WriteByte(0xE1);
        
        return memoryStream.ToArray();
    }
    
    /// <summary>
    /// Serializes a ledger object to its binary format asynchronously.
    /// </summary>
    /// <param name="ledgerObject">The ledger object to serialize.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task<Result<byte[]>> SerializeLedgerObjectAsync(object ledgerObject)
    {
        try
        {
            var binary = SerializeLedgerObject(ledgerObject);
            return await Task.FromResult(Result.Success(binary));
        }
        catch (Exception ex)
        {
            return Result.Failure<byte[]>($"Failed to serialize ledger object: {ex.Message}");
        }
    }
    
    private object DeserializeLedgerObject(byte[] binary)
    {
        // TODO: Implement deserialization logic
        return new Dictionary<string, object>
        {
            { "LedgerEntryType", "Generic" },
            { "Parsed", true },
            { "BinaryLength", binary.Length }
        };
    }
    
    /// <summary>
    /// Deserializes a binary ledger object to its object representation asynchronously.
    /// </summary>
    /// <param name="binary">The binary data to deserialize.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task<Result<object>> DeserializeLedgerObjectAsync(byte[] binary)
    {
        try
        {
            // Similar approach as DeserializeTransaction, but for ledger objects
            var ledgerObject = DeserializeLedgerObject(binary);
            return await Task.FromResult(Result.Success<object>(ledgerObject));
        }
        catch (Exception ex)
        {
            return Result.Failure<object>($"Failed to deserialize ledger object: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Computes the index of a ledger object asynchronously.
    /// </summary>
    /// <param name="ledgerObject">The ledger object.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task<Result<string>> ComputeLedgerObjectIndexAsync(object ledgerObject)
    {
        // TODO: Implement index computation logic
        throw new NotImplementedException();
    }
}

/// <summary>
/// Interface for type-specific serializers.
/// </summary>
internal interface ITypeSerializer
{
    /// <summary>
    /// Serializes a value to a memory stream.
    /// </summary>
    /// <param name="stream">The memory stream to write to.</param>
    /// <param name="value">The value to serialize.</param>
    void Serialize(MemoryStream stream, object value);
}

/// <summary>
/// Serializer for 32-bit unsigned integers.
/// </summary>
internal class UInt32Serializer : ITypeSerializer
{
    public void Serialize(MemoryStream stream, object value)
    {
        var uintValue = Convert.ToUInt32(value);
        var bytes = BitConverter.GetBytes(uintValue);
        
        // XRPL uses big-endian
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }
        
        stream.Write(bytes, 0, bytes.Length);
    }
}

/// <summary>
/// Serializer for 64-bit unsigned integers.
/// </summary>
internal class UInt64Serializer : ITypeSerializer
{
    public void Serialize(MemoryStream stream, object value)
    {
        ulong ulongValue = Convert.ToUInt64(value);
        byte[] bytes = BitConverter.GetBytes(ulongValue);
        
        // XRPL uses big-endian
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }
        
        stream.Write(bytes, 0, bytes.Length);
    }
}

/// <summary>
/// Serializer for strings.
/// </summary>
internal class StringSerializer : ITypeSerializer
{
    public void Serialize(MemoryStream stream, object value)
    {
        var stringValue = (string)value;
        var bytes = Encoding.UTF8.GetBytes(stringValue);
        
        // Write length prefix for variable-length fields
        WriteVariableLength(stream, bytes.Length);
        
        // Write the string bytes
        stream.Write(bytes, 0, bytes.Length);
    }
    
    private void WriteVariableLength(MemoryStream stream, int length)
    {
        switch (length)
        {
            case <= 192:
                stream.WriteByte((byte)length);
                break;
            case <= 12480:
            {
                int lenMinusFirst = length - 193;
                byte byte1 = (byte)(193 + (lenMinusFirst >> 8));
                byte byte2 = (byte)(lenMinusFirst & 0xFF);
                stream.WriteByte(byte1);
                stream.WriteByte(byte2);
                break;
            }
            case <= 918744:
            {
                int lenMinusFirst = length - 12481;
                byte byte1 = (byte)(241 + (lenMinusFirst >> 16));
                byte byte2 = (byte)((lenMinusFirst >> 8) & 0xFF);
                byte byte3 = (byte)(lenMinusFirst & 0xFF);
                stream.WriteByte(byte1);
                stream.WriteByte(byte2);
                stream.WriteByte(byte3);
                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(length), "Length too large for XRPL variable length encoding");
        }
    }
}

/// <summary>
/// Serializer for binary blobs.
/// </summary>
internal class BlobSerializer : ITypeSerializer
{
    public void Serialize(MemoryStream stream, object value)
    {
        var blobValue = value switch
        {
            byte[] byteArray => byteArray,
            string hexString => Convert.FromHexString(hexString),
            _ => throw new ArgumentException($"Cannot serialize {value.GetType()} as a blob")
        };

        // Write length prefix for variable-length fields
        WriteVariableLength(stream, blobValue.Length);
        
        // Write the blob bytes
        stream.Write(blobValue, 0, blobValue.Length);
    }
    
    private void WriteVariableLength(MemoryStream stream, int length)
    {
        switch (length)
        {
            case <= 192:
                stream.WriteByte((byte)length);
                break;
            case <= 12480:
            {
                int lenMinusFirst = length - 193;
                byte byte1 = (byte)(193 + (lenMinusFirst >> 8));
                byte byte2 = (byte)(lenMinusFirst & 0xFF);
                stream.WriteByte(byte1);
                stream.WriteByte(byte2);
                break;
            }
            case <= 918744:
            {
                int lenMinusFirst = length - 12481;
                byte byte1 = (byte)(241 + (lenMinusFirst >> 16));
                byte byte2 = (byte)((lenMinusFirst >> 8) & 0xFF);
                byte byte3 = (byte)(lenMinusFirst & 0xFF);
                stream.WriteByte(byte1);
                stream.WriteByte(byte2);
                stream.WriteByte(byte3);
                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(length), "Length too large for XRPL variable length encoding");
        }
    }
}