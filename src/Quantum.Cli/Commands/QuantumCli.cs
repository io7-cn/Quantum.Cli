using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;

namespace Quantum.Cli.Commands;

internal static partial class QuantumCli
{
    private const string DefaultBaseUrl = "https://quantum.io-vii.com";

    public static async Task<int> RunAsync(string[] args)
    {
        try
        {
            if (args.Length == 0 || args[0] is "help" or "--help" or "-h")
            {
                PrintHelp();
                return 0;
            }

            if (args[0] is "--version" or "version")
            {
                Console.WriteLine(typeof(QuantumCli).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion.Split('+')[0]
                    ?? typeof(QuantumCli).Assembly.GetName().Version?.ToString(3) ?? "unknown");
                return 0;
            }

            if (args is ["plugins", "publish", .. var rest])
            {
                return await PublishAsync(rest);
            }

            if (args is ["clients", "create", .. var clientArgs])
            {
                return await CreateClientAsync(clientArgs);
            }

            throw new CliException("Unknown command. Run 'quantum-cli --help'.");
        }
        catch (Exception exception) when (exception is CliException or HttpRequestException or IOException or JsonException or CryptographicException)
        {
            Console.Error.WriteLine(exception.Message);
            return 1;
        }
    }

    private static Dictionary<string, string> Parse(string[] args)
    {
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        for (var index = 0; index < args.Length; index++)
        {
            if (!args[index].StartsWith("--", StringComparison.Ordinal))
            {
                if (!values.TryAdd("source", args[index]))
                {
                    throw new CliException("Only one package path may be supplied.");
                }

                continue;
            }

            var name = args[index][2..];
            if (++index >= args.Length)
            {
                throw new CliException($"Missing value for --{name}.");
            }

            values[name] = args[index];
        }

        return values;
    }

    private static string Require(IReadOnlyDictionary<string, string> values, string key) => Get(values, key) ?? throw new CliException($"Missing required {(key == "source" ? "package path" : $"--{key}")}.");
    private static string? Get(IReadOnlyDictionary<string, string> values, string key) => values.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value) ? value : null;

    private static void PrintHelp() => Console.WriteLine("""
        Quantum Platform CLI

        quantum-cli clients create --email <email> --client-id <id> --display-name <name> --plugins <id,id> --private-jwks-output <file>
        quantum-cli plugins publish <folder-or-zip> [options]

        Options:
          --sdk-version-support <range>    Web plugins; .NET SDK version is detected by the platform
          --release-notes <text>
          --base-url <url>                 or QUANTUM_PLATFORM_URL
          --client-id <id>                 or QUANTUM_CLIENT_ID
          --client-private-jwks <file>      or QUANTUM_CLIENT_PRIVATE_JWKS
          --output text|subprocess
          --wait-seconds <0-3600>           wait for Published; default 0 only submits

        Client setup prompts for the password without echoing it; QUANTUM_USER_TOKEN can be used instead.
        """);
}
