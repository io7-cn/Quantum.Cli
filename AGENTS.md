# Quantum CLI instructions

- Follow the [English development standards](.standards/docs/en/README.md) at the recorded commit and the [English project documentation](docs/en/README.md).
- Follow the [Git branching standards](.standards/docs/en/02-development/git-branching.md) before editing: inspect the branch and working tree, refresh the approved target and create or reuse a compliant topic branch without discarding unrelated work. Do not edit, commit or push directly on protected branches; merge through a change request with required checks and approvals. Administrator access and passing tests do not waive this workflow.
- Keep temporary and one-off scripts/data, ad hoc acceptance checks, individual-task evidence, per-run histories and temporary investigation/handoff notes out of version control under [MNT-09](.standards/docs/en/05-maintenance/maintenance-standards.md#keep-acceptance-records-outside-source-repositories). Hidden directories do not exempt these files. Store retained evidence externally; version only maintained project assets with a lasting purpose, ownership and appropriate review/verification.
- Use the contributor-approved Git identity for author and committer.
- Maintain paired English and Simplified Chinese documents under docs/en and docs/zh-CN with reciprocal links. Keep this file English only.
- Group source by technical responsibility and mirror the source directories in tests. Keep command parsing separate from authentication, packaging, publication state and transport.
- Preserve the published package identity Quantum.Platform.Cli and the quantum-cli command. Do not add sibling source references or server implementation dependencies.
- Use the repository EditorConfig, including braces, var preferences, PascalCase static fields and expression bodies for single-line methods. Explain engineering terminology through the project or shared glossary.
- Never commit private keys, tokens, workstation paths or local development records. Use synthetic data and loopback test endpoints.
- Run .ci/Verify.ps1 for locked restore, formatting, tests, package inspection and installed-tool smoke checks. Run the standards adoption and documentation checks before handoff.
- On this Windows machine, every command invoking dotnet must run with elevated permissions.
