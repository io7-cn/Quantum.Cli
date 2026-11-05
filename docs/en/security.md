# Security

[简体中文](../zh-CN/security.md) · [Project guide](README.md)

Never include private signing keys, user tokens or secrets in Git, plugin distributions or ordinary logs. Inject publisher credentials from the approved secret store and restrict each client to the required plugin identifiers. A private key file takes precedence over the key environment variable. Keep generated private JWKS in protected storage outside the checkout and revoke the publisher client if exposed.

The package is a client of the configured platform, which performs authorization and review. Use encrypted connections for real credentials. Verify the platform address before using a credential. The default .NET HTTP certificate validation remains enabled.

Report suspected vulnerabilities privately to the repository maintainers through [GitHub private reporting](https://github.com/io7-cn/Quantum.Cli/security/advisories/new). Do not include live credentials or sensitive plugin archives in a public issue. Supply a synthetic reproduction, affected version and expected impact. Maintainers coordinate remediation and disclosure.
