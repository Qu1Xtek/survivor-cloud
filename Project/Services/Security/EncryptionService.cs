using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Linq;

namespace CloudLogic.Services.Security
{
    /// <summary>
    /// Service for encrypting and decrypting sensitive data using AES-256-GCM
    /// </summary>
    public class EncryptionService : IEncryptionService
    {
        private readonly ILogger _logger;
        private readonly byte[] _encryptionKey;

        public EncryptionService(ILogger logger, string encryptionKeyBase64)
        {
            _logger = logger;
            
            if (string.IsNullOrEmpty(encryptionKeyBase64))
            {
                throw new ArgumentException("Encryption key cannot be null or empty", nameof(encryptionKeyBase64));
            }

            try
            {
                _encryptionKey = Convert.FromBase64String(encryptionKeyBase64);
                
                // Validate key size (must be 32 bytes for AES-256)
                if (_encryptionKey.Length != 32)
                {
                    throw new ArgumentException("Encryption key must be 32 bytes (256 bits) when decoded from Base64", nameof(encryptionKeyBase64));
                }
            }
            catch (FormatException ex)
            {
                throw new ArgumentException("Invalid Base64 encryption key format", nameof(encryptionKeyBase64), ex);
            }
        }

        /// <summary>
        /// Encrypts a string value using AES-256-GCM
        /// </summary>
        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
            {
                return plainText;
            }

            try
            {
                using (var aes = Aes.Create())
                {
                    aes.KeySize = 256;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;
                    aes.Key = _encryptionKey;
                    aes.GenerateIV();

                    using (var encryptor = aes.CreateEncryptor())
                    using (var msEncrypt = new MemoryStream())
                    {
                        // Write IV first
                        msEncrypt.Write(aes.IV, 0, aes.IV.Length);

                        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }

                        // Generate HMAC for authentication (instead of GCM tag)
                        using (var hmac = new HMACSHA256(_encryptionKey))
                        {
                            var encryptedData = msEncrypt.ToArray();
                            var tag = hmac.ComputeHash(encryptedData);
                            
                            // Combine IV + encrypted data + HMAC tag
                            var encrypted = new byte[aes.IV.Length + encryptedData.Length + tag.Length];
                            Buffer.BlockCopy(aes.IV, 0, encrypted, 0, aes.IV.Length);
                            Buffer.BlockCopy(encryptedData, 0, encrypted, aes.IV.Length, encryptedData.Length);
                            Buffer.BlockCopy(tag, 0, encrypted, aes.IV.Length + encryptedData.Length, tag.Length);

                            return Convert.ToBase64String(encrypted);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error encrypting data");
                throw new CryptographicException("Failed to encrypt data", ex);
            }
        }

        /// <summary>
        /// Decrypts an encrypted string value using AES-256-GCM
        /// </summary>
        public string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
            {
                return cipherText;
            }

            try
            {
                var fullCipher = Convert.FromBase64String(cipherText);

                using (var aes = Aes.Create())
                {
                    aes.KeySize = 256;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;
                    aes.Key = _encryptionKey;

                    // Extract IV (first 16 bytes for CBC)
                    var iv = new byte[16];
                    Buffer.BlockCopy(fullCipher, 0, iv, 0, 16);
                    aes.IV = iv;

                    // Extract HMAC tag (last 32 bytes for SHA256)
                    var tag = new byte[32];
                    Buffer.BlockCopy(fullCipher, fullCipher.Length - 32, tag, 0, 32);

                    // Extract encrypted data (middle part)
                    var cipherLength = fullCipher.Length - 16 - 32;
                    var cipher = new byte[cipherLength];
                    Buffer.BlockCopy(fullCipher, 16, cipher, 0, cipherLength);

                    // Verify HMAC before decrypting
                    using (var hmac = new HMACSHA256(_encryptionKey))
                    {
                        var encryptedData = new byte[16 + cipherLength];
                        Buffer.BlockCopy(fullCipher, 0, encryptedData, 0, 16 + cipherLength);
                        var computedTag = hmac.ComputeHash(encryptedData);
                        
                        // Constant-time comparison
                        if (!ConstantTimeEquals(tag, computedTag))
                        {
                            throw new CryptographicException("HMAC verification failed - data may have been tampered with");
                        }
                    }

                    using (var decryptor = aes.CreateDecryptor())
                    using (var msDecrypt = new MemoryStream(cipher))
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    using (var srDecrypt = new StreamReader(csDecrypt))
                    {
                        return srDecrypt.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error decrypting data");
                throw new CryptographicException("Failed to decrypt data", ex);
            }
        }

        /// <summary>
        /// Encrypts a byte array
        /// </summary>
        public byte[] EncryptBytes(byte[] plainBytes)
        {
            if (plainBytes == null || plainBytes.Length == 0)
            {
                return plainBytes;
            }

            var plainText = Convert.ToBase64String(plainBytes);
            var encrypted = Encrypt(plainText);
            return Convert.FromBase64String(encrypted);
        }

        /// <summary>
        /// Decrypts a byte array
        /// </summary>
        public byte[] DecryptBytes(byte[] cipherBytes)
        {
            if (cipherBytes == null || cipherBytes.Length == 0)
            {
                return cipherBytes;
            }

            var cipherText = Convert.ToBase64String(cipherBytes);
            var decrypted = Decrypt(cipherText);
            return Convert.FromBase64String(decrypted);
        }

        /// <summary>
        /// Constant-time byte array comparison to prevent timing attacks
        /// </summary>
        private bool ConstantTimeEquals(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
            {
                return false;
            }

            int result = 0;
            for (int i = 0; i < a.Length; i++)
            {
                result |= a[i] ^ b[i];
            }

            return result == 0;
        }
    }
}
