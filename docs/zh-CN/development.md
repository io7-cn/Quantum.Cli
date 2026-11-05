# 开发

[English](../en/development.md) · [项目说明](README.md)

使用 global.json 选择的 .NET 10、PowerShell 7 及 Python 3.12 或更高版本。

```sh
git submodule update --init --recursive
pwsh .ci/Verify.ps1
python .standards/.ci/check-adoption.py
python .ci/check-docs.py
```

验证脚本锁定依赖还原，执行 `dotnet format`、Release 构建与测试，检查 NuGet 包，再将确切的本地包安装到隔离工具目录，检查帮助与版本输出。构建产物和临时凭据不提交到仓库。

| 目录 | 职责 |
| --- | --- |
| src/Quantum.Cli/Commands | 命令分发、选项、用户交互与编排 |
| src/Quantum.Cli/Authentication | 使用运行时密码学及 [base64url](glossary.md#base64url) 支持签署发布断言 |
| src/Quantum.Cli/Packaging | 确定性归档与根级插件清单 |
| src/Quantum.Cli/Publishing | 发行状态判断 |
| src/Quantum.Cli/Transport | 经过认证的平台 [RPC](../../.standards/docs/glossary.md#rpc) 请求 |
| tests/Quantum.Cli.Tests | 镜像源码职责的测试；合成本地集成测试 |
| .ci | 构建与验证辅助工具 |

共享 EditorConfig 定义命名、文件布局与格式，不覆盖单个 IDE 诊断级别。变更依赖时明确更新锁文件，并验证干净还原。为受影响的协议、签名与打包行为补充测试。验证时不使用生产凭据执行真实发布。
