# 开发上手指引

> 面向程序开发。环境安装、运行与验收见 [Manual/GettingStarted.md](../Manual/GettingStarted.md)。
> 相关文档：[ARCHITECTURE.md](ARCHITECTURE.md)、[Design.md](Design.md)、[EXTENDING.md](EXTENDING.md)、[CodeComments.md](CodeComments.md)、[Packages.md](Packages.md)、[AGENTS.md](../AGENTS.md)。

## 1. 工程结构

- 子系统：`Core` / `Game` / `Contracts` / `Character` / `Player` / `Level` / `UI` / `Editor` / `Infra`。
- 代码在 `Assets/<Subsystem>/Scripts/`；资源随子系统（`SO/` / `Prefabs/` / `Physics/`）或按格式（`Scene/` / `Visuals/` / `Sounds/`）。
- 文档：`Assets/Docs/Development/` 放代码相关文档，`Assets/Docs/Manual/` 放操作文档。
- 子系统文档：`Assets/Docs/Development/Subsystems/<Name>.md`。

## 2. 程序集边界

- 程序集依赖严格单向、无环；完整清单见 [ARCHITECTURE.md](ARCHITECTURE.md)。
- 所有脚本必须落在某个 asmdef 内，禁止回退到 `Assembly-CSharp`。
- 外部包的程序集引用按需添加；`NaughtyAttributes.Core`、`GrignardReagent.Pooling`、`GrignardReagent.Timer` 等不会自动进入自定义 asmdef。
- 生成物 `PlayerControls.cs` 归属 `Infra`，不手改。

## 3. 常用开发任务

详见 [EXTENDING.md](EXTENDING.md)：

- **加场景 / 关卡**：建场景 + 放 `LevelManager` / 通关条件 + `EnvironmentFacade` + 建 `SceneDefinition`，再用工具窗口注册。
- **加可交互物**：实现 `Contracts.IInteractable`。
- **加 UI 屏幕**：面板挂 `UiPanel`；按钮优先复用现成按钮组件。
- **加输入动作 / Tag / 数据 SO**：使用 `Tools/Basic2DPlatformer/Project` 的对应标签页。
- **加玩家能力**：新增组件；运动经 `Motor` 意图 API，死亡经 `Contracts.IKillable`。

## 4. 代码规范

- 入口见 [CodeComments.md](CodeComments.md) 与 [AGENTS.md](../AGENTS.md) §2。
- 新类型、序列化字段、日志与可见性规则以团队契约为准。
- 修改共享程序集或 asmdef 前先运行工具窗口的 `Validate`。
