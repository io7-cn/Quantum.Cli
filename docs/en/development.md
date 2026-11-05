# Development

[简体中文](../zh-CN/development.md) · [Project guide](README.md)

Use .NET 10 selected by global.json, PowerShell 7 and Python 3.12 or later.

```sh
git submodule update --init --recursive
pwsh .ci/Verify.ps1
python .standards/.ci/check-adoption.py
python .ci/check-docs.py
```

The verification script uses locked dependency restore, runs `dotnet format`, builds and tests in Release, inspects the NuGet package, then installs the exact local package into an isolated tool directory and checks help and version output. Build outputs and temporary credentials are never committed.

| Directory | Responsibility |
| --- | --- |
| src/Quantum.Cli/Commands | Command dispatch, options, user interaction and orchestration |
| src/Quantum.Cli/Authentication | Publisher assertion signing using runtime cryptography and [base64url](glossary.md#base64url) support |
| src/Quantum.Cli/Packaging | Deterministic archives and root plugin manifests |
| src/Quantum.Cli/Publishing | Release state evaluation |
| src/Quantum.Cli/Transport | Authenticated platform [RPC](../../.standards/docs/glossary.md#rpc) requests |
| tests/Quantum.Cli.Tests | Tests mirroring source responsibilities; synthetic local integration |
| .ci | Build and verification helpers |

The shared EditorConfig defines naming, file layout and formatting. Do not override individual IDE diagnostic severities. Update lock files intentionally when changing dependencies and verify a clean restore. Add tests for affected protocol, signing and packaging behavior. Do not exercise live publication using production credentials during verification.
