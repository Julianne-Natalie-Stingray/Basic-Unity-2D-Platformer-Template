# Timer

A lightweight coroutine-backed fluent timer for Unity.

## Dependency

This package depends on:

- `com.grignardreagent.gamelog` `1.1.0`

When using local tarballs, install the GameLog package first.

## Installation

- Install `com.grignardreagent.timer` through Unity Package Manager.
- Or add with url "https://github.com/Julianne-Natalie-Stingray/Timer.git".

## Setup

Add `TimerRunner` to a persistent GameObject (for example your `GameCore` object).

Only one `TimerRunner` may exist. Additional instances disable themselves with a warning.

`Timer` needs a runner. Starting a timer without one logs an error and aborts.

## Use

Basic use:

```csharp
new Timer(5f)
    .At(1f, WarnPlayer)
    .At(3f, FlashSomething)
    .OnComplete(Explode)
    .Start();
```

Timers use scaled time by default and therefore pause when `Time.timeScale == 0`.

Use `UseUnscaledTime()` to continue during a time-scale pause:

```csharp
new Timer(2f)
    .UseUnscaledTime()
    .OnComplete(HideMessage)
    .Start();
```

Timestamps are invoked when elapsed time reaches the configured time. `OnComplete` callbacks run after all due timestamps.

### Configure Before First Start

`At`, `OnComplete`, `UseUnscaledTime`, and `CompleteWhen` must be configured before the first `Start()` call. Later configuration changes are ignored with a warning.

A completed timer cannot be started again. Use `Restart()`:

```csharp
Timer timer = new Timer(3f)
    .OnComplete(ShowResult)
    .Start();

timer.Restart(); // resets elapsed time and starts again
```

### Pause and Resume

`Stop()` pauses the timer without resetting elapsed time. `Start()` resumes it.

```csharp
Timer timer = new Timer(3f)
    .OnComplete(ShowResult)
    .Start();

timer.Stop();
timer.Start(); // continues from the paused elapsed time
```

### Conditional Completion

Use `CompleteWhen` to finish when a condition becomes true:

```csharp
new Timer(10f)
    .CompleteWhen(() => player.IsDead)
    .OnComplete(ShowResults)
    .Start();
```

The condition is evaluated once per frame while the timer is running. When it becomes true, remaining timestamps fire immediately and the timer completes.

## API

### Timer

```csharp
new Timer(float duration)
```

Creates a timer. Negative durations are clamped to zero with a warning.

Properties:

| Member | Description |
| --- | --- |
| `Duration` | Total configured duration. |
| `Elapsed` | Elapsed time. |
| `Remaining` | Remaining time. |
| `NormalizedTime` | Progress from 0 to 1. |
| `IsRunning` | Whether the timer is currently running. |
| `IsComplete` | Whether the timer has completed. |

Configuration:

| Method | Description |
| --- | --- |
| `At(float time, Action callback)` | Adds a callback at a time within `[0, Duration]`. |
| `OnComplete(Action callback)` | Adds a completion callback; multiple calls append. |
| `UseUnscaledTime(bool useUnscaled = true)` | Uses unscaled time instead of scaled time. |
| `CompleteWhen(Func<bool> condition)` | Completes when the condition becomes true. |

Control:

| Method | Description |
| --- | --- |
| `Start()` | Starts or resumes the timer. |
| `Stop()` | Pauses without resetting elapsed time. |
| `Restart()` | Resets progress and starts again. |

Invalid timestamps and null callbacks are ignored with a warning.

### TimerRunner

`TimerRunner` is the coroutine host used internally by `Timer`.

- `TimerRunner.Instance` exposes the active runner.
- The runner must live on a persistent GameObject.
- Duplicate runners disable themselves.

## Requirements

- Unity 2022.2 or newer.
- `com.grignardreagent.gamelog` `1.1.0`.
