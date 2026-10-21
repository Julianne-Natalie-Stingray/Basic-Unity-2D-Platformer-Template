# AGENTS —— 团队协作契约

> 本文件是本工程(由 2D Platformer 模板派生)的协作契约。AI 代理习惯读取仓库根的 `AGENTS.md`,那里是指向本文件的指针。
> 新成员先看 [Manual/GettingStarted.md](Manual/GettingStarted.md) 与 [Development/Onboarding.md](Development/Onboarding.md)。

## 1. 分工与权限

- 代码与编辑器操作均可由人或 AI 完成;AI 通过 Unity MCP 操作场景、预制体、ScriptableObject、包与输入资产。
- 可写:`Assets/`、`Packages/`、`ProjectSettings/`(后两者请谨慎并集中到单一 owner)。
- 禁写:`Library/`、`Temp/`、`Logs/`、`UserSettings/`、`obj/`、`.vs/`、`.idea/`。
- 仓库根与 `Assets/` 之外的改动(`.gitattributes`、`Tools/` 脚本等)应在评审后进行。

## 2. 代码规范(摘要)

- 一个文件一个类型;显式访问词;property + backing field;`GameLog` 记日志;`InitializeInternal` / `ResolveDependencies`。
- 可见性:类型 `public`,成员默认 `internal`(例外见 `ARCHITECTURE.md §1`);字段只用 `[SerializeField] private`。
- `ScriptableObject`用`NaughtyAttributes`的`[Expandable]`展开
- 代码注释以 [Development/CodeComments.md](Development/CodeComments.md) 为准(中文单一语言)。
- 详细边界与理由见 [Development/ARCHITECTURE.md](Development/ARCHITECTURE.md)。

## 3. 文档规范

- 所有文档集中在 `Assets/Docs/`。
- 代码相关文档在 `Assets/Docs/Development/`;子系统文档在 `Assets/Docs/Development/Subsystems/<Name>.md`。
- 文档规范见 [Development/DocumentationRules.md](Development/DocumentationRules.md)。
- 面向非程序的操作文档在 `Assets/Docs/Manual/`。

## 4. 版本控制

### 4.1 Unity Smart Merge

- 仓库根 `.gitattributes` 已声明 `*.unity`、`*.prefab`、`*.asset`、`*.controller`、`*.meta` 使用 `unityyamlmerge`。
- **每台机器需一次性配置合并驱动**(Unity 路径因人而异):运行 `Tools/setup-git.ps1`(Windows)/`Tools/setup-git.sh`。
- 未配置时 git 会**静默退回**普通合并,冲突会明显增多。
- Smart Merge 无法合并同一对象同一字段的改动,也无法合并二进制。

### 4.2 合并纪律

- **一个场景/预制体同一时刻只有一个 owner**;禁止两人并行编辑同一场景或预制体。
- 共用资产用 Prefab Variant 派生,避免直接改主预制体。

### 4.3 Git LFS(可选)

模板默认**不启用**。若项目资产变大,按需启用:

- 每台机器安装 git-lfs 并执行 `git lfs install`。
- `git lfs track` 建议清单:`*.png *.psd *.aseprite *.tga *.jpg *.wav *.mp3 *.ogg *.ttf *.otf *.fbx *.blend *.mp4 *.mov`。
- **不要** LFS `*.unity/*.prefab/*.asset/*.meta`:它们是文本,交给 Smart Merge。
- 注意配额。

## 5. 拓展

各拓展点的做法见 [Development/EXTENDING.md](Development/EXTENDING.md)。
