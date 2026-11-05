using Quantum.Cli.Authentication;
using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Quantum.Cli.Tests.Authentication;

public sealed class ClientAssertionTests
{
    [Fact]
    public void AssertionAuthenticatesTheClientToTheExactTokenEndpoint()
    {
        using var rsa = RSA.Create(2048);
        var key = rsa.ExportParameters(true);
        var keys = JsonSerializer.Serialize(new
        {
            keys = new[] { new
        {
            kid = "synthetic-key", n = Base64Url.EncodeToString(key.Modulus!), e = Base64Url.EncodeToString(key.Exponent!),
            d = Base64Url.EncodeToString(key.D!), p = Base64Url.EncodeToString(key.P!), q = Base64Url.EncodeToString(key.Q!),
            dp = Base64Url.EncodeToString(key.DP!), dq = Base64Url.EncodeToString(key.DQ!), qi = Base64Url.EncodeToString(key.InverseQ!)
        } }
        });
        var endpoint = new Uri("https://platform.example.com/oauth2/token");
        var before = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var assertion = ClientAssertion.Create("sample-client", endpoint, keys);
        var parts = assertion.Split('.');
        Assert.Equal(3, parts.Length);
        using var header = JsonDocument.Parse(Base64Url.DecodeFromChars(parts[0]));
        using var payload = JsonDocument.Parse(Base64Url.DecodeFromChars(parts[1]));
        Assert.Equal("RS256", header.RootElement.GetProperty("alg").GetString());
        Assert.Equal("synthetic-key", header.RootElement.GetProperty("kid").GetString());
        Assert.Equal("sample-client", payload.RootElement.GetProperty("iss").GetString());
        Assert.Equal("sample-client", payload.RootElement.GetProperty("sub").GetString());
        Assert.Equal(endpoint.AbsoluteUri, payload.RootElement.GetProperty("aud").GetString());
        var issued = payload.RootElement.GetProperty("iat").GetInt64();
        Assert.InRange(issued, before, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        Assert.Equal(120, payload.RootElement.GetProperty("exp").GetInt64() - issued);
        Assert.False(string.IsNullOrWhiteSpace(payload.RootElement.GetProperty("jti").GetString()));
        Assert.True(rsa.VerifyData(Encoding.ASCII.GetBytes(parts[0] + "." + parts[1]), Base64Url.DecodeFromChars(parts[2]), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1));
    }
}
