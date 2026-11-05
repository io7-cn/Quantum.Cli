using Quantum.Cli.Transport;
using System.Text;

namespace Quantum.Cli.Commands;

internal static partial class QuantumCli
{
    private static async Task<int> CreateClientAsync(string[] args)
    {
        var options = Parse(args);
        var clientId = Require(options, "client-id");
        var displayName = Require(options, "display-name");
        var plugins = Require(options, "plugins").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var outputPath = Path.GetFullPath(Require(options, "private-jwks-output"));
        if (File.Exists(outputPath) || Directory.Exists(outputPath))
        {
            throw new CliException("The private key output path must not already exist.");
        }
        var baseUrl = Get(options, "base-url") ?? Environment.GetEnvironmentVariable("QUANTUM_PLATFORM_URL") ?? DefaultBaseUrl;
        using var http = new HttpClient { BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/") };
        var userToken = Environment.GetEnvironmentVariable("QUANTUM_USER_TOKEN");
        if (string.IsNullOrWhiteSpace(userToken))
        {
            var email = Require(options, "email");
            var password = ReadPassword("Password: ");
            var login = await PlatformRpcClient.SendAsync(http, "Login", new { email, password }, null);
            userToken = login.GetProperty("accessToken").GetString()
                ?? throw new CliException("The platform did not return a user access token.");
        }

        var created = await PlatformRpcClient.SendAsync(
            http,
            "CreateCredentialClient",
            new { clientId, displayName, allowedPluginIds = plugins },
            userToken);
        var privateJwks = created.GetProperty("generatedKeySet").GetProperty("privateJsonWebKeySet").GetString()
            ?? throw new CliException("The platform did not return generated private JWKS.");
        var fileOptions = new FileStreamOptions
        {
            Mode = FileMode.CreateNew,
            Access = FileAccess.Write,
            Share = FileShare.None
        };
        if (!OperatingSystem.IsWindows())
        {
            fileOptions.UnixCreateMode = UnixFileMode.UserRead | UnixFileMode.UserWrite;
        }
        await using (var stream = new FileStream(outputPath, fileOptions))
        await using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
        {
            await writer.WriteAsync(privateJwks);
        }
        Console.WriteLine($"Created Credential Client '{clientId}'. Private JWKS was written once to {outputPath}.");
        return 0;
    }

    private static string ReadPassword(string prompt)
    {
        if (Console.IsInputRedirected)
        {
            throw new CliException("Interactive password input requires a terminal. Alternatively set QUANTUM_USER_TOKEN.");
        }

        Console.Error.Write(prompt);
        var value = new StringBuilder();
        while (Console.ReadKey(intercept: true) is { } key && key.Key != ConsoleKey.Enter)
        {
            if (key.Key == ConsoleKey.Backspace && value.Length > 0)
            {
                value.Length--;
            }
            else if (!char.IsControl(key.KeyChar))
            {
                value.Append(key.KeyChar);
            }
        }
        Console.Error.WriteLine();
        return value.Length > 0 ? value.ToString() : throw new CliException("Password cannot be empty.");
    }
}
