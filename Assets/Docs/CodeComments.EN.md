# Code Comment Standard

> The **single** standard for code comments in this project. 中文: [CodeComments.md](CodeComments.md)
> Based on common practice from Microsoft C# conventions, .NET, and Google C# Style, adapted to this project's team and docs.

## 0. One line

Comments explain **why / contract / invariants / boundaries / side effects** only. If a name can say it, don't comment it.

## 1. Principles

1. **Self-documenting code first**: good names beat comments.
2. Comments explain **why / contract**, never restate **what**.
3. **Stay in sync**: changing code means updating its comments; **a stale comment is a defect**.
4. **Design rationale, architecture, and extension points live in `Docs/`** (see [ARCHITECTURE.EN.md](ARCHITECTURE.EN.md), [EXTENDING.EN.md](EXTENDING.EN.md)); comments do not duplicate docs.
5. Comments are for **maintainers**: write only what the code cannot show.

## 2. Language

- **Chinese only**. No English mirror, no bilingual pairs.
- Keep terms, type names, API names, and keywords in their original English form (e.g. `Rigidbody2D`, `FixedUpdate`).

## 3. What to comment, what not to

| Target | Comment it? |
| --- | --- |
| Types (class / interface / struct / enum) | **Yes** |
| public / internal / protected methods | **Yes** |
| public / internal / protected properties and events | **Yes** |
| Interface members | **Yes** |
| Enum members | Only the **non-obvious** ones |
| private members | **No** (rely on names) |
| Fields | Only when **confusing** |
| Serialized fields | **No comment — use `[Tooltip("...")]`** |
| Inline in methods | Only when the "why" is non-obvious |

## 4. Class comment template

Use an XML `<summary>` (IDE hover), but keep it **minimal**:

- Line 1: **responsibility**, a keyword or short phrase.
- Line 2: **subsystem** it belongs to.
- Optional line 3: **does not own** — only when the boundary is confusing (other boundaries live in `Docs/`).
- If implementation must be described, keep it **under 20 characters**.
- Do **not** write the `Where it lives` / `Lifetime` / `Paradigms` / `Data overview` checklists.

```csharp
/// <summary>
/// 管理音频请求、池与并发上限.
/// 归属: Core (Audio).
/// </summary>
public class AudioManager : MonoBehaviour
```

## 5. Member comments

- One line stating the **contract / side effect / boundary** (return value, exceptions, preconditions, visible consequences).
- **Never restate the name or the code**: don't comment `SetSpeed` as "sets the speed".
- Write nothing when there is no extra contract.
- Form: a single-line XML `<summary>`.

```csharp
/// <summary>首次播放会创建池, 之后复用.</summary>
public ISoundHandle Play(AudioId id, float volume)
```

## 6. Fields

- Default: no comment. Only when meaning is invisible from the name (unit, range, business meaning), write **one line**.
- **Serialized fields always use `[Tooltip]`**, never a comment.

## 7. Inline comments

- Only when the "why" is non-obvious, **1–2 lines**.
- Never describe "what" (the code already does).

## 8. Anti-patterns (forbidden)

- Restating code or names.
- Bilingual mirrors / line-by-line translations.
- Commented-out code (use version control).
- Changelog-style comments (use commits / CHANGELOG).
- Long design essays (move to `Docs/`).
- Stale comments.

## 9. Boundary with docs

- `Docs/` = design rationale, architecture, extension points, long contracts.
- Comments = code-local information.
- The two **never duplicate**; when a long explanation is needed, write one line and link the doc.

## 10. TODO / FIXME / HACK

- `TODO: <task> —— <reason it is not done>`
- `FIXME: <known defect> —— <reason>`
- `HACK: <temporary measure> —— <reason>`
- **Every entry must state a reason**; the format is regex-checkable.

## 11. Region

- **No custom `#region`** (such as `#region APIs`).
- Use conditional-compilation sections only (`#if UNITY_EDITOR`, etc.).

## 12. File header

- **No** regular file header (the type name is the file description).
- **Generated / partial files** put `// 生成物, 勿手改.` at the top.

## 13. Punctuation and wording

- Chinese punctuation; **short sentences**; **no trailing period**.
- Terms / API names stay in English.
- No filler ("this is a class").

## 14. Examples

```csharp
// Good: explains why / contract
/// <summary>
/// 冷却严格按未缩放时间推进, 因此暂停期间仍会走完.
/// 归属: Core (Audio).
/// </summary>
public class AudioEmitter : MonoBehaviour

// Bad: restates the name, no information
/// <summary>
/// AudioEmitter class.
/// Used to emit audio.
/// </summary>
```

```csharp
// Good: non-obvious why
// 复用 emitter 前必须先终结上一次播放, 否则池的租约不会回滚.

// Bad: restates the code
// 把 velocity 设为 0.
body.velocity = Vector2.zero;
```

## 15. Enforcement

- This standard is **convention only**: no `.editorconfig` rules or analyzers are introduced yet (the team has not agreed on formatting).
- Optional later: a built-in **editor validator** that checks whether public members carry comments, with ignore markers.

## 16. Legacy cleanup criteria (for the follow-up task)

- **Must rewrite**: bilingual mirrors, the long class template (`Where/Lifetime/Paradigms/Data` checklists), private member comments, private field comments, restating one-liners, commented-out code.
- **Compliant shape**: type = one-line responsibility + subsystem (+ optional does-not-own); public contract members = one-line contract; inline "why" where needed; `[Tooltip]` for serialized fields.
- **Goal**: the comment ratio drops substantially (it peaked near 87% before the refactor).
