using System;
using System.Security.Cryptography;
using System.Text;

namespace CloudLogic.Util
{
    /// <summary>
    /// Utility class for generating encryption and signing keys
    /// Run this once to generate keys, then store them securely
    /// </summary>
    public static class KeyGenerator
    {
        /// <summary>
        /// Generates a 256-bit (32-byte) encryption key for AES-256
        /// </summary>
        public static string GenerateEncryptionKey()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                var key = new byte[32]; // 256 bits
                rng.GetBytes(key);
                return Convert.ToBase64String(key);
            }
        }

        /// <summary>
        /// Generates a signing key for HMAC (recommended 32+ bytes)
        /// </summary>
        public static string GenerateSigningKey(int length = 32)
        {
            if (length < 32)
            {
                throw new ArgumentException("Signing key length must be at least 32 bytes", nameof(length));
            }

            using (var rng = RandomNumberGenerator.Create())
            {
                var key = new byte[length];
                rng.GetBytes(key);
                return Convert.ToBase64String(key);
            }
        }

        /// <summary>
        /// Generates both encryption and signing keys
        /// </summary>
        public static (string EncryptionKey, string SigningKey) GenerateKeys()
        {
            return (GenerateEncryptionKey(), GenerateSigningKey());
        }

        /// <summary>
        /// Prints keys in a format suitable for environment variables
        /// </summary>
        public static void PrintKeysForEnvironment()
        {
            var (encKey, signKey) = GenerateKeys();
            
            Console.WriteLine("=== ENCRYPTION KEYS ===");
            Console.WriteLine("Add these to your environment variables or appsettings.json:");
            Console.WriteLine();
            Console.WriteLine($"ENCRYPTION_KEY={encKey}");
            Console.WriteLine($"SIGNING_KEY={signKey}");
            Console.WriteLine();
            Console.WriteLine("⚠️  WARNING: Keep these keys secure! Never commit them to version control.");
            Console.WriteLine("⚠️  Store them in secure key management (Azure Key Vault, AWS Secrets Manager, etc.)");
        }
    }
}
