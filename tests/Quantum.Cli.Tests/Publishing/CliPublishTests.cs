using Quantum.Cli.Commands;
using Quantum.Cli.Packaging;
using System.Buffers.Text;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Quantum.Cli.Tests.Publishing;

public sealed class CliPublishTests
{
    private static readonly string[] ExistingReleaseMethods = ["ListPluginReleases"];

    [Theory]
    [InlineData(false, false, 0, false)]
    [InlineData(false, false, 0, true)]
    [InlineData(true, false, 0, false)]
    [InlineData(true, false, 0, true)]
    [InlineData(true, true, 1, false)]
    [InlineData(true, true, 1, true)]
    public async Task PublishUsesPrivateKeyUploadAndExistingRelease(bool existing, bool changed, int exitCode, bool sdk)
    {
        var root = Path.Combine(Path.GetTempPath(), "quantum-cli-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var source = Path.Combine(root, "plugin");
            Directory.CreateDirectory(source);
            await File.WriteAllTextAsync(Path.Combine(source, "plugin.json"), """{"id":"quantum.plugin.calendar","version":"1.1.1"}""");
            var keyFile = Path.Combine(root, "private.json");
            using var rsa = RSA.Create(2048);
            var key = rsa.ExportParameters(true);
            await File.WriteAllTextAsync(keyFile, JsonSerializer.Serialize(new { keys = new[] { new { kid = "test", kty = "RSA", n = Base64Url.EncodeToString(key.Modulus!), e = Base64Url.EncodeToString(key.Exponent!), d = Base64Url.EncodeToString(key.D!), p = Base64Url.EncodeToString(key.P!), q = Base64Url.EncodeToString(key.Q!), dp = Base64Url.EncodeToString(key.DP!), dq = Base64Url.EncodeToString(key.DQ!), qi = Base64Url.EncodeToString(key.InverseQ!) } } }));
            var archive = PluginArchive.Create(source);
            var sha = Convert.ToHexStringLower(SHA256.HashData(archive));
            var socket = new TcpListener(IPAddress.Loopback, 0);
            socket.Start();
            var port = ((IPEndPoint)socket.LocalEndpoint).Port;
            socket.Stop();
            using var server = new HttpListener();
            var url = $"http://127.0.0.1:{port}/";
            server.Prefixes.Add(url);
            server.Start();
            var requests = new List<string>();
            var serve = Task.Run(async () =>
            {
                var count = existing ? 2 : 4;
                for (var i = 0; i < count; i++)
                {
                    var context = await server.GetContextAsync().WaitAsync(TimeSpan.FromSeconds(15));
                    var body = await new StreamReader(context.Request.InputStream).ReadToEndAsync();
                    object response;
                    if (i == 0)
                    {
                        Assert.Contains("grant_type=client_credentials", body);
                        Assert.Contains("client_assertion=", body);
                        Assert.Contains("scope=plugin%3Apublish", body);
                        response = new { access_token = "test-token" };
                    }
                    else
                    {
                        Assert.Equal("Bearer test-token", context.Request.Headers["Authorization"]);
                        using var json = JsonDocument.Parse(body);
                        var method = json.RootElement.GetProperty("method").GetString()!;
                        requests.Add(method);
                        var release = new { releaseId = "42", version = "1.1.1", packageSha256 = changed ? "different" : sha, quantumVersionSupport = sdk ? "" : ">=0.1.0", sdkVersionSupport = sdk ? "0.3.0-rc.2" : null, status = 2, automatedReviewStatus = 3 };
                        if (method == "UploadPluginRelease")
                        {
                            var parameters = json.RootElement.GetProperty("params");
                            Assert.Equal(sha, parameters.GetProperty("expectedSha256").GetString());
                            Assert.Equal(sdk ? "0.3.0-rc.2" : ">=0.1.0", parameters.GetProperty(sdk ? "sdkVersionSupport" : "quantumVersionSupport").GetString());
                            Assert.False(parameters.TryGetProperty(sdk ? "quantumVersionSupport" : "sdkVersionSupport", out _));
                            Assert.Equal(archive, Convert.FromBase64String(parameters.GetProperty("packageArchiveBase64").GetString()!));
                            response = new { result = new { isSuccess = true, value = release with { status = 1, automatedReviewStatus = 1 } } };
                        }
                        else
                        {
                            response = new { result = new { isSuccess = true, value = existing || i == 3 ? new[] { release } : [] } };
                        }
                    }
                    var bytes = JsonSerializer.SerializeToUtf8Bytes(response);
                    context.Response.ContentType = "application/json";
                    await context.Response.OutputStream.WriteAsync(bytes);
                    context.Response.Close();
                }
            });
            var actual = await QuantumCli.RunAsync(["plugins", "publish", source, sdk ? "--sdk-version-support" : "--quantum-version-support", sdk ? "0.3.0-rc.2" : ">=0.1.0", "--base-url", url, "--client-id", "test-ci", "--client-private-jwks", keyFile, "--wait-seconds", "1", "--output", "subprocess"]);
            await serve;
            Assert.Equal(exitCode, actual);
            Assert.Equal(existing ? ExistingReleaseMethods : ["ListPluginReleases", "UploadPluginRelease", "ListPluginReleases"], requests);
        }
        finally { Directory.Delete(root, recursive: true); }
    }
}
