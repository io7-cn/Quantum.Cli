# 项目术语表

[English](../en/glossary.md) · [共享术语表](../../.standards/docs/glossary.md)

<a id="credential-client"></a>
## Credential Client

凭据客户端，平台签发且仅允许发布指定插件的身份。平台保留公钥，发布者保护私钥并签署短时有效的客户端断言。

<a id="jwks"></a>
## JWKS

JSON Web Key Set，JSON Web 密钥集，以 JSON 文档表示密码学密钥。公钥集用于验签，私钥集包含签名秘密，绝不能公开。已签名的客户端断言为 [JWT](../../.standards/docs/glossary.md#jwt)。

<a id="base64url"></a>
## Base64url

URL 安全的 base64 字母表，用于密钥参数与签名断言，省略末尾填充。它属于编码，不是加密。
