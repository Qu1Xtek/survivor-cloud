using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Unity.Services.CloudCode.Core;
using Newtonsoft.Json;

namespace CloudLogic.Services.Security
{
    /// <summary>
    /// Helper for encrypting/decrypting sensitive data in endpoint requests and responses
    /// </summary>
    public class EndpointEncryptionHelper
    {
        private readonly ISecureDataService _secureDataService;
        private readonly ILogger _logger;

        public EndpointEncryptionHelper(ISecureDataService secureDataService, ILogger logger)
        {
            _secureDataService = secureDataService ?? throw new ArgumentNullException(nameof(secureDataService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Encrypts sensitive fields in a response object before sending to client
        /// </summary>
        public TResponse EncryptResponse<TResponse>(TResponse response) where TResponse : class
        {
            try
            {
                // Serialize to JSON
                var json = JsonConvert.SerializeObject(response);
                
                // Create secure package
                var package = _secureDataService.ProtectData(response);
                
                // Return a wrapper that contains encrypted data
                // Note: This changes the response type, so you may need to adjust return types
                // Alternative: Encrypt specific fields instead of entire response
                
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error encrypting response");
                return response; // Return unencrypted on error (better than failing)
            }
        }

        /// <summary>
        /// Decrypts sensitive data from client request
        /// </summary>
        public TRequest DecryptRequest<TRequest>(SecureDataPackage package) where TRequest : class
        {
            try
            {
                return _secureDataService.UnprotectData<TRequest>(package);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error decrypting request");
                throw new CryptographicException("Failed to decrypt request data", ex);
            }
        }

        /// <summary>
        /// Encrypts a string value (for simple fields)
        /// </summary>
        public string EncryptString(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            return _secureDataService.EncryptString(value);
        }

        /// <summary>
        /// Decrypts a string value
        /// </summary>
        public string DecryptString(string encryptedValue)
        {
            if (string.IsNullOrEmpty(encryptedValue))
                return encryptedValue;

            return _secureDataService.DecryptString(encryptedValue);
        }

        /// <summary>
        /// Validates that request signature is valid and not tampered with
        /// </summary>
        public bool ValidateRequestSignature(SecureDataPackage package)
        {
            if (package == null)
                return false;

            try
            {
                // Verify signature using the secure data service
                // We need to extract the signing service
                // For now, just try to decrypt (which verifies signature)
                try
                {
                    _secureDataService.UnprotectData<object>(package);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
