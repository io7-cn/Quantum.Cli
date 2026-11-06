# Releases

[简体中文](../zh-CN/releases.md) · [Project guide](README.md)

The independently maintained package keeps the NuGet identifier `Quantum.Platform.Cli` and command `quantum-cli`. The source project's Version property is the version source of truth. Preserve previously published versions and tags; publish changes under a new version. Release notes must describe compatibility, fixes and upgrade requirements.

[Verify](../../.github/workflows/verify.yml) runs on main updates and change requests on Linux, Windows and macOS, and uploads inspected packages. [Publish](../../.github/workflows/publish.yml) runs verification again for `vX.Y.Z` or prerelease tags. The tag must match the project version, be annotated and reference verified main history. The release job downloads the already verified Linux package and verifies its SHA-256 manifest before publication.

NuGet publication uses short-lived [OIDC](../../.standards/docs/glossary.md#oidc) trusted publishing through NuGet/login. Before the first publication, the package owner must configure the NuGet trusted-publisher policy for io7-cn/Quantum.Cli, workflow publish.yml and environment release; set repository variable `NUGET_USER` to the authorized NuGet account. This non-secret string has no default. The release environment should require an owner review. No long-lived API key is stored in the repository. Missing setup fails publication.

Repository creation and ordinary CI do not publish a new package. Do not move old Platform cli-v tags or reuse a published version when changing the release source.

## 0.3.0 candidate — 2026-11-06

[Tracking issue #2](https://github.com/io7-cn/Quantum.Cli/issues/2). Publication can omit compatibility options for managed plugins; the compatible platform derives the exact SDK version from the uploaded assemblies. Explicit SDK and legacy host options remain available. Retries without an option compare the immutable package digest; explicitly supplied compatibility values must also match. Web plugins still require an explicit tested SDK range.

Deploy the compatible platform before publishing this CLI. No configuration or data migration is needed. Packages must include the SDK DLL at the root. An older CLI requiring the SDK option must pass the detected exact version. Validate the candidate on Linux, Windows and macOS, publish through the signed annotated `v0.3.0` tag on reviewed main history, and verify the published package by isolated installation. Until publication, use the verified local tool. If package or install verification fails, stop promotion and use an existing immutable CLI package with its supported explicit compatibility option; never replace a published package in place. This release adds no dependency or credential-handling changes. Source revision, CI, immutable package and install evidence belong in the linked release/issue record.
