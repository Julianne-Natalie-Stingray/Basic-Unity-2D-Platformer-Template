# Extending Guide

> How to use each official extension point. 中文: [EXTENDING.md](EXTENDING.md). Background: [ARCHITECTURE.EN.md](ARCHITECTURE.EN.md).

## ① Add a scene / level

1. Create the scene with Tilemap ground, `Hazard`, Core, etc.
2. Set up clear conditions and wire `LevelManager` (scene-local):
   - `LevelManager.rootCondition` points at the root condition (must implement `ILevelGoal`).
   - Single condition: add a `LevelGoal` (trigger) and assign it to `rootCondition`.
   - Multiple conditions: add `LevelGoalAndOperator` (all) or `LevelGoalOrOperator` (any), drop the `ILevelGoal`s into its `goals`, then assign the composite to `rootCondition` (collectible style: tick `disableWhenFulfilled` on a `LevelGoal`).
   - Custom condition: add a component implementing `ILevelGoal` (`bool Fulfilled` + `event Changed`), e.g. a kill counter.
3. In the scene's folder, create a `SceneDefinition` asset (`Create/Basic2DPlatformer/Scene Definition`) and assign the scene in the Inspector; `Id` auto-fills from the scene name.
4. Open `Tools/Basic2DPlatformer/Project`, click `Register All in Build Settings`; `Validate` should pass.
5. To enter it elsewhere, reference that `SceneDefinition` asset (e.g. `NextLevelScreen.nextLevel`).

> No shared code file is edited; you add a scene plus one new asset.

## ② Add an interactable

1. In `Assets/Level/Scripts` (or your gameplay assembly), add a component implementing `IInteractable`:
   `bool CanInteract { get; }`, `event Action<IInteractable> OnInteracted;`, `void Interact(InteractionDetails details)`.
2. In `Interact`, gate on `CanInteract`, apply the effect, then raise `OnInteracted`.
3. Attach it to a scene object with a Collider2D (triggers can be hit by the interaction radius).

## ③ Add a UI screen

1. Add a screen script in `Assets/UI/Scripts` (`Basic2DPlatformer.UI` assembly).
2. To freeze the game, attach `FreezeWhileVisible` (coordinated by `FreezeCoordinator`); never write `Time.timeScale` directly.
3. Create a UI prefab, or a **Prefab Variant** of an existing screen; don't edit the base prefab.

## ④ Add an input action

- Use the **Input** tab of `Tools/Basic2DPlatformer/Project` to add/remove actions and bindings; it writes back to `.inputactions` and regenerates `PlayerControls.cs`.
- Then **hand-write** the paired event forwarding in `Core/Managers/InputManager.cs` (the tool never edits Core code).
- `.inputactions` is a shared file — announce changes; `PlayerControls.cs` is generated — never hand-edit.

## ⑤ Add a player ability

1. Add a component in `Assets/Player/Scripts` (reading `CoreFacade.Instance.Input`, `GameStateManager`).
2. Attach it via a **Variant** of the Player prefab; don't edit `Player.prefab` directly.
3. Keep gameplay rules out of Env/Core; the component owns only its own ability.

## ⑥ Add a data SO

- Use the **SO** tab of the tool window to generate an `XxxConfigs` script (with `[CreateAssetMenu]`) plus a default asset; or write `[CreateAssetMenu]` by hand (literal path).
- Data assets hold only data and lookup methods, never logic.

## ⑦ Add a subsystem

1. Create `Assets/<Name>/Scripts/` with an asmdef (`Basic2DPlatformer.<Name>`) and references per [ARCHITECTURE.EN.md](ARCHITECTURE.EN.md).
2. If a higher layer consumes it, update that asmdef's references.
3. Register it under `Assets/Docs/Subsystems/<Name>.md` (+ `.EN.md`) and add a row in `ARCHITECTURE.EN.md`.
