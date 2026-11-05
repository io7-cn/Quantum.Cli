using Quantum.Cli.Commands;

namespace Quantum.Cli.Tests.Commands;

public sealed class QuantumCliClientTests
{
    [Fact]
    public async Task ExistingPrivateKeyOutputIsRejectedBeforeContactingThePlatform()
    {
        var path = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(path, "existing synthetic credential");
            var exit = await QuantumCli.RunAsync(["clients", "create", "--client-id", "sample-client", "--display-name", "Sample",
                "--plugins", "sample.plugin", "--private-jwks-output", path, "--base-url", "https://must-not-contact.invalid"]);
            Assert.Equal(1, exit);
            Assert.Equal("existing synthetic credential", await File.ReadAllTextAsync(path));
        }
        finally
        {
            File.Delete(path);
        }
    }
}
