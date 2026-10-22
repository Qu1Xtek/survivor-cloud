using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CloudLogic.Services.Security
{
    /// <summary>
    /// Helper for encrypting/decrypting endpoint parameters and responses in modules
    /// </summary>
    public static class ModuleEncryptionHelper
    {
        /// <summary>
        /// Decrypts a string parameter (supports both encrypted and plaintext for backwards compatibility)
        /// </summary>
        public static string DecryptParameter(ISecureDataService secureService, string value)
        {
            if (secureService == null || string.IsNullOrEmpty(value))
                return value;

            // If encrypted (typically > 50 chars), try to decrypt
            if (value.Length > 50)
            {
                try
                {
                    return secureService.DecryptString(value);
                }
                catch
                {
                    // Not encrypted or invalid, return as-is
                    return value;
                }
            }

            return value;
        }

        /// <summary>
        /// Decrypts a list of strings
        /// </summary>
        public static List<string> DecryptParameterList(ISecureDataService secureService, List<string> values)
        {
            if (secureService == null || values == null)
                return values;

            var decrypted = new List<string>();
            foreach (var value in values)
            {
                decrypted.Add(DecryptParameter(secureService, value));
            }
            return decrypted;
        }

        /// <summary>
        /// Encrypts a response object into SecureDataPackage
        /// </summary>
        public static SecureDataPackage EncryptResponse<T>(ISecureDataService secureService, T response)
        {
            if (secureService == null || response == null)
            {
                // Return unencrypted package if encryption not available
                return new SecureDataPackage
                {
                    EncryptedData = JsonConvert.SerializeObject(response),
                    Signature = "",
                    Timestamp = DateTime.UtcNow
                };
            }

            try
            {
                return secureService.ProtectData(response);
            }
            catch
            {
                // Fallback to unencrypted on error
                return new SecureDataPackage
                {
                    EncryptedData = JsonConvert.SerializeObject(response),
                    Signature = "",
                    Timestamp = DateTime.UtcNow
                };
            }
        }
    }
}
