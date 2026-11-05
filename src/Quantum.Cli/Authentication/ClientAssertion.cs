using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Quantum.Cli.Authentication;

internal static class ClientAssertion
{
    internal static string Create(string clientId, Uri audience, string privateJwks)
    {
        using var document = JsonDocument.Parse(privateJwks);
        var key = document.RootElement.GetProperty("keys")[0];
        var parameters = new RSAParameters
        {
            Modulus = Base64Url.DecodeFromChars(key.GetProperty("n").GetString()!),
            Exponent = Base64Url.DecodeFromChars(key.GetProperty("e").GetString()!),
            D = Base64Url.DecodeFromChars(key.GetProperty("d").GetString()!),
            P = Base64Url.DecodeFromChars(key.GetProperty("p").GetString()!),
            Q = Base64Url.DecodeFromChars(key.GetProperty("q").GetString()!),
            DP = Base64Url.DecodeFromChars(key.GetProperty("dp").GetString()!),
            DQ = Base64Url.DecodeFromChars(key.GetProperty("dq").GetString()!),
            InverseQ = Base64Url.DecodeFromChars(key.GetProperty("qi").GetString()!)
        };
        using var rsa = RSA.Create();
        rsa.ImportParameters(parameters);
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var header = Base64Url.EncodeToString(JsonSerializer.SerializeToUtf8Bytes(new { alg = "RS256", typ = "JWT", kid = key.GetProperty("kid").GetString() }));
        var payload = Base64Url.EncodeToString(JsonSerializer.SerializeToUtf8Bytes(new { iss = clientId, sub = clientId, aud = audience.AbsoluteUri, iat = now, exp = now + 120, jti = Guid.NewGuid().ToString("N") }));
        var input = $"{header}.{payload}";
        return $"{input}.{Base64Url.EncodeToString(rsa.SignData(Encoding.ASCII.GetBytes(input), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1))}";
    }
}
