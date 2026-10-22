using System;
using System.Security.Cryptography;
using System.Text;

public static class RSAUtilities
{
    public static RSA CreateFromPrivateKeyPem(string pem)
    {
        var base64 = pem.Replace("-----BEGIN PRIVATE KEY-----", "")
                        .Replace("-----END PRIVATE KEY-----", "")
                        .Replace("\n", "")
                        .Replace("\r", "");

        var privateKeyBytes = Convert.FromBase64String(base64);
        var rsa = RSA.Create();
        rsa.ImportPkcs8PrivateKey(privateKeyBytes, out _);
        return rsa;
    }
}


