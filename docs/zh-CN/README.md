# Quantum CLI

[English](../en/README.md)

独立的 [.NET 命令行工具](../../.standards/docs/glossary.md#cli)，用于创建发布身份和无人值守发布插件。由艾欧柒（杭州）智能科技有限公司产品研发团队维护；维护者通过[仓库议题](https://github.com/io7-cn/Quantum.Cli/issues)提供备用审核与支持。采用 [Apache 2.0](../../LICENSE) 许可。

遵循 .standards 固定提交对应的[简体中文开发规范](../../.standards/docs/zh-CN/README.md)。参见[开发](development.md)、[发行](releases.md)、[安全](security.md)和[术语表](glossary.md)。

## 安装与使用

使用 .NET 10 安装已发布的 NuGet 工具版本。包名保持 `Quantum.Platform.Cli`，可执行命令保持 `quantum-cli`。

```sh
dotnet tool install --global Quantum.Platform.Cli
quantum-cli --help
quantum-cli --version
quantum-cli plugins publish ./dist/plugin --sdk-version-support "0.3.0-rc.2" --wait-seconds 300 --output subprocess
```

SDK 兼容选项要求 CLI 0.2.0 或更高版本。若安装的 NuGet 发行版较旧，请使用 [.ci/Verify.ps1](../../.ci/Verify.ps1) 生成并验证的本地工具，直至兼容版本发布。

源码包版本由 [Quantum.Cli.csproj](../../src/Quantum.Cli/Quantum.Cli.csproj) 定义，源码变更不会自动发布包。

发布包含根级 `plugin.json` 的文件夹，或已有 ZIP 归档。文件夹打包按固定顺序处理文件，使用固定 ZIP 时间戳，并排除 .git、bin 和 obj 目录。仅发布已准备好的分发目录；其余文件（包括隐藏文件）都会被包含。只有内容摘要和兼容范围均一致时才复用已有版本，冲突版本会失败。`--sdk-version-support` 描述插件 SDK 兼容范围，与 Desktop 版本独立。保留旧的 `--quantum-version-support` 选项，两者互斥。

`--wait-seconds` 接受 0–3600，默认零表示仅提交、不等待；正值轮询至发布成功或超时。拒绝、人工审核、审核失败和未知状态均失败。超时不会撤回已提交版本，可以使用同一安装包重试检查。`--output text` 为默认模式，`subprocess` 向标准输出写入一个机器可读的成功对象。错误与进度写入标准错误；成功退出码为 0，已处理失败为 1。

## 发布身份

交互式创建限定插件范围的[凭据客户端](glossary.md#credential-client)：

```sh
quantum-cli clients create --email developer@example.com --client-id sample-plugin-ci --display-name "Sample plugin CI" --plugins sample.plugin --private-jwks-output ./publisher-private.json
```

密码输入不回显，也可提供 `QUANTUM_USER_TOKEN` 跳过交互登录。输出路径必须尚不存在。在 Git 之外保护返回的私有 [JWKS](glossary.md#jwks)，并注入发布环境。平台强制验证允许的插件标识。CLI 通过短时有效的签名断言认证，不会为发布保存用户密码。

## 配置

命令选项优先于环境变量，示例均为虚构值。私钥与令牌属于秘密，URL、标识、兼容范围和输出模式属于非秘密配置。

| 变量 | 类型、要求与默认值 | 覆盖方式与安全示例 | 秘密 |
| --- | --- | --- | --- |
| `QUANTUM_PLATFORM_URL` | 平台绝对 URL，可选，默认 https://quantum.io-vii.com | `--base-url`；https://platform.example.com | 否 |
| `QUANTUM_CLIENT_ID` | 发布者标识，发布时必填，无默认值 | `--client-id`；sample-plugin-ci | 否 |
| `QUANTUM_CLIENT_PRIVATE_JWKS` | 私钥集 JSON，未提供密钥文件时必填，无默认值 | `--client-private-jwks FILE` 优先；外部注入测试凭据 | 是 |
| `QUANTUM_USER_TOKEN` | 用户访问令牌，创建客户端时可选，无默认值 | 无命令行令牌选项；不设置则交互登录 | 是 |

`--private-jwks-output` 是创建客户端时必填的输出文件名。`--release-notes` 默认为空文本。创建时 `--client-id`、`--display-name` 和 `--plugins` 必填，仅交互登录时需要 `--email`。真实凭据须通过加密传输。集成测试使用合成密钥及回环 HTTP 端点。

## 仓库边界

CLI 运行仅依赖 .NET 运行时库，测试包限于测试项目。CLI 使用平台公开的网络契约与已发布插件元数据，不引用 Platform、Desktop、Mobile 或 SDK 源码。协议变更必须兼容已部署平台，或明确进行版本化。
