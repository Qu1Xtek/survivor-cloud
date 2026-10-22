using System;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;


public class PushNotifier
{
    private const string ServiceAccountEmail = "firebase-adminsdk-fbsvc@survivorforge-a1fa4.iam.gserviceaccount.com";
    private const string ProjectId = "survivorforge-a1fa4";
    private const string PrivateKey = "-----BEGIN PRIVATE KEY-----\nMIIEvQIBADANBgkqhkiG9w0BAQEFAASCBKcwggSjAgEAAoIBAQClg4i5LeUz7oRD\nDkSPLKOL4l1RpbAAu1EedB0rxUORrGVpZ13YzfQfPYCd6LzXpxM+wYRDZfiaHC/V\n0ABRpjNT9avEE6H7j46BoVh2xkqx0JnfqwRaE5dgNYeIL2MUtegGUDqC3ejJ31Jp\ntZmpAcsSXlaWQ4ugNL3RquFnsSxyf6Dqtj8VyDopo/WiBADYSTThORmh8VuVD7NM\nBXReapbjYyxIk0Ifnt8D5jveFRNoJjz6DB4Ubnb1uQfuOytOjG4BxDSyf0J6S5s8\nojFzjmZ2/coOxdOMwAv01YrcO856KikY0Ld6gsMCgsexsFmlMlDab5Jzs05/ujyD\n/yciBlS3AgMBAAECggEAQZWyarZt+TsNO8j+j8E/eSGUmtLNwCx5g1i9ame7U5D9\nilvESzGpB2wGeAwOMGHq9VYXQ+G3NdHvfojyZ2g1NJLHomWpLRaAgItfk4kVdgpT\nvtZ0XgEfXac4clVcrIs8Sqwl0099e+i7LlogbmDgiAHLy7UuMahUgy10LfN9ykx2\ne8qeMqHRAJLDCmzzu5Ew+kEdLrih1wPbXzVFpGmpmK0Lx95+uBGzNoV3Ei6gDfzv\nxSv7k6MM0ZpqwKzEhoYTs26BnOpZ6HQrhIA6gW6fHgBUsCXtMzssQly++GlKAEz9\nqZISQxjoHe/NJjQnl7QhBrUr7mwJlqf77plWCKxhKQKBgQDkuoVplB3CfxtzIuin\nMf1Gdjim4+R62raoteXhKRLM2b1judI4PTD8zZnlTnTC+qaOdcCcOcQ8lAPoT4+2\nqzYkL+aGWe7qP/80cG3iKLiETUaSS/pDYRipzWsJXF2SZSSSLivz37K16XHV5yFD\n3+jLB67p+GoWE8fOIDkwSMa1ywKBgQC5P4JKPctHNe+tMvlq72xeSn1N3ZZrcSfl\nV7TiZmmfQEOQiTYQ3kFVPeljcuS3QmZrE+hKNcP7dknbKmP+YTRLy9SzNiuChSOP\nfo5clYmWcrWGe0ASAelqrQAgOt67vv3wrVmKP/DbLdPnSuLcC00BWC1qBBquXVyL\nuJuZPONfRQKBgCAa6kUSjEbTrpevq7ZkNufgCj2quVRhRMhTl+vN5xyn54SK/a31\nMN4bQeHE7HC93AkdECcdmKTkJdRC7qo2SAg2TQZIs1dgGkNTEKlxC81OR3ybe4pf\no6WZ9s8+jsfQuJxoLLUzjhMorFjY83tif3uYHX4r50AoEKCnNz/Gx1WXAoGAFVaQ\nbnbRvIEk+4Fco4mypOXVqyt0ESo2mz9QEagUUIi5f6Oee7DuO6BnCtKIL+fRHlJr\ntmidC5EjjsBCRmHyAS0YchAyUNV/NB8yzM1YzOCnsjbuOWTI+0W0NwIw99VXMOr2\n3mrhFo4f04uXuBsBIr+Cc6zHWgBdEacRHUSbODUCgYEAptcEDaoAbnRsHa2DLJUT\nHvL64l4g4xVUcNzFEUvU2J1DX3VgiqMb86/GEFuzvF9DuKtQZukkcp7QU1Keq5F2\nIFb/2FBNPNiqPTmT8kZ+B4du3DOhdtRiyDVDZRNvLVltYaS73fe0qOhLfOR49gI3\n8MdyTrQivEPYlb6GOz1RIEc=\n-----END PRIVATE KEY-----\n";

    private static readonly HttpClient client = new HttpClient();

    public async Task<string> SendNotificationAsync(string deviceToken, string title, string body, Dictionary<string, string> data)
    {
        var jwt = CreateJwtToken();
        var accessToken = await GetAccessTokenAsync(jwt);

        var payload = new
        {
            message = new
            {
                token = deviceToken,
                notification = new { title = title, body = body },
                data
            }
        };

        var request = new HttpRequestMessage(HttpMethod.Post,
            $"https://fcm.googleapis.com/v1/projects/{ProjectId}/messages:send")
        {
            Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(payload),
                                        Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await client.SendAsync(request);
        return await response.Content.ReadAsStringAsync();
    }

    // Sends a single message to an FCM topic (e.g. "all"), which fans out to every
    // device subscribed to that topic. Used for mass/broadcast notifications.
    public async Task<string> SendNotificationToTopicAsync(string topic, string title, string body, Dictionary<string, string> data)
    {
        var jwt = CreateJwtToken();
        var accessToken = await GetAccessTokenAsync(jwt);

        var payload = new
        {
            message = new
            {
                topic = topic,
                notification = new { title = title, body = body },
                data
            }
        };

        var request = new HttpRequestMessage(HttpMethod.Post,
            $"https://fcm.googleapis.com/v1/projects/{ProjectId}/messages:send")
        {
            Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(payload),
                                        Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await client.SendAsync(request);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"FCM topic send failed ({(int)response.StatusCode}): {responseBody}");
        }

        return responseBody;
    }

    private string CreateJwtToken()
    {
        var now = DateTimeOffset.UtcNow;
        var iat = now.ToUnixTimeSeconds();

        var securityKey = new RsaSecurityKey(RSAUtilities.CreateFromPrivateKeyPem(PrivateKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.RsaSha256);

        var claims = new List<Claim>
    {
        new Claim("scope", "https://www.googleapis.com/auth/firebase.messaging"),
        new Claim("iat", iat.ToString(), ClaimValueTypes.Integer64)
    };

        var token = new JwtSecurityToken(
            issuer: ServiceAccountEmail,
            audience: "https://oauth2.googleapis.com/token",
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: now.AddMinutes(60).UtcDateTime,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<string> GetAccessTokenAsync(string jwt)
    {
        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "urn:ietf:params:oauth:grant-type:jwt-bearer"),
            new KeyValuePair<string, string>("assertion", jwt),
        });

        var response = await client.PostAsync("https://oauth2.googleapis.com/token", content);
        var result = await response.Content.ReadAsStringAsync();
        using var doc = System.Text.Json.JsonDocument.Parse(result);
        return doc.RootElement.GetProperty("access_token").GetString();
    }
}

