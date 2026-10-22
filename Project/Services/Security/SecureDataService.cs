using System;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace CloudLogic.Services.Security
{
    /// <summary>
    /// Combined service for encrypting and signing sensitive data
    /// </summary>
    public class SecureDataService : ISecureDataService
    {
        private readonly EncryptionService _encryptionService;
        private readonly DataSigningService _signingService;
        private readonly ILogger _logger;

        public SecureDataService(
            EncryptionService encryptionService,
            DataSigningService signingService,
            ILogger logger)
        {
            _encryptionService = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));
            _signingService = signingService ?? throw new ArgumentNullException(nameof(signingService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Encrypts and signs an object
        /// </summary>
        public SecureDataPackage ProtectData<T>(T data)
        {
            try
            {
                // Serialize to JSON
                var json = JsonSerializer.Serialize(data);
                
                // Encrypt
                var encrypted = _encryptionService.Encrypt(json);
                
                // Sign the encrypted data
                var signature = _signingService.SignData(encrypted);
                
                return new SecureDataPackage
                {
                    EncryptedData = encrypted,
                    Signature = signature,
                    Timestamp = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error protecting data");
                throw;
            }
        }

        /// <summary>
        /// Verifies signature and decrypts data
        /// </summary>
        public T UnprotectData<T>(SecureDataPackage package)
        {
            if (package == null)
            {
                throw new ArgumentNullException(nameof(package));
            }

            try
            {
                // Verify signature first
                if (!_signingService.VerifySignature(package.EncryptedData, package.Signature))
                {
                    _logger.LogWarning("Data signature verification failed");
                    throw new CryptographicException("Data signature verification failed - data may have been tampered with");
                }

                // Decrypt
                var decryptedJson = _encryptionService.Decrypt(package.EncryptedData);
                
                // Deserialize
                return JsonSerializer.Deserialize<T>(decryptedJson);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unprotecting data");
                throw;
            }
        }

        /// <summary>
        /// Encrypts a string value (for simple string fields)
        /// </summary>
        public string EncryptString(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            return _encryptionService.Encrypt(value);
        }

        /// <summary>
        /// Decrypts a string value
        /// </summary>
        public string DecryptString(string encryptedValue)
        {
            if (string.IsNullOrEmpty(encryptedValue))
            {
                return encryptedValue;
            }

            return _encryptionService.Decrypt(encryptedValue);
        }
    }

    /// <summary>
    /// Container for encrypted and signed data
    /// </summary>
    public class SecureDataPackage
    {
        public string EncryptedData { get; set; }
        public string Signature { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
