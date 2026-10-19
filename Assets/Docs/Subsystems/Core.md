# Core 子系统

> English: [Core.EN.md](Core.EN.md)。返回 [ARCHITECTURE.md](../ARCHITECTURE.md)。

## 职责

Core 是共享运行时基础设施:输入、音频、场景切换、对象池。它**不**决定玩法规则或下一个场景。

## 组成

| 路径 | 类型 | 职责 |
| --- | --- | --- |
| `CoreFacade.cs` | MonoBehaviour | 单一入口;暴露 `Input` / `Audio` / `SceneSwitch`;`DontDestroyOnLoad` 与重复实例守卫 |
| `Managers/InputManager.cs` | MonoBehaviour | 输入服务;包装生成的 `PlayerControls`,发布原始设备层 |
| `Managers/AudioManager.cs` | MonoBehaviour | 音频请求、池、注册表、限流、混音器应用 |
| `SceneSwitch/SceneSwitchManager.cs` | MonoBehaviour | 唯一切换入口 `RequestSwitch(SceneDefinition)`;拥有 `Loading` |
| `SceneSwitch/SceneDefinition.cs` | ScriptableObject | 每个场景/关卡的稳定定义 |
| `Audio/*` | — | `AudioBuilder` / `ISoundHandle` / `SoundHandle` / `AudioRegistry` / `AudioEmitter` / `AudioId` |
| `GrignardReagent.Pooling` | 独立 asmdef | `ComponentPool<T>`;Core 用它池化 `AudioEmitter` |

## 公共 API

- `CoreFacade.Instance` —— 唯一能熬过场景切换的访问点。
- `CoreFacade.Input` / `.Audio` / `.SceneSwitch`。
- `SceneSwitchManager.RequestSwitch(SceneDefinition)` —— 返回 `AsyncOperation`,被拒返回 `null`。
- `AudioManager.CreateBuilder().With...().Play(audioId)` —— 返回 `ISoundHandle`。

## 边界与注意

- 依赖方向是 **Core → Game**(`AudioManager`/`SceneSwitchManager` 读取 `GameState`),不是相反。
- `Assets/Infra` 里的生成物(`PlayerControls.cs`)属于 Infra 程序集,**不手改**。
- 场景切换不施加机制:进入 `Loading` 不改 `timeScale`、不暂停音频。
- `SceneSwitchConfigs` 与 `SceneId` 已移除,由 `SceneDefinition` 取代。
- 可见性:`AudioRegistry` / `AudioEmitter` / `SoundHandle` 的成员多为 `internal`;外部只通过 `ISoundHandle` 与 `AudioManager.CreateBuilder()` 使用。
- 对象池已迁到 `GrignardReagent.Pooling`(零项目依赖);计划抽成独立 UPM 包。
