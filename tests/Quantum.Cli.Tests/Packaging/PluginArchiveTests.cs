using Quantum.Cli.Packaging;
using System.IO.Compression;

namespace Quantum.Cli.Tests.Packaging;

public sealed class PluginArchiveTests
{
    private static readonly string[] ExpectedEntries = ["content/data.txt", "plugin.json"];

    [Fact]
    public void FolderPackagesRemainDeterministicAndExcludeBuildAndGitFiles()
    {
        var root = Path.Combine(Path.GetTempPath(), "quantum-package-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            File.WriteAllText(Path.Combine(root, "plugin.json"), """{"id":"sample.plugin","version":"1.0.0"}""");
            foreach (var directory in new[] { ".git", "bin", "obj", "content" })
            {
                Directory.CreateDirectory(Path.Combine(root, directory));
                File.WriteAllText(Path.Combine(root, directory, "data.txt"), directory);
            }
            var first = PluginArchive.Create(root);
            File.SetLastWriteTimeUtc(Path.Combine(root, "plugin.json"), DateTime.UtcNow.AddYears(-1));
            Assert.Equal(first, PluginArchive.Create(root));
            using var archive = new ZipArchive(new MemoryStream(first));
            Assert.Equal(ExpectedEntries, archive.Entries.Select(entry => entry.FullName));
            var manifest = PluginArchive.ReadManifest(first);
            Assert.Equal("sample.plugin", manifest.Id);
            Assert.Equal("1.0.0", manifest.Version);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }
}
