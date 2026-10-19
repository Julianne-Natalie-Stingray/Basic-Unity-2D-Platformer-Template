# 上手指引 (Onboarding)

> 面向两类人:① 在既有项目里上手的协作者;② 从模板派生新项目的人。English: [Onboarding.EN.md](Onboarding.EN.md)
> 相关文档:[AGENTS.md](AGENTS.md)(协作契约)、[ARCHITECTURE.md](ARCHITECTURE.md)(结构/程序集)、[EXTENDING.md](EXTENDING.md)(怎么加东西)、[CodeComments.md](CodeComments.md)(注释规范)。
> 注:模板仍在开发中;[2DPlatformerTemplate.md](2DPlatformerTemplate.md) 是**构建期设计规格**,"上手"不必读它。

---

## 1. 环境与依赖

- Unity 版本:**2022.3.43f1c1**(见 `ProjectSettings/ProjectVersion.txt`)。
- 包来源(`Packages/manifest.json`):
  - 引擎包:`com.unity.feature.2d`、`com.unity.inputsystem`(1.7.0)、`com.unity.cinemachine`(2.10.3)、`com.unity.textmeshpro` 等。
  - 第三方(git URL,克隆即得):`com.dbrizov.naughtyattributes`(2.1.6)、`com.grignardreagent.gamelog`、`com.grignardreagent.timer`。
  - 开发/AI 工具(**可选**,后续可能移除):`com.coplaydev.unity-mcp`。
- **首次打开**:让 Unity 解析包(联网拉取 git 包)→ 等编译完成 → 确认 Console 无报错。
- 缺包:检查 git URL 是否可访问,再重新解析。

## 2. 版本控制

- **Unity Smart Merge(每台机器跑一次)**:`Tools/setup-git.ps1`(Windows)/ `Tools/setup-git.sh`;`.gitattributes` 已声明 `unityyamlmerge`。
- **Force Text** 已开启;`.meta` 必须与文件**一起**提交。
- **Git LFS**:默认未启用;需要大资产时按 [AGENTS.md](AGENTS.md) §4.3 开启。
- **一人一 owner**:同一时刻只有一个场景/预制体 owner,禁止并行编辑。

## 3. 工程结构

- 子系统:`Core` / `Game` / `Contracts` / `Character` / `Player` / `Level` / `UI` / `Editor` / `Infra`。
- 对象池 `GrignardReagent.Pooling` 是独立 asmdef(计划抽 UPM),不依赖项目程序集。
- 代码在 `Assets/<Subsystem>/Scripts/`;资源随子系统(`SO/`/`Prefabs/`/`Physics/`)或按格式(`Scene/`/`Visuals/`/`Sounds/`)。
- 程序集边界与依赖方向见 [ARCHITECTURE.md](ARCHITECTURE.md);**所有脚本必须落在某个 asmdef 内**。

## 4. 运行与验收

- 打开 `Assets/Scene/Samples/MainMenu.unity` 播放 → 主菜单 → New Game 进入 `SampleLevel`。
- 或直接播放 `Assets/Scene/Samples/SampleLevel.unity`。
- 闭环:移动/跳跃/交互;**收集全部 3 个目标** → 通关面板 → `Final`;踩 `Hazard` → 死亡面板;可乘坐水平移动平台、走进传送门。
- Build Settings:`MainMenu(0)` / `SampleLevel(1)` / `Final(2)`;新场景用工具窗口一键注册。

## 5. 常见任务

详见 [EXTENDING.md](EXTENDING.md):

- **加场景/关卡**:建场景 + 放 `LevelManager`/通关条件 + `EnvironmentFacade` + 建 `SceneDefinition`(工具注册)。
- **加可交互物**:实现 `Contracts.IInteractable`。
- **加 UI 屏幕**:面板挂 `UiPanel`;按钮用 `SceneSwitchButton` / `OpenPanelButton` / `QuitGameButton`。
- **加输入动作 / Tag / 数据 SO**:工具窗口 `Tools/Basic2DPlatformer/Project` 对应标签页。
- **加玩家能力**:新增组件,运动经 `Motor` 的意图 API;死亡经 `Contracts.IKillable`。

## 6. 从模板派生新项目

1. fork / clone 本仓库,用对应 Unity(**2022.3**)打开。
2. 改**产品名 / 公司名**:`ProjectSettings/ProjectSettings.asset` 的 `productName` / `companyName`。
3. 跑一次 **Smart Merge 脚本**(见 §2)。
4. **清理示例内容**:替换/删除 `Assets/Scene/Samples` 下的示例关与 `Prefabs`/`SO`/`Visuals` 占位;用工具窗口创建并注册你自己的场景。
5. (可选)**移除 MCP**:从 `Packages/manifest.json` 删除 `com.coplaydev.unity-mcp`。
6. 打开 `Tools/Basic2DPlatformer/Project` 校验:场景全部注册、`Validate` 通过。

## 7. 排障

- **缺包 / 编译失败**:git URL 不可访问或未解析 → 重新 resolve。
- **场景跑不起来**:场景必须包含 `Core` 预制体(输入/音频/场景切换)。
- **Play 不动 / 测试不推进**:编辑器失焦时帧不推进 → 保持编辑器窗口聚焦。
- **相机不跟随**:虚拟相机需有 body(`CinemachineTransposer`)且 `Follow` 指向角色。
- **通关不触发**:`LevelManager.rootCondition` 必须指向一个实现 `ILevelGoal` 的组件。

## 8. 上手验收清单(逐条勾选)

> 目标:确认"这个克隆已就绪、能跑通"。全部勾上即上手成功。

### 8.1 依赖(终端)

在**仓库根**运行:

```powershell
# 本地路径依赖应为 0(没有 file: 依赖)
(Select-String -Path Packages/packages-lock.json -Pattern '"source": "local"').Count
# 查看第三方包的来源
Select-String -Path Packages/packages-lock.json -Pattern 'com.dbrizov.naughtyattributes' -Context 0,3
```

- [ ] 第一条输出 **`0`**。
- [ ] 第二条上下文里出现 **`"source": "git"`**。
- [ ] `Library/PackageCache/` 下存在 `com.dbrizov.naughtyattributes@*`(包已连到 git)。

### 8.2 Smart Merge(终端)

```powershell
git config --get merge.unityyamlmerge.driver
```

- [ ] 输出形如 `"...\UnityYAMLMerge.exe" merge -p %O %B %A %A`。(**无输出 = 未配置**,见 §2。)

### 8.3 编译与 Console

- [ ] Unity 打开后等待编译完成;**Console 无红色 Error**(黄色 Warning 可接受)。

### 8.4 自带校验工具

- [ ] 打开 `Tools/Basic2DPlatformer/Project` → **Scenes** 标签 → `Validate`;日志出现 `[Basic2DPlatformer] 校验通过, 无问题.`
- [ ] Build Settings 含 `MainMenu` / `SampleLevel` / `Final`。

### 8.5 目录树

- [ ] `Assets/` 顶层含:`Docs/ Core/ Game/ Character/ Player/ Level/ UI/ Editor/ Infra/ Scene/ Visuals/ Sounds/`。
- [ ] 每个子系统含 `Scripts/`;`Scene/Samples/` 含 `MainMenu / SampleLevel / Final`。

### 8.6 最小游戏流程

播放 `Assets/Scene/Samples/MainMenu.unity`:

- [ ] 主菜单 → **New Game** → 进入 `SampleLevel`。
- [ ] `A/D` 移动、`Space` 跳跃;相机跟随且受边界限制。
- [ ] 收集全部 3 个目标 → 出现通关面板 → **Next** → `Final`。
- [ ] 踩 `Hazard` → 出现死亡面板。
- [ ] 全程 **Console 无 Error**。

## 9. 更多

[ARCHITECTURE.md](ARCHITECTURE.md) · [EXTENDING.md](EXTENDING.md) · [CodeComments.md](CodeComments.md) · [AGENTS.md](AGENTS.md) · [2DPlatformerTemplate.md](2DPlatformerTemplate.md)(构建设计规格,非必读)
