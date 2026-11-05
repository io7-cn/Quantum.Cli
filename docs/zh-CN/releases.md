# 发行

[English](../en/releases.md) · [项目说明](README.md)

独立维护的包保留 NuGet 标识 `Quantum.Platform.Cli` 和命令 `quantum-cli`。项目 Version 属性是版本唯一来源。保留已发布版本与标签，变更使用新版本发布。发行说明须描述兼容性、修复及升级要求。

[Verify](../../.github/workflows/verify.yml) 在主分支更新和变更请求时，分别在 Linux、Windows、macOS 上验证并上传检查过的安装包。[Publish](../../.github/workflows/publish.yml) 对 `vX.Y.Z` 或预发行标签重新执行验证。标签必须匹配项目版本、为附注标签，并指向已验证的 main 历史。发行任务下载已经验证的 Linux 安装包，验证 SHA-256 清单后发布。

NuGet 发布通过 NuGet/login 使用短时有效的 [OIDC](../../.standards/docs/glossary.md#oidc) 受信任发布。首次发布前，包所有者须为 io7-cn/Quantum.Cli、publish.yml 工作流和 release 环境配置 NuGet 受信任发布策略，并将仓库变量 `NUGET_USER` 设置为获得授权的 NuGet 账户。此变量是无默认值的非秘密字符串。release 环境应要求所有者审核。仓库不保存长期 API 密钥，未完成配置时发布失败。

创建仓库与常规 CI 不会发布新包，不移动旧 Platform 的 cli-v 标签，不因迁移发行源而复用已发布版本。
