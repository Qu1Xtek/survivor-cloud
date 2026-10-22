using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;

public static class GoogleReceiptValidator
{
    const string PackageName = "com.cherryware.dropforge";
    const string TokenUrl = "https://oauth2.googleapis.com/token";
    const string Scope = "https://www.googleapis.com/auth/androidpublisher";

    static readonly HttpClient Http = new();
    static string _cachedSaJson;
    static string _cachedAccessToken;
    static DateTime _accessTokenExpiry = DateTime.MinValue;

    public static async Task<string> Verify(
        IGameApiClient client, IExecutionContext ctx,
        string productId, string purchaseToken, ILogger logger = null)
    {
        try
        {
            if (_cachedSaJson == null)
            {
                _cachedSaJson = (await client.SecretManager.GetSecret(ctx, "GOOGLE_SA_JSON")).Value;
            }

            var sa = JsonSerializer.Deserialize<ServiceAccount>(_cachedSaJson);
            var accessToken = await GetAccessToken(sa);

            var url = $"https://androidpublisher.googleapis.com/androidpublisher/v3/applications"
                    + $"/{PackageName}/purchases/products/{productId}/tokens/{purchaseToken}";

            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            using var resp = await Http.SendAsync(req);
            var body = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode)
            {
                logger?.LogWarning("Google API returned {status}: {body}", (int)resp.StatusCode, body);
                return null;
            }

            var purchase = JsonSerializer.Deserialize<GooglePurchase>(body);
            logger?.LogInformation("Google purchase state={state} consumptionState={consumed} orderId={order}",
                purchase?.PurchaseState, purchase?.ConsumptionState, purchase?.OrderId);

            bool valid = purchase?.PurchaseState == 0;
            return purchase?.OrderId;
        }
        catch (Exception ex)
        {
            logger?.LogError("GoogleReceiptValidator exception: {err}", ex.Message);
            return null;
        }
    }

    static async Task<string> GetAccessToken(ServiceAccount sa)
    {
        if (_cachedAccessToken != null && DateTime.UtcNow < _accessTokenExpiry)
        {
            return _cachedAccessToken;
        }

        using var rsa = RSA.Create();
        rsa.ImportFromPem(sa.PrivateKey);

        var rsaKey = new RsaSecurityKey(rsa)
        {
            CryptoProviderFactory = new CryptoProviderFactory { CacheSignatureProviders = false },
        };

        var jwt = new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = sa.ClientEmail,
            Audience = TokenUrl,
            Claims = new Dictionary<string, object> { ["scope"] = Scope },
            IssuedAt = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddMinutes(60),
            SigningCredentials = new SigningCredentials(rsaKey, SecurityAlgorithms.RsaSha256),
        });

        var form = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "urn:ietf:params:oauth:grant-type:jwt-bearer"),
            new KeyValuePair<string, string>("assertion", jwt),
        });

        using var resp = await Http.PostAsync(TokenUrl, form);
        var tokenResp = JsonSerializer.Deserialize<TokenResponse>(
            await resp.Content.ReadAsStringAsync());

        _cachedAccessToken = tokenResp!.AccessToken;
        _accessTokenExpiry = DateTime.UtcNow.AddMinutes(55);
        return _cachedAccessToken;
    }

    class ServiceAccount
    {
        [JsonPropertyName("client_email")] public string ClientEmail { get; set; }
        [JsonPropertyName("private_key")] public string PrivateKey { get; set; }
    }
    class TokenResponse
    {
        [JsonPropertyName("access_token")] public string AccessToken { get; set; }
    }
    class GooglePurchase
    {
        [JsonPropertyName("purchaseState")] public int? PurchaseState { get; set; }
        [JsonPropertyName("consumptionState")] public int? ConsumptionState { get; set; }
        [JsonPropertyName("orderId")] public string OrderId { get; set; }
    }
}