using Quantum.Cli.Authentication;
using Quantum.Cli.Packaging;
using Quantum.Cli.Publishing;
using Quantum.Cli.Transport;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Quantum.Cli.Commands;

internal static partial class QuantumCli
{
    private static async Task<int> PublishAsync(string[] args)
    {
        var options = Parse(args);
        var source = Require(options, "source");
        var sdkSupport = Get(options, "sdk-version-support");
        var legacySupport = Get(options, "quantum-version-support");
        if (sdkSupport is not null && legacySupport is not null)
        {
            throw new CliException("Use --sdk-version-support alone; host and SDK ranges are different contracts.");
        }
        var support = sdkSupport ?? legacySupport;
        var supportProperty = sdkSupport is not null ? "sdkVersionSupport" : "quantumVersionSupport";
        var output = Get(options, "output") ?? "text";
        if (output is not ("text" or "subprocess"))
        {
            throw new CliException("--output must be 'text' or 'subprocess'.");
        }

        var waitText = Get(options, "wait-seconds") ?? "0";
        if (!int.TryParse(waitText, out var waitSeconds) || waitSeconds is < 0 or > 3600)
        {
            throw new CliException("--wait-seconds must be an integer between 0 and 3600.");
        }

        var baseUrl = Get(options, "base-url") ?? Environment.GetEnvironmentVariable("QUANTUM_PLATFORM_URL") ?? DefaultBaseUrl;
        var clientId = Get(options, "client-id") ?? Environment.GetEnvironmentVariable("QUANTUM_CLIENT_ID");
        if (string.IsNullOrWhiteSpace(clientId))
        {
            throw new CliException("Set QUANTUM_CLIENT_ID or pass --client-id.");
        }

        var privateJwks = LoadPrivateJwks(options);
        var archive = Directory.Exists(source) ? PluginArchive.Create(source) : await File.ReadAllBytesAsync(source);
        var manifest = PluginArchive.ReadManifest(archive);
        var sha256 = Convert.ToHexStringLower(SHA256.HashData(archive));
        using var http = new HttpClient { BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/") };
        var tokenEndpoint = new Uri(http.BaseAddress, "oauth2/token");
        var assertion = ClientAssertion.Create(clientId, tokenEndpoint, privateJwks);
        using var tokenResponse = await http.PostAsync(tokenEndpoint, new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = clientId,
            ["client_assertion_type"] = "urn:ietf:params:oauth:client-assertion-type:jwt-bearer",
            ["client_assertion"] = assertion,
            ["scope"] = "plugin:publish"
        }));
        var tokenPayload = await tokenResponse.Content.ReadAsStringAsync();
        var token = JsonSerializer.Deserialize<TokenResponse>(tokenPayload, JsonSerializerOptions.Web);
        if (!tokenResponse.IsSuccessStatusCode || string.IsNullOrWhiteSpace(token?.AccessToken))
        {
            throw new CliException(token?.ErrorDescription ?? token?.Error ?? $"Token request failed with HTTP {(int)tokenResponse.StatusCode}.");
        }

        var existing = await FindReleaseAsync(http, token.AccessToken, manifest);
        JsonElement release;
        if (existing is { } found)
        {
            if (found.GetProperty("packageSha256").GetString() != sha256 ||
                (support is not null &&
                    (!found.TryGetProperty(supportProperty, out var existingSupport) || existingSupport.GetString() != support)))
            {
                throw new CliException("This version already exists with a different package or compatibility range. Bump plugin.json version.");
            }
            release = found;
        }
        else
        {
            var parameters = new Dictionary<string, object?>
            {
                ["pluginId"] = manifest.Id,
                ["version"] = manifest.Version,
                ["releaseNotes"] = Get(options, "release-notes") ?? string.Empty,
                ["packageArchiveBase64"] = Convert.ToBase64String(archive),
                ["expectedSha256"] = sha256
            };
            if (support is not null)
            {
                parameters[supportProperty] = support;
            }
            release = await PlatformRpcClient.SendAsync(http, "UploadPluginRelease", parameters, token.AccessToken);
        }

        var deadline = DateTimeOffset.UtcNow.AddSeconds(waitSeconds);
        while (true)
        {
            var state = ReleasePublication.Evaluate(release);
            if (state == "Published" || waitSeconds == 0)
            {
                break;
            }

            if (DateTimeOffset.UtcNow >= deadline)
            {
                throw new CliException($"Timed out waiting for {manifest.Id}@{manifest.Version} to publish. The release remains submitted; rerun to check it.");
            }

            Console.Error.WriteLine($"Waiting for automated review: {manifest.Id}@{manifest.Version} ({state}).");
            await Task.Delay(TimeSpan.FromSeconds(Math.Clamp((deadline - DateTimeOffset.UtcNow).TotalSeconds, 0, 10)));
            release = await FindReleaseAsync(http, token.AccessToken, manifest)
                ?? throw new CliException("The submitted release is no longer visible to this client.");
        }

        var status = ReleasePublication.Evaluate(release);
        if (output == "subprocess")
        {
            Console.WriteLine(JsonSerializer.Serialize(new { ok = true, pluginId = manifest.Id, version = manifest.Version, sha256, status, releaseId = release.GetProperty("releaseId").GetString() }, JsonSerializerOptions.Web));
        }
        else if (output == "text")
        {
            Console.WriteLine($"{status}: {manifest.Id}@{manifest.Version} ({sha256}).");
        }
        else
        {
            throw new CliException("--output must be 'text' or 'subprocess'.");
        }

        return 0;
    }

    private static async Task<JsonElement?> FindReleaseAsync(HttpClient http, string token, PluginManifest manifest)
    {
        var releases = await PlatformRpcClient.SendAsync(http, "ListPluginReleases", new { pluginId = manifest.Id }, token);
        foreach (var release in releases.EnumerateArray())
        {
            if (release.GetProperty("version").GetString() == manifest.Version)
            {
                return release.Clone();
            }
        }

        return null;
    }

    private static string LoadPrivateJwks(IReadOnlyDictionary<string, string> options)
    {
        var path = Get(options, "client-private-jwks");
        if (!string.IsNullOrWhiteSpace(path))
        {
            return File.ReadAllText(path);
        }

        var value = Environment.GetEnvironmentVariable("QUANTUM_CLIENT_PRIVATE_JWKS");
        return !string.IsNullOrWhiteSpace(value)
            ? value
            : throw new CliException("Set QUANTUM_CLIENT_PRIVATE_JWKS or pass --client-private-jwks FILE. Never pass private key material as an argument.");
    }

    private sealed record TokenResponse(
        [property: JsonPropertyName("access_token")] string? AccessToken,
        [property: JsonPropertyName("error")] string? Error,
        [property: JsonPropertyName("error_description")] string? ErrorDescription);
}
