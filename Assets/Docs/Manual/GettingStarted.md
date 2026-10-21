# 上手指引

> 面向在既有项目里上手的人，以及从模板派生新项目的人。
> 相关文档：[AGENTS.md](../AGENTS.md)、[开发上手指引](../Development/Onboarding.md)、[Packages](../Development/Packages.md)。

## 1. 环境与依赖

- Unity 版本：**2022.3.43f1c1**（见 `ProjectSettings/ProjectVersion.txt`）。
- 包来源见 `Packages/manifest.json`；包用途与入口见 [Packages.md](../Development/Packages.md)。
- 首次打开：让 Unity 解析 Git 包 → 等编译完成 → 确认 Console 无报错。
- 缺包：检查 Git URL 是否可访问，再重新解析。

## 2. 版本控制

- **Unity Smart Merge（每台机器跑一次）**：`Tools/setup-git.ps1`（Windows）/ `Tools/setup-git.sh`；`.gitattributes` 已声明 `unityyamlmerge`。
- **Force Text** 已开启；`.meta` 必须与文件一起提交。
- **Git LFS**：默认未启用；需要大资产时按 [AGENTS.md](../AGENTS.md) §4.3 开启。
- **一人一 owner**：同一时刻只有一个场景/预制体 owner，禁止并行编辑。

## 3. 运行

- 打开 `Assets/Scene/Samples/MainMenu.unity` 播放 → New Game（清档）或 Continue（读档）进入 `SampleLevel`。
- 或直接播放 `Assets/Scene/Samples/SampleLevel.unity`。
- 闭环：移动 / 跳跃 / 交互；右上角 Menu → PausePanel → Save；Resume 返回游戏；收集全部 3 个目标 → 通关面板 → `Final`；踩 `Hazard` → 死亡面板。
- Build Settings：`MainMenu(0)` / `SampleLevel(1)` / `Final(2)`；新场景用工具窗口注册。
- 场景与验收细节见 [Verification.md](Verification.md)。

## 4. 从模板派生新项目

1. Fork / clone 本仓库，用 Unity **2022.3** 打开。
2. 改产品名 / 公司名：`ProjectSettings/ProjectSettings.asset` 的 `productName` / `companyName`。
3. 跑一次 Smart Merge 脚本（见 §2）。
4. 清理示例内容：替换或删除 `Assets/Scene/Samples` 下的示例关与 `Prefabs` / `SO` / `Visuals` 占位；用工具窗口创建并注册自己的场景。
5. （可选）移除 MCP：从 `Packages/manifest.json` 删除 `com.coplaydev.unity-mcp`。
6. 打开 `Tools/Basic2DPlatformer/Project` 校验：场景全部注册、`Validate` 通过。

## 5. 排障

- **缺包 / 编译失败**：Git URL 不可访问或未解析 → 重新 resolve。
- **场景跑不起来**：场景必须包含 `Core` 预制体（输入 / 音频 / 场景切换）。
- **Play 不动 / 测试不推进**：编辑器失焦时帧不推进 → 保持编辑器窗口聚焦。
- **相机不跟随**：虚拟相机需有 body（`CinemachineTransposer`）且 `Follow` 指向角色。
- **通关不触发**：`LevelManager.rootCondition` 必须指向一个实现 `ILevelGoal` 的组件。

## 6. 上手验收清单

> 目标：确认这个克隆已就绪、能跑通。

### 6.1 依赖

在仓库根运行：

```powershell
(Select-String -Path Packages/packages-lock.json -Pattern '"source": "local"').Count
Select-String -Path Packages/packages-lock.json -Pattern 'com.dbrizov.naughtyattributes' -Context 0,3
```

- [ ] 第一条输出 `0`。
- [ ] 第二条上下文里出现 `"source": "git"`。
- [ ] `Library/PackageCache/` 下存在 `com.dbrizov.naughtyattributes@*`。

### 6.2 Smart Merge

```powershell
git config --get merge.unityyamlmerge.driver
```

- [ ] 输出形如 `"...\UnityYAMLMerge.exe" merge -p %O %B %A %A`。
- [ ] 无输出表示未配置，见 §2。

### 6.3 编译与 Console

- [ ] Unity 编译完成，Console 无红色 Error（黄色 Warning 可接受）。

### 6.4 自带校验工具

- [ ] 打开 `Tools/Basic2DPlatformer/Project` → Scenes 标签 → `Validate`；日志出现 `[Basic2DPlatformer] 校验通过, 无问题.`
- [ ] Build Settings 含 `MainMenu` / `SampleLevel` / `Final`。

### 6.5 目录结构

- [ ] `Assets/` 顶层含：`Docs/ Core/ Game/ Contracts/ Character/ Player/ Level/ UI/ Editor/ Infra/ Scene/ Visuals/ Sounds/`。
- [ ] `Docs/` 含 `Development/` 与 `Manual/`；`Development/Subsystems/` 含各子系统文档。
- [ ] `Scene/Samples/` 含 `MainMenu / SampleLevel / Final`。

### 6.6 最小游戏流程

播放 `Assets/Scene/Samples/MainMenu.unity`：

- [ ] 主菜单 → New Game → 进入 `SampleLevel`；暂停菜单 Save 后返回主菜单，Continue 可读档进入。
- [ ] 右上角 Menu 打开暂停面板并冻结；Resume 解冻。
- [ ] A/D 移动、Space 跳跃；相机跟随且受边界限制。
- [ ] 收集全部 3 个目标 → 通关面板 → Next → `Final`。
- [ ] 踩 `Hazard` → 死亡面板。
- [ ] 全程 Console 无 Error。

## 7. 更多

- 代码结构与程序集：[Development/Onboarding.md](../Development/Onboarding.md)
- 运行与关卡验收：[Verification.md](Verification.md)
- 团队契约：[AGENTS.md](../AGENTS.md)
