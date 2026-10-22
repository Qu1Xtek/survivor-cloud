using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;

public static class AppleReceiptValidator
{
    const string BundleId = "com.cherryware.dropforge";
    const string ProductionBase = "https://api.storekit.itunes.apple.com";
    const string SandboxBase = "https://api.storekit-sandbox.itunes.apple.com";

    static readonly HttpClient Http = new();
    static string _cachedKeyId;
    static string _cachedIssuerId;
    static string _cachedPrivateKey;
    static string _cachedJwt;
    static DateTime _jwtExpiry = DateTime.MinValue;

    public static async Task<string> Verify(
        IGameApiClient client, IExecutionContext ctx,
        string transactionId, string productId, ILogger logger = null)
    {
        try
        {
            if (_cachedKeyId == null)
            {
                logger?.LogInformation("AppleReceiptValidator: loading secrets");
                _cachedKeyId      = (await client.SecretManager.GetSecret(ctx, "APPLE_KEY_ID")).Value;
                _cachedIssuerId   = (await client.SecretManager.GetSecret(ctx, "APPLE_ISSUER_ID")).Value;
                _cachedPrivateKey = (await client.SecretManager.GetSecret(ctx, "APPLE_PRIVATE_KEY")).Value;
                logger?.LogInformation("AppleReceiptValidator: secrets loaded");
            }

            if (_cachedJwt == null || DateTime.UtcNow >= _jwtExpiry)
            {
                logger?.LogInformation("AppleReceiptValidator: refreshing JWT");
                _cachedJwt = CreateJwt(_cachedKeyId, _cachedIssuerId, _cachedPrivateKey);
                _jwtExpiry = DateTime.UtcNow.AddMinutes(18);
            }

            var jwt = _cachedJwt;

            var tx = await FetchTransaction(jwt, transactionId, ProductionBase, logger)
                  ?? await FetchTransaction(jwt, transactionId, SandboxBase, logger);

            if (tx == null)
            {
                logger?.LogWarning("AppleReceiptValidator: no transaction found for {txId}", transactionId);
                return null;
            }

            logger?.LogInformation("Apple tx txId={txId} productId={product} bundleId={bundle} revoked={rev} originalTxId={orig}",
                tx.TransactionId, tx.ProductId, tx.BundleId, tx.RevocationReason, tx.OriginalTransactionId);

            bool valid = tx.RevocationReason == null
                      && tx.BundleId == BundleId
                      && tx.ProductId == productId;

            logger?.LogInformation("AppleReceiptValidator: valid={valid} (revoked={rev} bundleMatch={bundle} productMatch={product})",
                valid, tx.RevocationReason, tx.BundleId == BundleId, tx.ProductId == productId);

            return valid ? tx.OriginalTransactionId : null;
        }
        catch (Exception ex)
        {
            logger?.LogError("AppleReceiptValidator exception: {err}", ex.Message);
            return null;
        }
    }

    static string CreateJwt(string keyId, string issuerId, string privateKeyPem)
    {
        using var ecdsa = ECDsa.Create();
        ecdsa.ImportFromPem(privateKeyPem);

        var key = new ECDsaSecurityKey(ecdsa)
        {
            KeyId = keyId,
            CryptoProviderFactory = new CryptoProviderFactory { CacheSignatureProviders = false },
        };

        return new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = issuerId,
            Audience = "appstoreconnect-v1",
            Claims = new Dictionary<string, object> { ["bid"] = BundleId },
            IssuedAt = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddMinutes(20),
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.EcdsaSha256),
        });
    }

    static async Task<AppleTransaction> FetchTransaction(
        string jwt, string transactionId, string baseUrl, ILogger logger)
    {
        using var req = new HttpRequestMessage(HttpMethod.Get,
            $"{baseUrl}/inApps/v1/transactions/{transactionId}");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

        using var resp = await Http.SendAsync(req);
        var body = await resp.Content.ReadAsStringAsync();

        if (!resp.IsSuccessStatusCode)
        {
            logger?.LogWarning("Apple API {status} from {url}: {body}", (int)resp.StatusCode, baseUrl, body);
            return null;
        }

        var wrapper = JsonSerializer.Deserialize<TransactionWrapper>(body);
        return DecodeJwsPayload<AppleTransaction>(wrapper?.SignedTransactionInfo);
    }

    static T DecodeJwsPayload<T>(string jws) where T : class
    {
        if (string.IsNullOrEmpty(jws)) return null;
        var parts = jws.Split('.');
        if (parts.Length < 2) return null;
        var padding = (4 - parts[1].Length % 4) % 4;
        var base64 = parts[1].Replace('-', '+').Replace('_', '/') + new string('=', padding);
        var json = Encoding.UTF8.GetString(Convert.FromBase64String(base64));
        return JsonSerializer.Deserialize<T>(json);
    }

    class TransactionWrapper
    {
        [JsonPropertyName("signedTransactionInfo")] public string SignedTransactionInfo { get; set; }
    }
    class AppleTransaction
    {
        [JsonPropertyName("transactionId")] public string TransactionId { get; set; }
        [JsonPropertyName("originalTransactionId")] public string OriginalTransactionId { get; set; }
        [JsonPropertyName("bundleId")] public string BundleId { get; set; }
        [JsonPropertyName("productId")] public string ProductId { get; set; }
        [JsonPropertyName("revocationReason")] public int? RevocationReason { get; set; }
    }
}
