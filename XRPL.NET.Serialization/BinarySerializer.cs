using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XRPL.NET.Core.Constants;
using XRPL.NET.Core.Exceptions;
using XRPL.NET.Core.Utils;
using XRPL.NET.Core.Interfaces;
using XRPL.NET.Models.Transactions;
using XRPL.NET.Models.Transactions.AccountSet;
using XRPL.NET.Models.Transactions.Escrow;
using XRPL.NET.Models.Transactions.NFToken;
using XRPL.NET.Models.Transactions.Offer;
using XRPL.NET.Models.Transactions.Payment;
using XRPL.NET.Models.Transactions.Trust;

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
    
    
    private object DeserializeTransaction(byte[] binary)
    {
        using var memoryStream = new MemoryStream(binary);
        BlobSerializer blobSerializer = new BlobSerializer();
        
        try
        {
            Dictionary<string, object> fields = new Dictionary<string, object>();
        
            // Read fields until we reach the end of the stream or an end marker
            while (memoryStream.Position < memoryStream.Length)
            {
                byte firstByte = (byte)memoryStream.ReadByte();
            
                // Check for end marker
                if (firstByte == 0xE1)
                {
                    break;
                }
            
                // Rewind to reread the first byte
                memoryStream.Position -= 1;
            
                // Parse the field header
                (FieldTypes.SerializedType fieldType, int fieldCode) = blobSerializer.ReadFieldHeader(memoryStream);
            
                // Get field name
                string fieldName = blobSerializer.GetFieldNameFromTypeAndCode(fieldType, fieldCode);
            
                // Read the field value
                object fieldValue = blobSerializer.ReadFieldValue(memoryStream, fieldType);
            
                fields[fieldName] = fieldValue;
            }
        
            // Create the appropriate transaction object
            if (fields.TryGetValue("TransactionType", out object txTypeObj))
            {
                uint txTypeCode = Convert.ToUInt32(txTypeObj);
                string txTypeName = TransactionTypes.GetTransactionTypeName((int)txTypeCode);
            
                var transaction = blobSerializer.CreateTransactionObject(txTypeName);
            
                // Set properties
                foreach (var kvp in fields)
                {
                    blobSerializer.SetTransactionProperty(transaction, kvp.Key, kvp.Value);
                }
            
                return transaction;
            }
        
            // If we can't determine the transaction type, return the fields dictionary
            return fields;
        }
        catch (Exception ex)
        {
            throw new XrplException($"Failed to deserialize transaction: {ex.Message}", ex);
        }
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
    /// Computes the index of a ledger object asynchronously.
    /// </summary>
    /// <param name="ledgerObject">The ledger object.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task<Result<string>> ComputeLedgerObjectIndexAsync(object ledgerObject)
    {
        // TODO: Implement index computation logic
        throw new NotImplementedException();
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

        internal string GetFieldNameFromTypeAndCode(FieldTypes.SerializedType type, int code)
        {
            foreach (var field in FieldTypes.Fields)
            {
                if (field.Value.Type == type && field.Value.FieldCode == code)
                {
                    return field.Key;
                }
            }
        
            return $"Unknown_Field_{type}_{code}";
        }

        internal object ReadFieldValue(MemoryStream stream, FieldTypes.SerializedType type)
        {
            switch (type)
            {
                case FieldTypes.SerializedType.Uint8:
                    return (byte)stream.ReadByte();
                
                case FieldTypes.SerializedType.Uint16:
                    return ReadUInt16(stream);
                
                case FieldTypes.SerializedType.Uint32:
                    return ReadUInt32(stream);
                
                case FieldTypes.SerializedType.Uint64:
                    return ReadUInt64(stream);
                
                case FieldTypes.SerializedType.Uint128:
                case FieldTypes.SerializedType.Uint256:
                    return ReadHex(stream, type);
                
                case FieldTypes.SerializedType.Hash128:
                    return ReadHex(stream, 16);
                
                case FieldTypes.SerializedType.Hash160:
                    return ReadHex(stream, 20);
                
                case FieldTypes.SerializedType.Hash256:
                    return ReadHex(stream, 32);
                
                case FieldTypes.SerializedType.AccountId:
                    return ReadAccountID(stream);
                
                case FieldTypes.SerializedType.Amount:
                    return ReadAmount(stream);
                
                case FieldTypes.SerializedType.Blob:
                    return ReadVarLengthBytes(stream);
                
                case FieldTypes.SerializedType.StObject:
                    return ReadObject(stream);
                
                case FieldTypes.SerializedType.StArray:
                    return ReadArray(stream);
                
                case FieldTypes.SerializedType.Currency:
                    return ReadCurrency(stream);
                
                default:
                    throw new XrplException($"Unsupported field type: {type}");
            }
        }
        
        private ushort ReadUInt16(MemoryStream stream)
        {
            byte[] buffer = new byte[2];
            stream.Read(buffer, 0, 2);
        
            // XRP Ledger uses big-endian
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(buffer);
            }
        
            return BitConverter.ToUInt16(buffer, 0);
        }
        
        private uint ReadUInt32(MemoryStream stream)
        {
            byte[] buffer = new byte[4];
            stream.Read(buffer, 0, 4);
        
            // XRP Ledger uses big-endian
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(buffer);
            }
        
            return BitConverter.ToUInt32(buffer, 0);
        }

        private ulong ReadUInt64(MemoryStream stream)
        {
            byte[] buffer = new byte[8];
            stream.Read(buffer, 0, 8);
        
            // XRP Ledger uses big-endian
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(buffer);
            }
        
            return BitConverter.ToUInt64(buffer, 0);
        }

        internal (FieldTypes.SerializedType, int) ReadFieldHeader(MemoryStream stream)
        {
            byte firstByte = (byte)stream.ReadByte();
        
            byte typeCode = (byte)((firstByte & 0xF0) >> 4);
            byte fieldCode = (byte)(firstByte & 0x0F);
        
            // Handle special cases for type and field codes
            if (typeCode == 0)
            {
                // Type code is in the next byte
                typeCode = (byte)stream.ReadByte();
            
                if (fieldCode == 0)
                {
                    // Field code is in the byte after the type code
                    fieldCode = (byte)stream.ReadByte();
                }
            }
            else if (fieldCode == 0)
            {
                // Field code is in the next byte
                fieldCode = (byte)stream.ReadByte();
            }
        
            return ((FieldTypes.SerializedType)typeCode, fieldCode);
        }
        
        private string ReadHex(MemoryStream stream, FieldTypes.SerializedType type)
        {
            int length;
        
            switch (type)
            {
                case FieldTypes.SerializedType.Uint128:
                    length = 16;
                    break;
                case FieldTypes.SerializedType.Uint256:
                    length = 32;
                    break;
                default:
                    throw new ArgumentException($"Invalid type for ReadHex: {type}");
            }
        
            return ReadHex(stream, length);
        }

        private string ReadHex(MemoryStream stream, int length)
        {
            byte[] buffer = new byte[length];
            stream.Read(buffer, 0, length);
            return Convert.ToHexString(buffer).ToLower();
        }

        private string ReadAccountID(MemoryStream stream)
        {
            // Account IDs are 20 bytes
            return ReadHex(stream, 20);
        }
        
        private object ReadAmount(MemoryStream stream)
        {
            // The first byte's high bit indicates whether it's XRP (0) or an issued currency (1)
            byte[] buffer = new byte[8];
            stream.Read(buffer, 0, 1);
            bool isXRP = (buffer[0] & 0x80) == 0;
            
            // Rewind to read the full amount
            stream.Position -= 1;
            
            if (isXRP)
            {
                // XRP is stored as a 64-bit unsigned integer
                stream.Read(buffer, 0, 8);
                
                // Clear currency type bits
                buffer[0] &= 0x3F;
                
                // XRP Ledger uses big-endian
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(buffer);
                }
                
                return BitConverter.ToUInt64(buffer, 0).ToString();
            }
            else
            {
                // Issued currency amount (160 bits)
                byte[] issuedBuffer = new byte[48];
                stream.Read(issuedBuffer, 0, 48);
                
                // Extract value, currency, and issuer
                // For simplicity, returning a placeholder IssuedCurrencyAmount
                var currency = ExtractCurrencyFromBuffer(issuedBuffer);
                var issuer = ExtractIssuerFromBuffer(issuedBuffer);
                var value = ExtractValueFromBuffer(issuedBuffer);
                
                return new XRPL.NET.Models.Transactions.Common.IssuedCurrencyAmount
                {
                    Currency = currency,
                    Issuer = issuer,
                    Value = value
                };
            }
        }

        private string ExtractCurrencyFromBuffer(byte[] buffer)
        {
            // Currency code is stored at offset 8-28
            byte[] currencyBytes = new byte[20];
            Array.Copy(buffer, 8, currencyBytes, 0, 20);
            
            // Check if it's a standard 3-letter ISO code
            if (currencyBytes[0] == 0 && currencyBytes[1] == 0 && 
                currencyBytes[2] == 0 && currencyBytes[12] == 0)
            {
                char c1 = (char)currencyBytes[15];
                char c2 = (char)currencyBytes[14];
                char c3 = (char)currencyBytes[13];
                
                if (char.IsLetterOrDigit(c1) && char.IsLetterOrDigit(c2) && char.IsLetterOrDigit(c3))
                {
                    return new string(new[] { c1, c2, c3 });
                }
            }
            
            // Otherwise, return the hex representation
            return Convert.ToHexString(currencyBytes).ToLower();
        }

        private string ExtractIssuerFromBuffer(byte[] buffer)
        {
            // Issuer is stored at offset 28-48
            byte[] issuerBytes = new byte[20];
            Array.Copy(buffer, 28, issuerBytes, 0, 20);
            return Convert.ToHexString(issuerBytes).ToLower();
        }

        private string ExtractValueFromBuffer(byte[] buffer)
        {
            // Value extraction is complex due to the mantissa/exponent format
            // For simplicity, we're returning a placeholder
            return "0";
        }

        private byte[] ReadVarLengthBytes(MemoryStream stream)
        {
            int length = ReadVarLength(stream);
            byte[] buffer = new byte[length];
            stream.Read(buffer, 0, length);
            return buffer;
        }

        private int ReadVarLength(MemoryStream stream)
        {
            byte firstByte = (byte)stream.ReadByte();
            
            if (firstByte <= 192)
            {
                return firstByte;
            }
            else if (firstByte <= 240)
            {
                byte secondByte = (byte)stream.ReadByte();
                return 193 + ((firstByte - 193) * 256) + secondByte;
            }
            else
            {
                byte secondByte = (byte)stream.ReadByte();
                byte thirdByte = (byte)stream.ReadByte();
                return 12481 + ((firstByte - 241) * 65536) + (secondByte * 256) + thirdByte;
            }
        }

        private Dictionary<string, object> ReadObject(MemoryStream stream)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            
            while (stream.Position < stream.Length)
            {
                byte firstByte = (byte)stream.ReadByte();
                
                // Check for end marker
                if (firstByte == 0xE1)
                {
                    break;
                }
                
                // Rewind to reread the first byte
                stream.Position -= 1;
                
                // Parse the field header
                (FieldTypes.SerializedType fieldType, int fieldCode) = ReadFieldHeader(stream);
                
                // Get field name
                string fieldName = GetFieldNameFromTypeAndCode(fieldType, fieldCode);
                
                // Read the field value
                object fieldValue = ReadFieldValue(stream, fieldType);
                
                result[fieldName] = fieldValue;
            }
            
            return result;
        }

        private List<object> ReadArray(MemoryStream stream)
        {
            List<object> result = new List<object>();
            
            while (stream.Position < stream.Length)
            {
                byte marker = (byte)stream.ReadByte();
                
                if (marker == 0xF1) // Array end marker
                {
                    break;
                }
                else if (marker == 0xF0) // Array element marker
                {
                    Dictionary<string, object> element = ReadObject(stream);
                    result.Add(element);
                }
                else
                {
                    // Unexpected marker, rewind and break
                    stream.Position -= 1;
                    break;
                }
            }
            
            return result;
        }

        private string ReadCurrency(MemoryStream stream)
        {
            byte[] buffer = new byte[20];
            stream.Read(buffer, 0, 20);
            
            // Check if it's a standard 3-letter ISO code
            if (buffer[0] == 0 && buffer[1] == 0 && 
                buffer[2] == 0 && buffer[12] == 0)
            {
                char c1 = (char)buffer[15];
                char c2 = (char)buffer[14];
                char c3 = (char)buffer[13];
                
                if (char.IsLetterOrDigit(c1) && char.IsLetterOrDigit(c2) && char.IsLetterOrDigit(c3))
                {
                    return new string(new[] { c1, c2, c3 });
                }
            }
            
            // Otherwise, return the hex representation
            return Convert.ToHexString(buffer).ToLower();
        }

        internal TransactionBase CreateTransactionObject(string transactionType)
        {
            return transactionType switch
            {
                TransactionTypes.Payment => new PaymentTransaction(),
                TransactionTypes.AccountSet => new AccountSetTransaction(),
                TransactionTypes.TrustSet => new TrustSetTransaction(),
                TransactionTypes.OfferCreate => new OfferCreateTransaction(),
                TransactionTypes.OfferCancel => new OfferCancelTransaction(),
                TransactionTypes.EscrowCreate => new EscrowCreateTransaction(),
                TransactionTypes.EscrowFinish => new EscrowFinishTransaction(),
                TransactionTypes.EscrowCancel => new EscrowCancelTransaction(),
                TransactionTypes.NFTokenMint => new NFTokenMintTransaction(),
                TransactionTypes.NFTokenBurn => new NFTokenBurnTransaction(),
                TransactionTypes.NFTokenCreateOffer => new NFTokenCreateOfferTransaction(),
                TransactionTypes.NFTokenAcceptOffer => new NFTokenAcceptOfferTransaction(),
                TransactionTypes.NFTokenCancelOffer => new NFTokenCancelOfferTransaction(),
                _ => throw new XrplException($"Unsupported transaction type: {transactionType}")
            };
        }

        internal void SetTransactionProperty(object transaction, string propertyName, object value)
        {
            var property = transaction.GetType().GetProperty(propertyName);
            
            if (property != null)
            {
                try
                {
                    // Convert the value to the property type if needed
                    var convertedValue = ConvertToPropertyType(value, property.PropertyType);
                    property.SetValue(transaction, convertedValue);
                }
                catch (Exception ex)
                {
                    throw new XrplException($"Failed to set property {propertyName}: {ex.Message}", ex);
                }
            }
        }

        private object ConvertToPropertyType(object value, Type targetType)
        {
            if (value == null || targetType.IsInstanceOfType(value))
            {
                return value;
            }
            
            // Handle common conversions
            if (targetType == typeof(string))
            {
                if (value is byte[] bytes)
                {
                    return Convert.ToHexString(bytes).ToLower();
                }
                
                return value.ToString();
            }
            
            if (targetType == typeof(uint) && value is ulong ulongValue)
            {
                return (uint)ulongValue;
            }
            
            if (targetType == typeof(decimal) && value is string strValue)
            {
                if (decimal.TryParse(strValue, out decimal decimalValue))
                {
                    return decimalValue;
                }
            }
            
            // Try using Convert for basic type conversions
            try
            {
                return Convert.ChangeType(value, targetType);
            }
            catch
            {
                // Return default value if conversion fails
                return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
            }
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
}