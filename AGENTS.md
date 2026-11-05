# Quantum CLI instructions

- Follow the [English development standards](.standards/docs/en/README.md) at the recorded commit and the [English project documentation](docs/en/README.md).
- Use the contributor-approved Git identity for author and committer.
- Maintain paired English and Simplified Chinese documents under docs/en and docs/zh-CN with reciprocal links. Keep this file English only.
- Group source by technical responsibility and mirror the source directories in tests. Keep command parsing separate from authentication, packaging, publication state and transport.
- Preserve the published package identity Quantum.Platform.Cli and the quantum-cli command. Do not add sibling source references or server implementation dependencies.
- Use the repository EditorConfig, including braces, var preferences, PascalCase static fields and expression bodies for single-line methods. Explain engineering terminology through the project or shared glossary.
- Never commit private keys, tokens, workstation paths or local development records. Use synthetic data and loopback test endpoints.
- Run .ci/Verify.ps1 for locked restore, formatting, tests, package inspection and installed-tool smoke checks. Run the standards adoption and documentation checks before handoff.
- On this Windows machine, every command invoking dotnet must run with elevated permissions.
