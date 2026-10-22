# Security Setup Guide

This guide explains how to set up encryption and data protection between the Unity mobile client and the cloud backend.

## Overview

The security system provides:
- **AES-256-GCM Encryption**: Encrypts sensitive data at rest and in transit
- **HMAC-SHA256 Signing**: Verifies data integrity and prevents tampering
- **Secure Data Packages**: Combined encryption and signing for complete protection

## Step 1: Generate Encryption Keys

### Option A: Use the Key Generator (Recommended)

Create a simple console app or run this in a test project:

```csharp
using CloudLogic.Util;

KeyGenerator.PrintKeysForEnvironment();
```

This will output:
```
ENCRYPTION_KEY=<base64-encoded-32-byte-key>
SIGNING_KEY=<base64-encoded-32-byte-key>
```

### Option B: Generate Manually

```csharp
using System;
using System.Security.Cryptography;

// Generate encryption key (32 bytes = 256 bits for AES-256)
var encryptionKey = new byte[32];
RandomNumberGenerator.Fill(encryptionKey);
var encryptionKeyBase64 = Convert.ToBase64String(encryptionKey);

// Generate signing key (32+ bytes recommended)
var signingKey = new byte[32];
RandomNumberGenerator.Fill(signingKey);
var signingKeyBase64 = Convert.ToBase64String(signingKey);
```

## Step 2: Store Keys Securely

### ⚠️ CRITICAL: Never commit keys to version control!

### For Cloud Backend (TaralejCloud)

**Development:**
1. Create `appsettings.Development.json` (if it doesn't exist)
2. Add keys:
```json
{
  "Encryption": {
    "EncryptionKey": "<your-encryption-key-base64>",
    "SigningKey": "<your-signing-key-base64>"
  }
}
```
3. Add `appsettings.Development.json` to `.gitignore`

**Production:**
- **Azure**: Use Azure Key Vault
- **AWS**: Use AWS Secrets Manager
- **Google Cloud**: Use Secret Manager
- **Environment Variables**: Set in your hosting platform

Example for environment variables:
```bash
export ENCRYPTION_KEY="<your-encryption-key-base64>"
export SIGNING_KEY="<your-signing-key-base64>"
```

### For Unity Client (taralej-mobile)

**Option 1: ScriptableObject (Development Only)**
1. Create a `SecurityConfig` ScriptableObject in Unity
2. Set the keys in the inspector
3. **Add to .gitignore** - Never commit this file!

**Option 2: Environment Variables (Production)**
- Use Unity Cloud Build environment variables
- Or inject at build time via CI/CD

**Option 3: Remote Config (Recommended for Production)**
- Store keys in Unity Remote Config
- Download and cache securely on first launch
- Rotate keys periodically

## Step 3: Configure Cloud Backend

The encryption services are automatically registered in `ServiceProvider.cs` if the environment variables are set:

```csharp
// In ServiceProvider.cs (already configured)
var encryptionKey = Environment.GetEnvironmentVariable("ENCRYPTION_KEY");
var signingKey = Environment.GetEnvironmentVariable("SIGNING_KEY");
```

If keys are not found, the service will throw an exception on initialization.

## Step 4: Configure Unity Client

### Using SecurityConfig ScriptableObject

1. In Unity, right-click in Project window → Create → Taralej → Security Config
2. Name it `SecurityConfig` (or your preferred name)
3. Set the encryption and signing keys
4. Create a singleton or service locator to access it:

```csharp
public class SecurityManager : MonoBehaviour
{
    [SerializeField] private SecurityConfig securityConfig;
    private SecureDataService secureDataService;

    void Awake()
    {
        secureDataService = securityConfig.CreateSecureDataService();
    }

    public SecureDataService GetSecureDataService() => secureDataService;
}
```

### Using Environment Variables

```csharp
public class SecurityManager : MonoBehaviour
{
    private SecureDataService secureDataService;

    void Awake()
    {
        var encryptionKey = System.Environment.GetEnvironmentVariable("ENCRYPTION_KEY");
        var signingKey = System.Environment.GetEnvironmentVariable("SIGNING_KEY");
        
        var encryptionService = new EncryptionService(encryptionKey);
        var signingService = new DataSigningService(signingKey);
        secureDataService = new SecureDataService(encryptionService, signingService);
    }
}
```

## Step 5: Use Encryption in Your Code

### Cloud Backend Example

```csharp
// Encrypt sensitive player data
var secureDataService = ServiceProvider.GetService<ISecureDataService>();

// Encrypt email
var encryptedEmail = secureDataService.EncryptString(playerEmail);

// Store encrypted email in database
await playerDataRepo.UpdateEmail(playerId, encryptedEmail);

// Decrypt when needed
var decryptedEmail = secureDataService.DecryptString(encryptedEmail);
```

### Unity Client Example

```csharp
// Encrypt data before sending to server
var secureDataService = securityManager.GetSecureDataService();

var playerData = new { PlayerId = "player123", Email = "player@example.com" };
var protectedPackage = secureDataService.ProtectData(playerData);

// Send protectedPackage to server
await apiClient.SendSecureData(protectedPackage);
```

## Step 6: Integration Points

### What to Encrypt

Consider encrypting:
- ✅ Player email addresses
- ✅ Device IDs
- ✅ Personal identification data
- ✅ Payment information (if stored)
- ✅ Authentication tokens
- ✅ Any PII (Personally Identifiable Information)

### What NOT to Encrypt

- ❌ Player IDs (needed for queries)
- ❌ Public game data (items, stats visible to others)
- ❌ Non-sensitive metadata

## Security Best Practices

1. **Key Rotation**: Rotate keys every 90 days
   - Generate new keys
   - Re-encrypt all data with new keys
   - Update both client and server

2. **Key Storage**: 
   - Never in code
   - Never in version control
   - Use secure key management services

3. **Access Control**: 
   - Limit who can access keys
   - Use principle of least privilege
   - Audit key access

4. **Monitoring**: 
   - Log encryption/decryption operations
   - Alert on signature verification failures
   - Monitor for unusual patterns

5. **Error Handling**: 
   - Never expose decryption errors to clients
   - Log errors internally for debugging
   - Return generic error messages to clients

## Troubleshooting

### "ENCRYPTION_KEY environment variable is not set"
- Ensure environment variables are set before the service initializes
- Check your hosting platform's environment variable configuration
- Verify the variable names are correct (case-sensitive on Linux)

### "Invalid Base64 encryption key format"
- Ensure keys are properly Base64 encoded
- Check for whitespace or newlines
- Regenerate keys if corrupted

### "Data signature verification failed"
- Keys may not match between client and server
- Data may have been tampered with
- Check that signature is stored and retrieved correctly

### Unity: "Security keys are not configured"
- Ensure SecurityConfig ScriptableObject is assigned
- Or set environment variables for the build
- Check that keys are not empty strings

## Testing

Test encryption/decryption in both directions:

```csharp
// Test on server
var service = ServiceProvider.GetService<ISecureDataService>();
var original = "test@example.com";
var encrypted = service.EncryptString(original);
var decrypted = service.DecryptString(encrypted);
Assert.AreEqual(original, decrypted);

// Test on client (same keys)
var clientService = new SecureDataService(encryptionService, signingService);
var clientDecrypted = clientService.DecryptString(encrypted);
Assert.AreEqual(original, clientDecrypted);
```

## Migration Guide

If you have existing unencrypted data:

1. **Backup**: Create a full backup of your database
2. **Encrypt**: Run a migration script to encrypt existing sensitive fields
3. **Verify**: Test that decryption works correctly
4. **Update Client**: Deploy new client with encryption support
5. **Monitor**: Watch for any decryption errors

Example migration script:
```csharp
var players = await playerRepo.GetAllPlayers();
var secureService = ServiceProvider.GetService<ISecureDataService>();

foreach (var player in players)
{
    if (!string.IsNullOrEmpty(player.Email) && !IsEncrypted(player.Email))
    {
        var encrypted = secureService.EncryptString(player.Email);
        await playerRepo.UpdateEmail(player.Id, encrypted);
    }
}
```

## Support

For issues or questions:
1. Check the logs for detailed error messages
2. Verify keys match between client and server
3. Ensure keys are properly Base64 encoded
4. Check that environment variables are accessible
