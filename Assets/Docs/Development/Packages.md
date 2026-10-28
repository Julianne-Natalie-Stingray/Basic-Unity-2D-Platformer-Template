# Packages

本文记录项目直接使用的 UPM 包及其入口。版本以 `Packages/packages-lock.json` 为准；Unity 模块包不逐一列出。

## 第三方包

| 包 | 版本 | 用途 | 入口 / 程序集 |
| --- | --- | --- | --- |
| `com.dbrizov.naughtyattributes` | 2.1.6 | Inspector 特性与绘制器 | `NaughtyAttributes.Core` / `NaughtyAttributes.Editor` |
| `com.grignardreagent.gamelog` | 1.1.1 | 结构化日志 | `GameLog` |
| `com.grignardreagent.pooling` | 1.0.0 | 组件池 | `ComponentPool<T>` / `GameObjectPool` |
| `com.grignardreagent.timer` | 1.2.0 | 协程计时器 | `Timer` / `TimerRunner` |
| `com.coplaydev.unity-mcp` | Git | 编辑器 AI 工具（可选） | Unity MCP 窗口 |

第三方包通过 Git URL 安装：

```text
Window > Package Manager > + > Add package from git URL...
```

## 项目相关包

| 包 | 版本 | 用途 |
| --- | --- | --- |
| `com.unity.cinemachine` | 2.10.3 | 相机跟随与边界 |
| `com.unity.inputsystem` | 1.7.0 | 输入资产与生成物 |
| `com.unity.textmeshpro` | 3.0.6 | UI 文本 |
| `com.unity.ugui` | 1.0.0 | uGUI |
| `com.unity.test-framework` | 1.1.33 | EditMode / PlayMode 测试 |
| `com.unity.feature.2d` | 2.0.1 | 2D 功能集 |

其余直接依赖（`collab-proxy`、`ide.rider`、`ide.visualstudio`、`timeline`、`visualscripting`）属于工具链或非运行时架构，版本见 `Packages/manifest.json`。

## 内嵌包

| 包 | 版本 | 用途 | 入口 / 程序集 |
| --- | --- | --- | --- |
| `com.grignardreagent.layeredid` | 0.1.0 | 分层标识、目录与抽屉 | `GrignardReagent.LayeredId` (autoReferenced) / `GrignardReagent.LayeredId.Editor` |

`Packages/com.grignardreagent.layeredid` 是 repo 内嵌包，不写进 `manifest.json` 的 `dependencies`；测试通过 `manifest.json` 的 `testables` 纳入。

## 使用约束

- `NaughtyAttributes` 的运行时特性在 `NaughtyAttributes.Core`；自定义 asmdef 必须显式引用该程序集。
- `NaughtyAttributes.Editor` 仅用于编辑器扩展，运行时程序集不得引用。
- `GameLog` 为 DLL，项目程序集可直接使用。
- `Pooling` 已是 UPM 包；项目内不再保留 `Assets/Pooling`。
- `Timer` 依赖 `GameLog 1.1.0`；场景中的 `TimerRunner` 必须挂在持久对象上。
- `Cinemachine` 固定在 2.x；3.x 与当前 Input System 1.7.0 不兼容。
- `PlayerControls.cs` 由 Input System 生成，归属 `Infra` 程序集，不手改。
- `LayeredId` 运行时可被自动引用；`LayeredId.Editor` 仅供编辑器程序集。发布到远端时保持程序集名不变，并固定 tag。
- `Unity MCP` 仅用于编辑器开发，不进入运行时依赖。

## 相关文档

- 程序集边界与依赖方向：[ARCHITECTURE.md](ARCHITECTURE.md)
- 扩展方式：[EXTENDING.md](EXTENDING.md)
- 运行与环境检查：[Manual/GettingStarted.md](../Manual/GettingStarted.md)
