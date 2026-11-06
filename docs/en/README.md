# Quantum CLI

[简体中文](../zh-CN/README.md)

Standalone [.NET CLI](../../.standards/docs/glossary.md#cli) for publisher credential setup and unattended plugin publication. Owned by the product engineering team of 艾欧柒（杭州）智能科技有限公司; maintainers also provide backup review and support through [repository issues](https://github.com/io7-cn/Quantum.Cli/issues). Licensed under [Apache 2.0](../../LICENSE).

Follow the [English development standards](../../.standards/docs/en/README.md) at the exact commit recorded by .standards. See [development](development.md), [release](releases.md), [security](security.md) and the [glossary](glossary.md).

## Install and use

Install a published NuGet tool version with .NET 10. The package remains `Quantum.Platform.Cli` and the executable remains `quantum-cli`.

```sh
dotnet tool install --global Quantum.Platform.Cli
quantum-cli --help
quantum-cli --version
quantum-cli plugins publish ./dist/plugin --wait-seconds 300 --output subprocess
```

SDK compatibility options require CLI 0.2.0 or later. If the installed NuGet release is older, use the verified local tool produced by [.ci/Verify.ps1](../../.ci/Verify.ps1) until a compatible package is published.

Publishing without a compatibility option requires CLI 0.3.0 and the platform's automatic SDK detection. Until 0.3.0 is published, use the verified local tool. Deploy the server change first; older servers cannot infer an omitted SDK version.

The source package version is defined in [Quantum.Cli.csproj](../../src/Quantum.Cli/Quantum.Cli.csproj). Source changes do not publish a package automatically.

Publish a folder containing root-level `plugin.json` or an existing ZIP archive. Folder packaging orders files deterministically, uses fixed ZIP timestamps and excludes .git, bin and obj directories. Publish only a prepared distribution folder; other files, including hidden files, are included. Existing versions are reused only if the content digest matches; explicitly supplied compatibility fields must also match. A conflicting version fails.

For .NET plugins, include `Quantum.Plugin.Abstraction.dll` at the package root beside the entry DLL. The platform reads its informational version without executing plugin code, checks the entry's SDK reference, and records the exact SDK version including prerelease identifiers. No compatibility option is needed. Missing or inconsistent assemblies fail validation. `--sdk-version-support` remains available for Web plugins to declare a tested SDK range, independently of Desktop versions. When explicitly used for .NET, it must equal the detected exact version. The legacy `--quantum-version-support` option remains available; the two options are mutually exclusive.

`--wait-seconds` accepts 0–3600 and defaults to zero: submit without waiting. Positive values poll until publication or timeout. Rejection, manual review, review failure and unknown states fail. Timeout does not withdraw an already submitted release; rerun the same package to check it. `--output text` is the default; `subprocess` writes one machine-readable success object to standard output. Errors and progress go to standard error; exit codes are 0 for success and 1 for handled failures.

## Publisher identity

Create a plugin-scoped [Credential Client](glossary.md#credential-client) interactively:

```sh
quantum-cli clients create --email developer@example.com --client-id sample-plugin-ci --display-name "Sample plugin CI" --plugins sample.plugin --private-jwks-output ./publisher-private.json
```

The password prompt does not echo input. Alternatively provide `QUANTUM_USER_TOKEN` to skip interactive login. The output path must not already exist. Protect the returned private [JWKS](glossary.md#jwks) outside Git and inject it into the publishing environment. The platform enforces allowed plugin identifiers. The CLI authenticates using a short-lived signed assertion; it never stores a user password for publication.

## Configuration

Command options override environment variables. All examples are fictional. A private key or token is a secret; URLs, identifiers, ranges and output modes are non-secret configuration.

| Variable | Type, requirement and default | Override and safe example | Secret |
| --- | --- | --- | --- |
| `QUANTUM_PLATFORM_URL` | Absolute platform URL; optional; default https://quantum.io-vii.com | `--base-url`; https://platform.example.com | No |
| `QUANTUM_CLIENT_ID` | Publisher identifier; required for publication; no default | `--client-id`; sample-plugin-ci | No |
| `QUANTUM_CLIENT_PRIVATE_JWKS` | Private key-set JSON; required unless a key file is supplied; no default | `--client-private-jwks FILE` takes precedence; use an externally injected test credential | Yes |
| `QUANTUM_USER_TOKEN` | User access token; optional for client setup; no default | No command-line token option; omit for interactive login | Yes |

`--private-jwks-output` is a required output filename for client setup. `--release-notes` defaults to empty text. `--client-id`, `--display-name` and `--plugins` are required for setup; `--email` is required only for interactive login. Use encrypted transport for real credentials. Local integration tests use synthetic keys and loopback HTTP endpoints.

## Repository boundaries

Only the .NET runtime libraries are needed by the CLI; test packages are isolated to tests. The CLI uses public platform network contracts and published plugin metadata. It does not reference Platform, Desktop, Mobile or SDK source. Protocol changes must remain compatible with deployed platforms or be explicitly versioned.
