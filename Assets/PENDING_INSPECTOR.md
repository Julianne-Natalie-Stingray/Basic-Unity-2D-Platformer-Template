# PENDING INSPECTOR

Inspector operations that are **not yet completed by the user**. This file is the hard gate of §5.1.3: while any item
below is unchecked, DSH must not begin new work that depends on those Inspector assignments.

记录**尚未由用户完成**的 Inspector 操作. 本文件是 §5.1.3 的硬闸门: 只要下列任一项未勾选,
DSH 不得开始任何依赖这些 Inspector 赋值的新工作.

The gate is lifted only by your explicit confirmation in words (for example "已配好"). DSH never infers it from the
checkbox state alone.

解闸只能由你**用语言显式确认**触发 (例如"已配好"). DSH 不会仅凭勾选状态自行推断闸门已解除.

## Open items / 未完成项

### 1. Delivery (C) — verify the probe compiled and nothing broke / 交付 (C): 确认探针编译且无破坏

- [x] Unity Console shows no compile error mentioning `DshWriteProbe` or `Diagnostics`.
- [x] Unity Console 没有提及 `DshWriteProbe` 或 `Diagnostics` 的编译错误.
- [x] `Assets/Prefabs/Core.prefab` still resolves `CoreFacade`, `InputManager`, and `AudioManager`
  
      (no "Missing (Mono Script)") after the folder migration.
- [x] 目录迁移后 `Assets/Prefabs/Core.prefab` 仍能解析 `CoreFacade`, `InputManager`, `AudioManager`
  
      (没有 "Missing (Mono Script)").
- [x] The new `.meta` files listed in `Assets/CHANGELOG.md` have been committed together with their files.
- [x] `Assets/CHANGELOG.md` 中列出的新增 `.meta` 已与所属文件一起提交.

### 2. Round trip deliberately deferred to you / 刻意留给你的一次往返

- [x] None in this delivery. Delivery (C) introduces no `[CreateAssetMenu]` ScriptableObject, so no `.menutool` round
  
      trip is required here.
- [x] 本次交付无此项. 交付 (C) 未引入任何 `[CreateAssetMenu]` ScriptableObject, 因此不需要 `.menutool` 往返.

## History / 历史

No prior delivery is awaiting check-off; this is the first entry in this file.

此前没有待勾完的交付; 这是本文件的第一条记录.
