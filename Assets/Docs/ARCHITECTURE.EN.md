# Architecture

> Subsystem layout, assembly boundaries, and extension points. 中文: [ARCHITECTURE.md](ARCHITECTURE.md)
> Full rationale: [2DPlatformerTemplate.md](2DPlatformerTemplate.md).

## 1. Assembly boundaries

Prefix `Basic2DPlatformer`; dependencies are strictly one-way and acyclic.

```
Low (no in-project deps)   Infra, Game, Character
Core    → { Infra, Game }
Player  → { Core, Character }
Level   → { Game, Character }
UI      → { Core, Game, Level, Character }
Editor  → all
```

| Assembly | Covers | Responsibility |
| --- | --- | --- |
| `Basic2DPlatformer.Infra` | `Assets/Infra` | Generated input wrapper; **do not hand-edit** |
| `Basic2DPlatformer.Game` | `Assets/Game/Scripts` | Global state, settings data, progress |
| `Basic2DPlatformer.Core` | `Assets/Core/Scripts` | Input / audio / scene switch / object pool |
| `Basic2DPlatformer.Character` | `Assets/Character/Scripts` | Character motor, movement state, movement config, animation view |
| `Basic2DPlatformer.Player` | `Assets/Player/Scripts` | Input controller, interaction, animation audio |
| `Basic2DPlatformer.Level` | `Assets/Level/Scripts` | Clear conditions, hazards, moving platform, portal |
| `Basic2DPlatformer.UI` | `Assets/UI/Scripts` | UI screens |
| `Basic2DPlatformer.Editor` | `Assets/Editor` | Editor tooling and validation (Editor platform only) |

> Note: `Core → Game` (not the reverse) — `AudioManager`/`SceneSwitchManager` depend on `GameState`. `AudioSettings` therefore lives in `Game/Setting/`.

## 2. Subsystems

| Subsystem | Location | Responsibility |
| --- | --- | --- |
| Infra | `Assets/Infra` | Generated `PlayerControls` |
| Core | `Assets/Core/Scripts` | `CoreFacade`, input, audio, scene switch, object pool |
| Game | `Assets/Game/Scripts` | `GameStateManager`, Setting store, Progress |
| Character | `Assets/Character/Scripts` | `Motor`, `MovementState`, `CharacterMovementConfigs`, `CharacterAnimationView` |
| Player | `Assets/Player/Scripts` | `PlayerController` (input → Motor), `PlayerInteraction`, `PlayerAnimationAudio` |
| Level | `Assets/Level/Scripts` | Clear conditions (`ILevelGoal`/`LevelGoalAndOperator`/`LevelGoalOrOperator`/`LevelManager`), goal, hazards |
| UI | `Assets/UI/Scripts` | Main menu / settings / death / next level / final |
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
| ① Add scene/level | Create scene + add `LevelManager`/clear conditions + `SceneDefinition`; tool registers Build Settings | No |
| ② Add interactable | Implement `IInteractable` | No |
| ③ Add UI screen | New UI script + prefab variant | No |
| ④ Add input action | Editor tool (Phase 2) edits `.inputactions` and regenerates | Yes (tool-managed) |
| ⑤ Add player ability | New component + Player prefab variant (movement via `Motor` intent API) | No |
| ⑥ Add data SO | SO generator (Phase 2) | No |
| ⑦ Add subsystem | Folder + asmdef + register here | Yes (registration) |

How-tos: [EXTENDING.EN.md](EXTENDING.EN.md).

## 5. Key conventions

- Every script must live under an asmdef; never fall back to `Assembly-CSharp`.
- Generated files (`PlayerControls.cs`) are never hand-edited.
- Docs are paired (Chinese canonical); code comments follow [CodeComments.EN.md](CodeComments.EN.md).
