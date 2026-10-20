# Pooling 子系统

## 子系统功能

负责组件池的复用与释放；不负责对象语义与重置。

## 依赖的子系统及具体依赖业务

暂无。

## API

### 字段

- `ComponentPool<T>.CountAll` / `CountActive` / `CountInactive` / `MaxTotal`：计数与上限。
- `GameObjectPool.MaxTotal` / `CountAll` / `CountActive` / `CountInactive`：同语义。

### 事件

暂无。

### 公共方法

- `ComponentPool<T>.Get` / `Release` / `ReleaseAll` / `Clear` / `Dispose` / `Prewarm`：池操作。
- `GameObjectPool.Get` / `Release`：GameObject 适配。
- `GameObjectPool.ReleaseAll` / `Clear` / `Dispose` / `Prewarm`：同语义。

## 注意事项

### 总则

- `MaxTotal <= 0` 表示不设上限。
- `Release` 幂等；外来或重复释放忽略。
- `onGet` / `onReturn` 由调用方提供。
- `Clear(true)` 同时销毁 active。
- `Dispose` 等价 `Clear(true)`。
- `Prewarm` 不触发回调。
- 编辑模式销毁用 `DestroyImmediate`。

### Core

- 用于 `AudioEmitter` 池化。

## 内容

```text
Packages/
  manifest.json — 包引用
UPM 包 com.grignardreagent.pooling/
  Runtime/
    ComponentPool.cs — 组件池
    GameObjectPool.cs — GameObject 池
```

## 实现思路

### 代码架构

活动与空闲集合 + 容量上限。

### 使用的范式

对象池、回调。

### 内部方法 / 字段 / 事件

- `ComponentPool.all`：全部实例。
- `ComponentPool.active`：活动集合。
- `ComponentPool.maxTotal`：容量上限。
- `GameObjectPool.pool`：内部组件池。
