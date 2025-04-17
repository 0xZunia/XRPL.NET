using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Math;
using XRPL.NET.Wallet.Derivation;

namespace XRPL.NET.Wallet
{
    /// <summary>
    /// Provides functionality for managing XRP Ledger wallets, including key derivation and signing.
    /// </summary>
    public class XrplWallet
    {
        // Constants for seed encoding and derivation
        private const string ALPHABET = "rpshnaf39wBUDNEGHJKLM4PQRST7VWXYZ2bcdeCg65jkm8oFqi1tuvAxyz";
        private const string FAMILY_SEED = "secp256k1";
        private const byte SEED_PREFIX = 0x21; // Used for encoding seeds ('s')
        private const byte ACCOUNT_ID_PREFIX = 0x00; // Used for encoding addresses ('r')
        
        /// <summary>
        /// Gets the account ID (address) of this wallet.
        /// </summary>
        public string Address { get; }
        
        /// <summary>
        /// Gets the public key of this wallet.
        /// </summary>
        public string PublicKey { get; }
        
        /// <summary>
        /// Gets a value indicating whether this wallet is a read-only wallet (no private key).
        /// </summary>
        public bool IsReadOnly => string.IsNullOrEmpty(_privateKey);
        
        private readonly string _privateKey;
        private readonly string? _seed;
        
        /// <summary>
        /// Initializes a new instance of the XrplWallet class.
        /// </summary>
        /// <param name="address">The account address.</param>
        /// <param name="publicKey">The public key.</param>
        /// <param name="privateKey">The private key.</param>
        /// <param name="seed">The seed used to generate the key pair (optional).</param>
        private XrplWallet(string address, string publicKey, string privateKey, string? seed = null)
        {
            Address = address;
            PublicKey = publicKey;
            _privateKey = privateKey;
            _seed = seed;
        }
        
        /// <summary>
        /// Creates a new random wallet.
        /// </summary>
        /// <returns>A new wallet.</returns>
        public static XrplWallet Generate()
        {
            // Generate a random seed (16 bytes)
            byte[] seedBytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(seedBytes);
            }
            
            // Encode the seed in base58 format
            string seed = EncodeSeed(seedBytes);
            
            // Derive key pair from seed
            var (publicKey, privateKey) = DeriveKeyPair(seedBytes);
            
            // Derive address from public key
            string address = DeriveAddress(Convert.FromHexString(publicKey));
            
            return new XrplWallet(address, publicKey, privateKey, seed);
        }
        
        /// <summary>
        /// Creates a wallet from a seed.
        /// </summary>
        /// <param name="seed">The seed in base58 encoding.</param>
        /// <returns>A wallet derived from the seed.</returns>
        public static XrplWallet FromSeed(string seed)
        {
            // Validate and decode seed
            byte[] seedBytes = DecodeSeed(seed);
            
            // Derive key pair from seed
            var (publicKey, privateKey) = DeriveKeyPair(seedBytes);
            
            // Derive address from public key
            string address = DeriveAddress(Convert.FromHexString(publicKey));
            
            return new XrplWallet(address, publicKey, privateKey, seed);
        }
        
        /// <summary>
        /// Creates a wallet from a private key.
        /// </summary>
        /// <param name="privateKey">The private key in hex format.</param>
        /// <returns>A wallet derived from the private key.</returns>
        public static XrplWallet FromPrivateKey(string privateKey)
        {
            // Validate private key
            if (string.IsNullOrEmpty(privateKey) || privateKey.Length != 64)
            {
                throw new ArgumentException("Invalid private key", nameof(privateKey));
            }
            
            try
            {
                // Convert hex to bytes
                byte[] privateKeyBytes = Convert.FromHexString(privateKey);
                
                // Get public key from private key
                string publicKey = GetPublicKeyFromPrivateKey(privateKeyBytes);
                
                // Derive address from public key
                string address = DeriveAddress(Convert.FromHexString(publicKey));
                
                return new XrplWallet(address, publicKey, privateKey);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Invalid private key", nameof(privateKey), ex);
            }
        }
        
        /// <summary>
        /// Creates a read-only wallet from a public key.
        /// </summary>
        /// <param name="publicKey">The public key in hex format.</param>
        /// <returns>A read-only wallet derived from the public key.</returns>
        public static XrplWallet FromPublicKey(string publicKey)
        {
            // Validate public key
            if (string.IsNullOrEmpty(publicKey) || (publicKey.Length != 66 && publicKey.Length != 130))
            {
                throw new ArgumentException("Invalid public key", nameof(publicKey));
            }
            
            try
            {
                // Derive address from public key
                string address = DeriveAddress(Convert.FromHexString(publicKey));
                
                return new XrplWallet(address, publicKey, string.Empty);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Invalid public key", nameof(publicKey), ex);
            }
        }
        
        /// <summary>
        /// Creates a read-only wallet from an address.
        /// </summary>
        /// <param name="address">The account address.</param>
        /// <returns>A read-only wallet for the address.</returns>
        public static XrplWallet FromAddress(string address)
        {
            // Validate address
            if (!ValidateAddress(address))
            {
                throw new ArgumentException("Invalid address", nameof(address));
            }
            
            return new XrplWallet(address, string.Empty, string.Empty);
        }
        
        /// <summary>
        /// Signs a transaction with this wallet's private key.
        /// </summary>
        /// <param name="transaction">The transaction to sign.</param>
        /// <returns>The signed transaction in hex format.</returns>
        public string SignTransaction(object transaction)
        {
            if (IsReadOnly)
            {
                throw new InvalidOperationException("Cannot sign transactions with a read-only wallet");
            }
            
            try
            {
                // TODO: Verify transaction object structure
                string txJson = JsonSerializer.Serialize(transaction);
                byte[] txBytes = Encoding.UTF8.GetBytes(txJson);
                
                // Hash the transaction bytes using SHA-512 and take first half
                using var sha512 = SHA512.Create();
                byte[] txHash = sha512.ComputeHash(txBytes);
                byte[] hashBytes = new byte[32];
                Array.Copy(txHash, hashBytes, 32);
                
                // Sign the hash with the private key
                byte[] privateKeyBytes = Convert.FromHexString(_privateKey);
                byte[] signature = SignHash(hashBytes, privateKeyBytes);
                
                // TODO: Add transaction signature to the transaction object
                return Convert.ToHexString(signature).ToLower();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to sign transaction", ex);
            }
        }
        
        /// <summary>
        /// Verifies a signature.
        /// </summary>
        /// <param name="message">The message that was signed.</param>
        /// <param name="signature">The signature to verify.</param>
        /// <returns>True if the signature is valid; otherwise, false.</returns>
        public bool VerifySignature(byte[] message, byte[] signature)
        {
            if (string.IsNullOrEmpty(PublicKey))
            {
                throw new InvalidOperationException("Cannot verify signatures with a wallet that has no public key");
            }
            
            try
            {
                // Hash the message using SHA-512 and take first half
                using var sha512 = SHA512.Create();
                byte[] messageHash = sha512.ComputeHash(message);
                byte[] hashBytes = new byte[32];
                Array.Copy(messageHash, hashBytes, 32);
                
                // Verify the signature using the public key
                byte[] publicKeyBytes = Convert.FromHexString(PublicKey);
                return VerifySignature(hashBytes, signature, publicKeyBytes);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Signature verification failed", ex);
            }
        }
        
        /// <summary>
        /// Gets the secret from this wallet.
        /// </summary>
        /// <returns>The secret (seed or private key).</returns>
        public string GetSecret()
        {
            if (IsReadOnly)
            {
                throw new InvalidOperationException("Cannot get secret from a read-only wallet");
            }
            
            return _seed ?? _privateKey;
        }
        
        /// <summary>
        /// Exports the wallet to a JSON string.
        /// </summary>
        /// <param name="includeSecrets">Whether to include secrets in the export.</param>
        /// <returns>The wallet JSON.</returns>
        public string ToJson(bool includeSecrets = false)
        {
            var walletData = new Dictionary<string, string>
            {
                ["address"] = Address,
                ["public_key"] = PublicKey
            };
            
            if (includeSecrets && !IsReadOnly)
            {
                walletData["private_key"] = _privateKey;
                
                if (!string.IsNullOrEmpty(_seed))
                {
                    walletData["seed"] = _seed;
                }
            }
            
            return JsonSerializer.Serialize(walletData, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }
        
        /// <summary>
        /// Validates an XRP Ledger address.
        /// </summary>
        /// <param name="address">The address to validate.</param>
        /// <returns>True if the address is valid; otherwise, false.</returns>
        public static bool ValidateAddress(string address)
        {
            try
            {
                if (string.IsNullOrEmpty(address) || !address.StartsWith('r'))
                {
                    return false;
                }
                
                // Decode base58 address
                byte[] decoded = Base58Decode(address);
                
                if (decoded.Length < 25 || decoded.Length > 35)
                {
                    return false;
                }
                
                // Check account ID prefix
                if (decoded[0] != ACCOUNT_ID_PREFIX)
                {
                    return false;
                }
                
                // TODO: Check checksum
                
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        #region Private Crypto Methods
        
        private static string EncodeSeed(byte[] seedBytes)
        {
            // Prepend version byte
            byte[] versionedSeed = new byte[seedBytes.Length + 1];
            versionedSeed[0] = SEED_PREFIX;
            Array.Copy(seedBytes, 0, versionedSeed, 1, seedBytes.Length);
            
            // Calculate checksum (4 bytes)
            byte[] checksum = CalculateChecksum(versionedSeed);
            
            // Combine versioned seed and checksum
            byte[] combined = new byte[versionedSeed.Length + 4];
            Array.Copy(versionedSeed, 0, combined, 0, versionedSeed.Length);
            Array.Copy(checksum, 0, combined, versionedSeed.Length, 4);
            
            // Encode in base58
            return Base58Encode(combined);
        }
        
        private static byte[] DecodeSeed(string encodedSeed)
        {
            // Decode base58
            byte[] decoded = Base58Decode(encodedSeed);
            
            if (decoded.Length != 21) // 16 bytes seed + 1 version byte + 4 checksum bytes
            {
                throw new ArgumentException("Invalid seed length");
            }
            
            // Verify version byte
            if (decoded[0] != SEED_PREFIX)
            {
                throw new ArgumentException("Invalid seed version");
            }
            
            // Verify checksum
            byte[] payload = new byte[17]; // version byte + 16 bytes seed
            Array.Copy(decoded, 0, payload, 0, 17);
            
            byte[] checksum = new byte[4];
            Array.Copy(decoded, 17, checksum, 0, 4);
            
            byte[] calculatedChecksum = CalculateChecksum(payload);
            
            // Compare checksums
            for (int i = 0; i < 4; i++)
            {
                if (checksum[i] != calculatedChecksum[i])
                {
                    throw new ArgumentException("Invalid seed checksum");
                }
            }
            
            // Extract the actual seed bytes
            byte[] seedBytes = new byte[16];
            Array.Copy(decoded, 1, seedBytes, 0, 16);
            
            return seedBytes;
        }
        
        private static (string publicKey, string privateKey) DeriveKeyPair(byte[] seedBytes)
        {
            // Prepare root from seed bytes + family string
            using var sha512 = SHA512.Create();
            
            // Add family key info
            byte[] rootBytes = Encoding.ASCII.GetBytes(FAMILY_SEED).Concat(seedBytes).ToArray();
            
            // Hash to get master key
            byte[] masterKeyBytes = sha512.ComputeHash(rootBytes);
            
            // Extract private and public keys
            byte[] privateKeyBytes = new byte[32];
            Array.Copy(masterKeyBytes, 0, privateKeyBytes, 0, 32);
            
            // Get public key from private key
            string publicKey = GetPublicKeyFromPrivateKey(privateKeyBytes);
            string privateKey = Convert.ToHexString(privateKeyBytes).ToLower();
            
            return (publicKey, privateKey);
        }
        
        private static string GetPublicKeyFromPrivateKey(byte[] privateKeyBytes)
        {
            // Create secp256k1 curve parameters
            var curve = SecNamedCurves.GetByName("secp256k1");
            var domain = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H);
            
            // Create key parameters
            var privKeyParams = new ECPrivateKeyParameters(new Org.BouncyCastle.Math.BigInteger(1, privateKeyBytes), domain);
            
            // Calculate public key point
            var Q = domain.G.Multiply(privKeyParams.D);
            
            // Get encoded public key (compressed)
            var pubKeyPoint = Q.Normalize();
            byte[] pubKeyBytes = new byte[33]; // 1 header byte + 32 bytes for X coordinate
            
            pubKeyBytes[0] = (byte)(pubKeyPoint.AffineYCoord.ToBigInteger().TestBit(0) ? 0x03 : 0x02); // Even/odd Y coordinate prefix
            var xBytes = pubKeyPoint.AffineXCoord.ToBigInteger().ToByteArrayUnsigned();
            
            // Pad X coordinate if needed
            int padding = 32 - xBytes.Length;
            Array.Copy(xBytes, 0, pubKeyBytes, 1 + padding, xBytes.Length);
            
            return Convert.ToHexString(pubKeyBytes).ToLower();
        }
        
        private static string DeriveAddress(byte[] publicKeyBytes)
        {
            return XrplAddressCodec.EncodeAddress(publicKeyBytes);
        }
        
        private static byte[] CalculateChecksum(byte[] data)
        {
            using var sha256 = SHA256.Create();
            byte[] hash1 = sha256.ComputeHash(data);
            byte[] hash2 = sha256.ComputeHash(hash1);
            
            byte[] checksum = new byte[4];
            Array.Copy(hash2, 0, checksum, 0, 4);
            
            return checksum;
        }
        
        private static string Base58Encode(byte[] data)
        {
            // Convert bytes to a big integer
            var intData = new BigInteger(1, data);
            
            // Constants
            var zero = BigInteger.Zero;
            var fiftyEight = new BigInteger("58");
            
            // Convert big integer to base58 string
            string result = "";
            while (intData.CompareTo(zero) > 0)
            {
                BigInteger[] divmod = intData.DivideAndRemainder(fiftyEight);
                intData = divmod[0]; // quotient
                int remainder = divmod[1].IntValue;
                result = ALPHABET[remainder] + result;
            }
            
            // Add '1's for leading zeros
            for (int i = 0; i < data.Length && data[i] == 0; i++)
            {
                result = '1' + result;
            }
            
            return result;
        }
        
        private static byte[] Base58Decode(string encoded)
        {
            // Constants
            var zero = BigInteger.Zero;
            var fiftyEight = new BigInteger("58");
            
            // Convert base58 string to big integer
            var intData = zero;
            foreach (char c in encoded)
            {
                int digit = ALPHABET.IndexOf(c);
                if (digit < 0)
                {
                    throw new FormatException($"Invalid Base58 character '{c}'");
                }
                intData = intData.Multiply(fiftyEight).Add(new BigInteger(digit.ToString()));
            }
            
            // Convert big integer to byte array
            byte[] bytes = intData.ToByteArray();
            
            // Remove sign byte if present
            if (bytes[0] == 0 && bytes.Length > 1)
            {
                byte[] tmp = new byte[bytes.Length - 1];
                Array.Copy(bytes, 1, tmp, 0, tmp.Length);
                bytes = tmp;
            }
            
            // Reverse the array (BouncyCastle uses big-endian)
            Array.Reverse(bytes);
            
            // Count leading '1's (zeros)
            int leadingZeros = 0;
            for (int i = 0; i < encoded.Length && encoded[i] == '1'; i++)
            {
                leadingZeros++;
            }
            
            // Add leading zeros
            byte[] result = new byte[bytes.Length + leadingZeros];
            Array.Copy(bytes, 0, result, leadingZeros, bytes.Length);
            
            return result;
        }
        
        private static byte[] SignHash(byte[] hashBytes, byte[] privateKeyBytes)
        {
            // Create secp256k1 curve parameters
            var curve = SecNamedCurves.GetByName("secp256k1");
            var domain = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H);
            
            // Create key parameters
            var privKeyParams = new ECPrivateKeyParameters(new Org.BouncyCastle.Math.BigInteger(1, privateKeyBytes), domain);
            
            // Create signer
            var signer = new ECDsaSigner(new HMacDsaKCalculator(new Sha256Digest()));
            signer.Init(true, privKeyParams);
            
            // Sign the hash
            var signature = signer.GenerateSignature(hashBytes);
            
            // Combine r and s values into a DER-encoded signature
            var r = signature[0];
            var s = signature[1];
            
            // Ensure s is in the lower half of the curve order
            if (s.CompareTo(domain.N.ShiftRight(1)) > 0)
            {
                s = domain.N.Subtract(s);
            }
            
            // DER encoding of ECDSA signature
            using var stream = new System.IO.MemoryStream();
            
            // Write sequence marker
            stream.WriteByte(0x30);
            
            // Calculate length of r and s
            byte[] rBytes = r.ToByteArrayUnsigned();
            byte[] sBytes = s.ToByteArrayUnsigned();
            
            // Ensure r and s are positive (add leading zero if needed)
            if (rBytes[0] >= 0x80)
            {
                var paddedrBytes = new byte[rBytes.Length + 1];
                Array.Copy(rBytes, 0, paddedrBytes, 1, rBytes.Length);
                rBytes = paddedrBytes;
            }
            
            if (sBytes[0] >= 0x80)
            {
                var paddedsBytes = new byte[sBytes.Length + 1];
                Array.Copy(sBytes, 0, paddedsBytes, 1, sBytes.Length);
                sBytes = paddedsBytes;
            }
            
            // Write sequence length
            int length = 2 + rBytes.Length + 2 + sBytes.Length;
            stream.WriteByte((byte)length);
            
            // Write r
            stream.WriteByte(0x02); // INTEGER type
            stream.WriteByte((byte)rBytes.Length); // Length
            stream.Write(rBytes);
            
            // Write s
            stream.WriteByte(0x02); // INTEGER type
            stream.WriteByte((byte)sBytes.Length); // Length
            stream.Write(sBytes);
            
            return stream.ToArray();
        }
        
        private static bool VerifySignature(byte[] hashBytes, byte[] signature, byte[] publicKeyBytes)
        {
            // Create secp256k1 curve parameters
            var curve = SecNamedCurves.GetByName("secp256k1");
            var domain = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H);
            
            // Parse public key
            var ecPoint = domain.Curve.DecodePoint(publicKeyBytes);
            var pubKeyParams = new ECPublicKeyParameters(ecPoint, domain);
            
            // Create signer
            var signer = new ECDsaSigner();
            signer.Init(false, pubKeyParams);
            
            // Parse DER signature to extract r and s values
            if (signature[0] != 0x30)
            {
                throw new ArgumentException("Invalid signature format");
            }
            
            int rLength = signature[3];
            byte[] rBytes = new byte[rLength];
            Array.Copy(signature, 4, rBytes, 0, rLength);
            
            int sPos = 4 + rLength + 1;
            int sLength = signature[sPos];
            byte[] sBytes = new byte[sLength];
            Array.Copy(signature, sPos + 1, sBytes, 0, sLength);
            
            var r = new Org.BouncyCastle.Math.BigInteger(1, rBytes);
            var s = new Org.BouncyCastle.Math.BigInteger(1, sBytes);
            
            // Verify the signature
            return signer.VerifySignature(hashBytes, r, s);
        }
        
        #endregion
    }
}