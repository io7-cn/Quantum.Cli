# 发行

[English](../en/releases.md) · [项目说明](README.md)

独立维护的包保留 NuGet 标识 `Quantum.Platform.Cli` 和命令 `quantum-cli`。项目 Version 属性是版本唯一来源。保留已发布版本与标签，变更使用新版本发布。发行说明须描述兼容性、修复及升级要求。

[Verify](../../.github/workflows/verify.yml) 在主分支更新和变更请求时，分别在 Linux、Windows、macOS 上验证并上传检查过的安装包。[Publish](../../.github/workflows/publish.yml) 对 `vX.Y.Z` 或预发行标签重新执行验证。标签必须匹配项目版本、为附注标签，并指向已验证的 main 历史。发行任务下载已经验证的 Linux 安装包，验证 SHA-256 清单后发布。

NuGet 发布通过 NuGet/login 使用短时有效的 [OIDC](../../.standards/docs/glossary.md#oidc) 受信任发布。首次发布前，包所有者须为 io7-cn/Quantum.Cli、publish.yml 工作流和 release 环境配置 NuGet 受信任发布策略，并将仓库变量 `NUGET_USER` 设置为获得授权的 NuGet 账户。此变量是无默认值的非秘密字符串。release 环境应要求所有者审核。仓库不保存长期 API 密钥，未完成配置时发布失败。

创建仓库与常规 CI 不会发布新包，不移动旧 Platform 的 cli-v 标签，不因迁移发行源而复用已发布版本。

## 0.3.0 候选版 — 2026-11-06

[跟踪议题 #2](https://github.com/io7-cn/Quantum.Cli/issues/2)。发布托管插件时可以省略兼容选项，由兼容的平台从上传程序集读取精确 SDK 版本。保留显式 SDK 与旧宿主兼容选项。未指定选项的重试核对不可变的包摘要；显式填写兼容值时还须保持对应值一致。Web 插件仍须显式声明已测试的 SDK 范围。

先部署兼容的平台，再发布此 CLI。无需配置或数据迁移，分发包根目录须包含 SDK DLL。仍要求 SDK 选项的旧 CLI 必须填写识别出的精确版本。候选版需通过 Linux、Windows 和 macOS 验证，再从经评审的 main 历史创建签名附注标签 `v0.3.0`，通过发行工作流发布，并隔离安装验证正式包。正式发包前使用已验证的本地工具。若发包或安装验证失败，停止推进，改用已有不可变 CLI 包及其支持的显式兼容选项，不原地覆盖已发布包。本版未新增依赖或修改凭据处理。源码版本、CI、不可变包与安装证据记录在关联的发行或议题中。
