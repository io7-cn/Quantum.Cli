# Releases

[简体中文](../zh-CN/releases.md) · [Project guide](README.md)

The independently maintained package keeps the NuGet identifier `Quantum.Platform.Cli` and command `quantum-cli`. The source project's Version property is the version source of truth. Preserve previously published versions and tags; publish changes under a new version. Release notes must describe compatibility, fixes and upgrade requirements.

[Verify](../../.github/workflows/verify.yml) runs on main updates and change requests on Linux, Windows and macOS, and uploads inspected packages. [Publish](../../.github/workflows/publish.yml) runs verification again for `vX.Y.Z` or prerelease tags. The tag must match the project version, be annotated and reference verified main history. The release job downloads the already verified Linux package and verifies its SHA-256 manifest before publication.

NuGet publication uses short-lived [OIDC](../../.standards/docs/glossary.md#oidc) trusted publishing through NuGet/login. Before the first publication, the package owner must configure the NuGet trusted-publisher policy for io7-cn/Quantum.Cli, workflow publish.yml and environment release; set repository variable `NUGET_USER` to the authorized NuGet account. This non-secret string has no default. The release environment should require an owner review. No long-lived API key is stored in the repository. Missing setup fails publication.

Repository creation and ordinary CI do not publish a new package. Do not move old Platform cli-v tags or reuse a published version when changing the release source.
