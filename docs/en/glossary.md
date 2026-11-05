# Project glossary

[简体中文](../zh-CN/glossary.md) · [Shared glossary](../../.standards/docs/glossary.md)

<a id="credential-client"></a>
## Credential Client

A platform-issued publication identity restricted to selected plugin identifiers. The platform retains its public keys; the publisher protects the private keys and signs short-lived client assertions.

<a id="jwks"></a>
## JWKS

JSON Web Key Set, a JSON document containing cryptographic keys. Public JWKS supports signature verification; private JWKS contains signing secrets and must never be published. The signed client assertion is a [JWT](../../.standards/docs/glossary.md#jwt).

<a id="base64url"></a>
## Base64url

The URL-safe base64 alphabet used in key parameters and signed assertions, without trailing padding. It is an encoding, not encryption.
