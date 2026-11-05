using Quantum.Cli.Commands;
using System.IO.Compression;
using System.Text.Json;

namespace Quantum.Cli.Packaging;

internal static class PluginArchive
{
    internal static byte[] Create(string source)
    {
        var root = Path.GetFullPath(source);
        using var output = new MemoryStream();
        using (var zip = new ZipArchive(output, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var file in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
                .Where(file => !IsExcluded(Path.GetRelativePath(root, file)))
                .Order(StringComparer.Ordinal))
            {
                var relative = Path.GetRelativePath(root, file).Replace('\\', '/');
                var entry = zip.CreateEntry(relative, CompressionLevel.Optimal);
                entry.LastWriteTime = new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero);
                using var input = File.OpenRead(file);
                using var destination = entry.Open();
                input.CopyTo(destination);
            }
        }
        return output.ToArray();
    }

    private static bool IsExcluded(string relative)
    {
        var parts = relative.Replace('\\', '/').Split('/');
        return parts.Any(part => part is ".git" or "bin" or "obj");
    }

    internal static PluginManifest ReadManifest(byte[] archive)
    {
        using var stream = new MemoryStream(archive, writable: false);
        using var zip = new ZipArchive(stream, ZipArchiveMode.Read);
        var entry = zip.Entries.SingleOrDefault(item => item.FullName.Equals("plugin.json", StringComparison.OrdinalIgnoreCase))
            ?? throw new CliException("The package root must contain plugin.json.");
        using var manifestStream = entry.Open();
        var manifest = JsonSerializer.Deserialize<PluginManifest>(manifestStream, JsonSerializerOptions.Web);
        return !string.IsNullOrWhiteSpace(manifest?.Id) && !string.IsNullOrWhiteSpace(manifest.Version)
            ? manifest
            : throw new CliException("plugin.json must contain id and version.");
    }
}
