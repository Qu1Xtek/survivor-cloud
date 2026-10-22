using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;

namespace CloudLogic.Services.Security
{
    /// <summary>
    /// Service for signing and verifying data integrity using HMAC-SHA256
    /// </summary>
    public class DataSigningService : IDataSigningService
    {
        private readonly ILogger _logger;
        private readonly byte[] _signingKey;

        public DataSigningService(ILogger logger, string signingKeyBase64)
        {
            _logger = logger;
            
            if (string.IsNullOrEmpty(signingKeyBase64))
            {
                throw new ArgumentException("Signing key cannot be null or empty", nameof(signingKeyBase64));
            }

            try
            {
                _signingKey = Convert.FromBase64String(signingKeyBase64);
                
                // Validate key size (recommended minimum 32 bytes)
                if (_signingKey.Length < 32)
                {
                    throw new ArgumentException("Signing key must be at least 32 bytes when decoded from Base64", nameof(signingKeyBase64));
                }
            }
            catch (FormatException ex)
            {
                throw new ArgumentException("Invalid Base64 signing key format", nameof(signingKeyBase64), ex);
            }
        }

        /// <summary>
        /// Generates an HMAC signature for the given data
        /// </summary>
        public string SignData(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                throw new ArgumentException("Data cannot be null or empty", nameof(data));
            }

            try
            {
                using (var hmac = new HMACSHA256(_signingKey))
                {
                    var dataBytes = Encoding.UTF8.GetBytes(data);
                    var hashBytes = hmac.ComputeHash(dataBytes);
                    return Convert.ToBase64String(hashBytes);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error signing data");
                throw new CryptographicException("Failed to sign data", ex);
            }
        }

        /// <summary>
        /// Verifies the signature of the given data
        /// </summary>
        public bool VerifySignature(string data, string signature)
        {
            if (string.IsNullOrEmpty(data) || string.IsNullOrEmpty(signature))
            {
                return false;
            }

            try
            {
                var expectedSignature = SignData(data);
                
                // Use constant-time comparison to prevent timing attacks
                return ConstantTimeEquals(expectedSignature, signature);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying signature");
                return false;
            }
        }

        /// <summary>
        /// Constant-time string comparison to prevent timing attacks
        /// </summary>
        private bool ConstantTimeEquals(string a, string b)
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

        /// <summary>
        /// Signs data with timestamp to prevent replay attacks
        /// </summary>
        public string SignDataWithTimestamp(string data, DateTime timestamp)
        {
            var timestampedData = $"{timestamp:O}|{data}";
            return SignData(timestampedData);
        }

        /// <summary>
        /// Verifies signature with timestamp and checks if it's within valid time window
        /// </summary>
        public bool VerifySignatureWithTimestamp(string data, string signature, DateTime timestamp, TimeSpan validWindow)
        {
            if (DateTime.UtcNow - timestamp > validWindow)
            {
                _logger.LogWarning("Signature timestamp is outside valid window");
                return false;
            }

            return VerifySignature($"{timestamp:O}|{data}", signature);
        }
    }
}
