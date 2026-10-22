using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;

namespace CloudLogic.Services.Security
{
    /// <summary>
    /// Server-side key derivation matching Unity client implementation
    /// Derives keys from device-specific data + server seed
    /// </summary>
    public class SecureKeyDerivation
    {
        private readonly ILogger _logger;

        public SecureKeyDerivation(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Derives encryption keys from device fingerprint and server seed
        /// Must match the Unity client implementation exactly
        /// </summary>
        public (string EncryptionKey, string SigningKey) DeriveKeys(string deviceFingerprint, string serverSeed)
        {
            if (string.IsNullOrEmpty(serverSeed))
            {
                throw new ArgumentException("Server seed cannot be null or empty", nameof(serverSeed));
            }

            if (string.IsNullOrEmpty(deviceFingerprint))
            {
                throw new ArgumentException("Device fingerprint cannot be null or empty", nameof(deviceFingerprint));
            }

            // Derive encryption key using PBKDF2 (must match Unity client)
            var encryptionKey = DeriveKey(deviceFingerprint, serverSeed, "encryption", 32, 100000);
            var signingKey = DeriveKey(deviceFingerprint, serverSeed, "signing", 32, 100000);

            return (encryptionKey, signingKey);
        }

        /// <summary>
        /// Derives a key using PBKDF2 with SHA-256
        /// Must match Unity client implementation exactly
        /// </summary>
        private string DeriveKey(string deviceFingerprint, string serverSeed, string purpose, int keyLength, int iterations)
        {
            // Combine all inputs to create unique salt (must match Unity)
            var saltString = $"{deviceFingerprint}:{serverSeed}:{purpose}:Taralej2024";
            var salt = Encoding.UTF8.GetBytes(saltString);

            // Use PBKDF2 to derive key
            using (var pbkdf2 = new Rfc2898DeriveBytes(serverSeed, salt, iterations, HashAlgorithmName.SHA256))
            {
                var key = pbkdf2.GetBytes(keyLength);
                return Convert.ToBase64String(key);
            }
        }

        /// <summary>
        /// Alternative: Simple HMAC-based derivation (faster but less secure than PBKDF2)
        /// </summary>
        public (string EncryptionKey, string SigningKey) DeriveKeysSimple(string deviceId, string serverSeed)
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(deviceId)))
            {
                var encryptionKeyBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(serverSeed + ":encryption"));
                var signingKeyBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(serverSeed + ":signing"));
                
                return (
                    Convert.ToBase64String(encryptionKeyBytes),
                    Convert.ToBase64String(signingKeyBytes)
                );
            }
        }
    }
}
