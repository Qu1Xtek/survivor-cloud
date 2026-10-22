# Data Encryption and Security

This module provides encryption and data signing services to protect sensitive data between the Unity mobile client and the cloud backend.

## Features

- **AES-256-GCM Encryption**: Industry-standard encryption for sensitive data
- **HMAC-SHA256 Signing**: Data integrity verification to prevent tampering
- **Secure Data Packages**: Combined encryption and signing for complete protection
- **Replay Attack Prevention**: Timestamp-based signature verification

## Setup

### 1. Generate Encryption Keys

Run the key generator to create secure keys:

```csharp
// In a console app or one-time script
CloudLogic.Util.KeyGenerator.PrintKeysForEnvironment();
```

This will output:
```
ENCRYPTION_KEY=<base64-encoded-32-byte-key>
SIGNING_KEY=<base64-encoded-32-byte-key>
```

### 2. Store Keys Securely

**DO NOT** commit keys to version control. Store them in:

- **Development**: Environment variables or `appsettings.Development.json` (excluded from git)
- **Production**: 
  - Azure Key Vault
  - AWS Secrets Manager
  - Google Cloud Secret Manager
  - Environment variables in your hosting platform

### 3. Configure Service Provider

The encryption services are registered in `ServiceProvider.cs`. Ensure keys are loaded from secure storage:

```csharp
// Example: Load from environment variables
var encryptionKey = Environment.GetEnvironmentVariable("ENCRYPTION_KEY");
var signingKey = Environment.GetEnvironmentVariable("SIGNING_KEY");

RegisterSingleton<IEncryptionService>(new EncryptionService(logger, encryptionKey));
RegisterSingleton<IDataSigningService>(new DataSigningService(logger, signingKey));
RegisterSingleton<ISecureDataService>(new SecureDataService(
    GetService<IEncryptionService>(),
    GetService<IDataSigningService>(),
    logger));
```

## Usage

### Encrypting Sensitive Data

```csharp
var secureDataService = ServiceProvider.GetService<ISecureDataService>();

// Encrypt a complex object
var playerData = new { PlayerId = "player123", Gold = 1000 };
var protectedPackage = secureDataService.ProtectData(playerData);

// Store protectedPackage.EncryptedData and protectedPackage.Signature in database

// Encrypt a simple string
var encryptedEmail = secureDataService.EncryptString("player@example.com");
```

### Decrypting Data

```csharp
var secureDataService = ServiceProvider.GetService<ISecureDataService>();

// Reconstruct package from database
var package = new SecureDataPackage 
{ 
    EncryptedData = storedEncryptedData,
    Signature = storedSignature,
    Timestamp = storedTimestamp
};

// Decrypt and verify
var playerData = secureDataService.UnprotectData<PlayerData>(package);

// Decrypt simple string
var email = secureDataService.DecryptString(encryptedEmail);
```

## Integration Points

### Sensitive Fields to Encrypt

Consider encrypting:
- Player email addresses
- Payment information (if stored)
- Personal identification data
- Device IDs
- Authentication tokens
- Any PII (Personally Identifiable Information)

### Example: Encrypting Player Email

```csharp
public class PlayerDataRepo
{
    private readonly ISecureDataService _secureData;
    
    public async Task StorePlayerEmail(string playerId, string email)
    {
        var encryptedEmail = _secureData.EncryptString(email);
        
        // Store encryptedEmail in Firestore instead of plain email
        await UpdateRecord(new { Email = encryptedEmail });
    }
    
    public async Task<string> GetPlayerEmail(string playerId)
    {
        var record = await GetRecord(playerId);
        return _secureData.DecryptString(record.Email);
    }
}
```

## Security Best Practices

1. **Key Rotation**: Rotate encryption keys periodically (every 90 days recommended)
2. **Key Storage**: Never store keys in code or configuration files in version control
3. **Key Access**: Limit access to keys to only necessary services
4. **Audit Logging**: Log all encryption/decryption operations for security auditing
5. **Error Handling**: Never expose decryption errors to clients - log internally only
6. **Performance**: Cache encryption service instances - they're thread-safe

## Unity Client Integration

See the Unity client encryption utilities in the `taralej-mobile` repository. The client must use the same encryption keys to encrypt/decrypt data.

## Troubleshooting

### "Invalid Base64 encryption key format"
- Ensure the key is properly Base64 encoded
- Check for whitespace or newlines in the key

### "Encryption key must be 32 bytes"
- The encryption key must decode to exactly 32 bytes (256 bits)
- Regenerate the key if it's incorrect

### "Data signature verification failed"
- Data may have been tampered with
- Check that the signature was stored and retrieved correctly
- Verify the signing key matches between client and server
