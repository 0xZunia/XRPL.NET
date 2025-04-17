using System;
using System.Security.Cryptography;
using Org.BouncyCastle.Crypto.Digests;

namespace XRPL.NET.Wallet.Derivation;

public static class XrplAddressCodec
{
    // XRP Ledger specific base58 dictionary (this ordering is critical)
    private const string XRPL_ALPHABET = "rpshnaf39wBUDNEGHJKLM4PQRST7VWXYZ2bcdeCg65jkm8oFqi1tuvAxyz";
    
    // Type prefix for addresses (encodes to 'r' in XRPL's base58)
    private const byte ADDRESS_PREFIX = 0x00;

    /// <summary>
    /// Encodes a public key to an XRP Ledger address.
    /// </summary>
    /// <param name="publicKey">The public key bytes.</param>
    /// <returns>The XRP Ledger address.</returns>
    public static string EncodeAddress(byte[] publicKey)
    {
        // 1. Calculate the Account ID (RIPEMD160 of SHA256 hash of public key)
        byte[] accountId = CalculateAccountId(publicKey);
        
        // 2. Prefix with address type (0x00)
        byte[] prefixedAccountId = new byte[accountId.Length + 1];
        prefixedAccountId[0] = ADDRESS_PREFIX;
        Array.Copy(accountId, 0, prefixedAccountId, 1, accountId.Length);
        
        // 3. Calculate checksum (first 4 bytes of double SHA256 hash)
        byte[] checksum = CalculateChecksum(prefixedAccountId);
        
        // 4. Combine payload with checksum
        byte[] dataToEncode = new byte[prefixedAccountId.Length + checksum.Length];
        Array.Copy(prefixedAccountId, 0, dataToEncode, 0, prefixedAccountId.Length);
        Array.Copy(checksum, 0, dataToEncode, prefixedAccountId.Length, checksum.Length);
        
        // 5. Encode with XRPL-specific base58
        return Base58EncodeXrpl(dataToEncode);
    }

    private static byte[] CalculateAccountId(byte[] publicKey)
    {
        // SHA256 hash of public key
        using var sha256 = SHA256.Create();
        byte[] sha256Hash = sha256.ComputeHash(publicKey);
        
        // RIPEMD160 hash of the SHA256 hash
        var ripemd160 = new RipeMD160Digest();
        byte[] accountId = new byte[20]; // RIPEMD160 produces a 20-byte hash
        ripemd160.BlockUpdate(sha256Hash, 0, sha256Hash.Length);
        ripemd160.DoFinal(accountId, 0);
        
        return accountId;
    }

    private static byte[] CalculateChecksum(byte[] data)
    {
        // First SHA256 hash
        using var sha256 = SHA256.Create();
        byte[] firstHash = sha256.ComputeHash(data);
        
        // Second SHA256 hash
        byte[] secondHash = sha256.ComputeHash(firstHash);
        
        // Take first 4 bytes as checksum
        byte[] checksum = new byte[4];
        Array.Copy(secondHash, 0, checksum, 0, 4);
        
        return checksum;
    }

    private static string Base58EncodeXrpl(byte[] data)
    {
        // Convert to BigInteger for base conversion
        // We need to handle byte array as big-endian, so reverse if needed
        byte[] encodingData = new byte[data.Length];
        Array.Copy(data, encodingData, data.Length);
        
        // Handle leading zeros - they need special treatment
        int zeroCount = 0;
        while (zeroCount < encodingData.Length && encodingData[zeroCount] == 0)
        {
            zeroCount++;
        }
        
        // Convert remaining bytes to BigInteger
        System.Numerics.BigInteger intData = 0;
        for (int i = zeroCount; i < encodingData.Length; i++)
        {
            intData = intData * 256 + encodingData[i];
        }
        
        // Convert to base58 using XRPL-specific alphabet
        string result = "";
        while (intData > 0)
        {
            int remainder = (int)(intData % 58);
            intData /= 58;
            result = XRPL_ALPHABET[remainder] + result;
        }
        
        // Add leading 'r's for each leading zero byte
        for (int i = 0; i < zeroCount; i++)
        {
            result = XRPL_ALPHABET[0] + result; // 'r' is index 0
        }
        
        return result;
    }
}
