# GameState 子系统

## 职能

维护游戏的全局状态标签, 并施加状态变化所隐含的两个机制.

- 拥有 `GameState` 枚举与 `GameStateManager`.
- 接受 `Freeze` / `Resume` 请求, 并在状态变化后对外广播 `Changed`.
- **不负责**决定谁应当响应, 也不负责响应内容.
- 不引用任何其他 Subsystem, 包括 `AudioManager`.

## 构成

| 路径 | 类型 | 职责 |
| --- | --- | --- |
| `GameState.cs` | 枚举 | 状态标签; 目前只有 `Playing` 与 `Freezed` |
| `GameStateManager.cs` | 静态类 | 持有 `Current`; 提供 `Freeze` / `Resume`; 广播 `Changed` |

## 公共API

- `GameStateManager.Current` -> `GameState`: 当前状态. 从第一个场景加载之前到进程结束均有效.
- `GameStateManager.Freeze()`: 挂起游玩. 停止游戏时间, 暂停音频监听器.
- `GameStateManager.Resume()`: 恢复游玩. 还原进入 `Freezed` 之前的时间缩放.
- `GameStateManager.Changed` -> `event Action<GameState>`: 在 `Current` **改变之后**触发.

## 内部实现思路

### 为什么是静态类而不是 Component

状态必须在**第一个场景对象 `Awake` 之前**就是正确的. Component 要做到这一点, 就得依赖一个必须手动放置的物体, 而模版的约定是"无需手动装配". 因此与 `Setting` 一致, 采用静态访问器.

**代价**: 与 `SettingBootstrap` 同样,**无法在 Hierarchy 中找到它**.

### 机制归属: 为什么 GameStateManager 动 timeScale

`Freezed` 不只是标签, 它真的会 `Time.timeScale = 0f` 并 `AudioListener.pause = true`. 这是刻意的: 若只发标签, 每个想暂停的调用方都得自己记住要停什么, 于是"暂停"会在各处被部分实现.

但这也意味着一条必须记住的相互作用: **`Time.timeScale = 0` 会让走 scaled time 的东西全部停摆**. 音频侧为此已把 emitter 的结束判定改为 `UseUnscaledTime()` —— 否则暂停期间 `AudioEmitter` 永不归还池.

### 幂等与时间缩放的记忆

`Freeze` 与 `Resume` 都是**幂等**的, 重复调用只记一条日志.

时间缩放不是固定写 `1f` 还原, 而是**记忆进入时所生效的值**:

- 若写死 `1f`, 任何故意以非 1 倍速运行的玩法(慢动作, 子弹时间)会在暂停一次后**永久丢失**其缩放, 且排查时毫无线索.
- 因此 `Freeze` 记录当时的 `Time.timeScale`, `Resume` 还原它.
- **关键约束**: 已处于 `Freezed` 时再次 `Freeze` 必须直接返回, 不得覆盖已记录的值 —— 否则会把 `0f` 记进去, 解冻后永久冻结.

### 广播顺序

`Apply()` **先赋值 `Current`, 后触发 `Changed`**. 顺序不可颠倒: 处理函数会读取 `Current`, 若先广播再赋值, 每个处理函数观察到的都是上一个状态.

### 谁响应状态

`GameStateManager` 不认识任何响应者. 需要响应状态变化的组件自行订阅 `Changed`. 目前**没有任何订阅者** —— 音频接线会在后续交付中加入第一个.

## TODO

- TODO: 没有可点击的测试入口. `GameStateManager` 是静态类, `NaughtyAttributes` 的 `Button` 只出现在 Component 的 Inspector 上, 没有可挂载的面. 未实现原因同 `SettingBootstrap`; 验证需由玩法代码调用 `Freeze` / `Resume`.
- TODO: 退出时恢复状态未实现. 若在 `Freezed` 状态下退出编辑器, `Time.timeScale` 会保留为 `0`, 影响下次运行. 未实现原因: 需要确认是否值得为此增加一个退出钩子, 而 `Time.timeScale` 在每次进入播放模式时由 Unity 重置, 因此实际影响仅限编辑器会话内.
