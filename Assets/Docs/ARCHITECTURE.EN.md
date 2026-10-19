# Architecture

> Subsystem layout, assembly boundaries, and extension points. 中文: [ARCHITECTURE.md](ARCHITECTURE.md)
> Full rationale: [2DPlatformerTemplate.md](2DPlatformerTemplate.md).

## 1. Assembly boundaries

Prefix `Basic2DPlatformer`; dependencies are strictly one-way and acyclic.

Visibility:

- **Types are always `public`**: scenes/prefabs bind scripts by GUID, and Unity serialization does not depend on C# visibility.
- **Members default to `internal`**; use `public` only when the member is used by another assembly, invoked by a `Button.onClick` persistent listener, an interface implementation, or a documented extension point.
- **Fields are always `[SerializeField] private`**; never `public`/`internal` (Unity does not serialize `internal` fields).
- `[InternalsVisibleTo]` is allowed only for test/Editor assemblies and must be registered here.

```
External package (no project deps)   GrignardReagent.Pooling
Low (no in-project deps)   Contracts, Infra, Game, Character
Core    → { Infra, Game, GrignardReagent.Pooling }
Level   → { Game, Contracts }
Player  → { Core, Character, Contracts }
UI      → { Core, Game, Level, Player }
Editor  → { Infra, Core, Game, UI } (transitively sees all)
```

| Assembly | Covers | Responsibility |
| --- | --- | --- |
| `Basic2DPlatformer.Infra` | `Assets/Infra` | Generated input wrapper; **do not hand-edit** |
| `Basic2DPlatformer.Game` | `Assets/Game/Scripts` | Global state, settings data, progress |
| `Basic2DPlatformer.Contracts` | `Assets/Contracts/Scripts` | Cross-assembly contracts (`IInteractable` / `InteractionDetails` / `IKillable`) |
| `GrignardReagent.Pooling` | `Assets/Pooling/Runtime` | Component pool (`ComponentPool<T>` / `GameObjectPool`); standalone package, no project dependencies |
| `Basic2DPlatformer.Core` | `Assets/Core/Scripts` | Input / audio / scene switch / object pool |
| `Basic2DPlatformer.Character` | `Assets/Character/Scripts` | Character motor, movement state, movement config, animation view |
| `Basic2DPlatformer.Player` | `Assets/Player/Scripts` | Input controller, interaction, animation audio, death state (`PlayerDeath`) |
| `Basic2DPlatformer.Level` | `Assets/Level/Scripts` | Environment facade, clear conditions, hazards, moving platform, portal |
| `Basic2DPlatformer.UI` | `Assets/UI/Scripts` | Panels, button components, screens; subscribes to `PlayerDied` and `LevelCleared` |
| `Basic2DPlatformer.Editor` | `Assets/Editor` | Editor tooling and validation (Editor platform only) |

> Note: `Core → Game` (not the reverse) — `AudioManager`/`SceneSwitchManager` depend on `GameState`. `AudioSettings` therefore lives in `Game/Setting/`.

## 2. Subsystems

| Subsystem | Location | Responsibility |
| --- | --- | --- |
| Infra | `Assets/Infra` | Generated `PlayerControls` |
| Core | `Assets/Core/Scripts` | `CoreFacade`, input, audio, scene switch, object pool |
| Game | `Assets/Game/Scripts` | `GameStateManager`, Setting store, Progress |
| Contracts | `Assets/Contracts/Scripts` | Cross-assembly contracts: `IInteractable`, `InteractionDetails`, `IKillable` |
| Pooling | `Assets/Pooling/Runtime` | `ComponentPool<T>` / `GameObjectPool`; standalone asmdef, no project dependencies |
| Character | `Assets/Character/Scripts` | `Motor`, `MovementState`, `CharacterMovementConfigs`, `CharacterAnimationView` |
| Player | `Assets/Player/Scripts` | `PlayerController` (input → Motor), `PlayerInteraction`, `PlayerAnimationAudio` |
| Level | `Assets/Level/Scripts` | Clear conditions (`ILevelGoal`/`LevelGoalAndOperator`/`LevelGoalOrOperator`/`LevelManager`), goal, hazards |
| UI | `Assets/UI/Scripts` | `UiPanel`, button components (scene switch / open panel / quit), `SettingsScreen`, `NextLevelScreen`, `DeathScreen`, `SelectableAudio` |
| Editor | `Assets/Editor` | Project tool window, custom inspectors |

See [Subsystems/](Subsystems/) for details.

## 3. Scene / level system

- `SceneDefinition` (ScriptableObject, Core): one per scene/level, fields `Id`, `SceneName`, `DisplayName`.
- **No central scene list**, no hand-written `SceneId`; adding a scene = create the scene + its `SceneDefinition`.
- The editor window (`Tools/Basic2DPlatformer/Project`) scans, registers Build Settings, and validates.
- `SceneSwitchManager.RequestSwitch(SceneDefinition)` is the only switch entry point.

## 4. Extension points

| # | Extend | Mechanism | Touches shared files |
| --- | --- | --- | --- |
| ① Add scene/level | Create scene + add `LevelManager`/clear conditions + `EnvironmentFacade` + `SceneDefinition`; tool registers Build Settings | No |
| ② Add interactable | Implement `IInteractable` (Contracts) | No |
| ③ Add UI screen | New `UiPanel` panel + button components / screen script | No |
| ④ Add input action | Editor tool (Phase 2) edits `.inputactions` and regenerates | Yes (tool-managed) |
| ⑤ Add player ability | New component + Player prefab variant (movement via `Motor` intent API; death via `IKillable`) | No |
| ⑥ Add data SO | SO generator (Phase 2) | No |
| ⑦ Add subsystem | Folder + asmdef + register here | Yes (registration) |
| ⑧ Add project-wide contract | Cross-assembly interface/payload goes to `Contracts` (zero project deps) | Yes (registration) |

How-tos: [EXTENDING.EN.md](EXTENDING.EN.md).

## 5. Key conventions

- Every script must live under an asmdef; never fall back to `Assembly-CSharp`.
- Generated files (`PlayerControls.cs`) are never hand-edited.
- Docs are paired (Chinese canonical); code comments follow [CodeComments.EN.md](CodeComments.EN.md).
