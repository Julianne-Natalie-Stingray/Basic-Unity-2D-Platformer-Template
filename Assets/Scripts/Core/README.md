# Core Subsystem / Core 子系统

## Responsibilities / 职能

Core is the single entry point of the project's shared runtime infrastructure.

Core 是本工程共享运行时基础设施的单一入口.

- Owns the shared service facade that other Subsystems obtain their dependencies from.
- 拥有共享服务门面, 其他 Subsystem 由它取得依赖.
- Owns input handling, audio playback, and object pooling.
- 拥有输入处理, 音频播放与对象池.
- Does NOT own game rules, gameplay state, or any scene-specific behaviour.
- 不负责游戏规则, 玩法状态, 以及任何场景专属行为.
- Does NOT own the generated input bindings; those live under `Assets/Infra/InputSystem/`.
- 不负责生成的输入绑定资产; 那些位于 `Assets/Infra/InputSystem/` 下.

## Composition / 构成

| Path | Kind | Role |
|---|---|---|
| `CoreFacade.cs` | MonoBehaviour on `GameCore` | Single entry point; exposes `Input` and `Audio`; owns `DontDestroyOnLoad` |
| `Managers/InputManager.cs` | MonoBehaviour | Input service; consumes the generated InputSystem actions |
| `Managers/AudioManager.cs` | MonoBehaviour | Audio service; resolves clips through its ScriptableObject configs |
| `Audio/` | Playback + SO definitions | `AudioEmitter` playback and the `AudioId` / `AudioClipData` / `AudioManagerConfigs` data layer |
| `GameObjectPool/` | Utility | Reusable object pooling |
| `Input/` | Helper service | `DragAndDropService2D`, a plain serializable helper class |
| `Diagnostics/` | Probe | `DshWriteProbe`, a temporary write-path probe; see `Assets/CHANGELOG.md` |

## Public API / 公共API

- `CoreFacade.Input` -> `InputManager`: the input service.
- `CoreFacade.Audio` -> `AudioManager`: the audio service.
- `InputManager` and `AudioManager` are required components of `CoreFacade` and are resolved through `GetComponent<>()` in `InitializeInternal()`.
- `AudioManagerConfigs` and `AudioClipData` are ScriptableObject definitions; `AudioId` is the clip lookup key.

## Internal approach / 内部实现思路

- Initialization happens in `Awake()`; dependencies inside the Subsystem are resolved by `InitializeInternal()` through
  `GetComponent<>()` plus caching, so no Inspector wiring is required.
- Manager references are cached in private backing fields and exposed as read-only properties.
- Data that must survive across scenes is persisted in ScriptableObjects rather than in serialized scene fields.
- `CoreFacade` is the only type that calls `DontDestroyOnLoad`; managers do not call it themselves.

- 初始化在 `Awake()` 中完成; Subsystem 内部依赖由 `InitializeInternal()` 通过 `GetComponent<>()` 加缓存解决, 不需要 Inspector 接线.
- 管理器引用缓存在私有字段中, 以只读 Property 暴露.
- 需要跨场景存活的数据持久化为 ScriptableObject, 而不是场景内序列化字段.
- `CoreFacade` 是唯一调用 `DontDestroyOnLoad` 的类型; 各管理器自身不调用.

## TODO

- TODO: Expand `Managers/InputManager.cs` and `GameObjectPool/GameObjectPool.cs` documentation detail once their public surfaces are confirmed against the live prefab wiring. Not expanded here because that wiring is verified through the Inspector, which is outside this workspace's authority.
