# Extending Guide

> How to use each official extension point. 中文: [EXTENDING.md](EXTENDING.md). Background: [ARCHITECTURE.EN.md](ARCHITECTURE.EN.md).

## ① Add a scene / level

1. Create the scene with Tilemap ground, `Hazard`, Core, etc.
2. Set up clear conditions and wire `LevelManager` (scene-local):
   - `LevelManager.rootCondition` points at the root condition (must implement `ILevelGoal`).
   - Single condition: add a `LevelGoal` (trigger) and assign it to `rootCondition`.
   - Multiple conditions: add `LevelGoalAndOperator` (all) or `LevelGoalOrOperator` (any), drop the `ILevelGoal`s into its `goals`, then assign the composite to `rootCondition` (collectible style: tick `disableWhenFulfilled` on a `LevelGoal`).
   - Custom condition: add a component implementing `ILevelGoal` (`bool Fulfilled` + `event Changed`), e.g. a kill counter.
3. Add `EnvironmentFacade` to the scene root (same GameObject as `LevelManager`); the UI's next-level panel reads `Level` through it. A death panel needs a Player carrying `PlayerDeath`.
4. In the scene's folder, create a `SceneDefinition` asset (`Create/Basic2DPlatformer/Scene Definition`) and assign the scene in the Inspector; `Id` auto-fills from the scene name.
5. Open `Tools/Basic2DPlatformer/Project`, click `Register All in Build Settings`; `Validate` should pass.
6. To enter it elsewhere, reference that `SceneDefinition` asset (e.g. `SceneSwitchButton.target`).

> No shared code file is edited; you add a scene plus one new asset.

## ② Add an interactable

1. In any gameplay assembly, add a component implementing `IInteractable` (reference `Basic2DPlatformer.Contracts`):
   `bool CanInteract { get; }`, `event Action<IInteractable> OnInteracted;`, `void Interact(InteractionDetails details)`.
2. In `Interact`, gate on `CanInteract`, apply the effect, then raise `OnInteracted`.
3. Attach it to a scene object with a Collider2D (triggers can be hit by the interaction radius).

## ③ Add a UI screen

1. Attach `UiPanel` to the panel itself: it owns visibility, focus restore, and optional freezing. Never call `SetActive` directly or write `Time.timeScale`.
2. To react to game events, add a script under the persistent Canvas root (`UiRoot`); prefer `SceneSwitchButton` / `OpenPanelButton` / `QuitGameButton` for button behaviour instead of writing button methods on the screen.
3. Panels start inactive; open with `UiPanel.Open()` and close with `UiPanel.Close()` (wire it as a persistent listener if needed).
4. Adding a screen = a new script plus an in-scene panel/prefab; no shared script edits.

## ④ Add an input action

- Use the **Input** tab of `Tools/Basic2DPlatformer/Project` to add/remove actions and bindings; it writes back to `.inputactions` and regenerates `PlayerControls.cs`.
- Then **hand-write** the paired event forwarding in `Core/Managers/InputManager.cs` (the tool never edits Core code).
- `.inputactions` is a shared file — announce changes; `PlayerControls.cs` is generated — never hand-edit.

## ⑤ Add a player ability

1. Add a component in `Assets/Player/Scripts` (reading `CoreFacade.Instance.Input`, `GameStateManager`).
2. Attach it via a **Variant** of the Player prefab; don't edit `Player.prefab` directly.
3. Keep gameplay rules out of Env/Core; the component owns only its own ability.
4. Never put death semantics into `Motor`: the environment calls `IKillable.Kill()`, and `PlayerDeath` shuts physics/motion down, plays the animation, and raises `PlayerDied`.

## ⑥ Add a data SO

- Use the **SO** tab of the tool window to generate an `XxxConfigs` script (with `[CreateAssetMenu]`) plus a default asset; or write `[CreateAssetMenu]` by hand (literal path).
- Data assets hold only data and lookup methods, never logic.

## ⑦ Add a subsystem

1. Create `Assets/<Name>/Scripts/` with an asmdef (`Basic2DPlatformer.<Name>`) and references per [ARCHITECTURE.EN.md](ARCHITECTURE.EN.md).
2. If a higher layer consumes it, update that asmdef's references.
3. Register it under `Assets/Docs/Subsystems/<Name>.md` (+ `.EN.md`) and add a row in `ARCHITECTURE.EN.md`.

## ⑧ Add a project-wide contract

When an interface or data payload is implemented or consumed by **several unrelated assemblies**:

1. Put it in `Assets/Contracts/Scripts` (`Basic2DPlatformer.Contracts`, zero project dependencies).
2. Keep the interface and its payload (`struct`/`class`/`enum`) in the same layer; do not split out `Data`.
3. Upper assemblies reference `Contracts`; `Contracts` **must not** reference any `Basic2DPlatformer.*` assembly.
4. Types used inside a single assembly, or only by upper layers as concrete types, stay where they are.
5. Run `Validate` in `Tools/Basic2DPlatformer/Project` afterwards (it checks asmdef edges).

> Typical: `IInteractable` / `InteractionDetails` / `IKillable`.
