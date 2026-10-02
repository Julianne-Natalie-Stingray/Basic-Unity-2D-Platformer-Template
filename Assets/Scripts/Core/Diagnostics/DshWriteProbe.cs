/// <summary>
/// Write-path probe that verifies the DSH-to-Unity handover chain.
/// Subsystem: Core (Diagnostics).
/// Lives: nowhere. It is a static class and is never attached to a GameObject.
/// Responsibility: expose a compile-time constant that only exists on disk after DSH has written this file.
/// Does NOT own: any runtime behaviour, any asset, any scene or prefab wiring.
/// Lifetime: none. Static, never instantiated, never destroyed.
/// Paradigms: none. This is a marker, not a service.
/// Verification chain it serves: DSH writes this .cs -> Unity imports it and generates a .meta with a GUID
/// -> the assembly compiles -> both the .cs and the .meta are visible to git status -> the user commits them together.
/// TODO: Delete this probe after the handover chain has been observed end to end; it carries no production value.
/// 写入链路探针, 用于验证 DSH 到 Unity 的交接链路.
/// Subsystem 归属: Core (Diagnostics).
/// 存在位置: 无. 它是静态类, 不贴在任何 GameObject 上.
/// 职能: 暴露一个编译期常量, 该常量只有在 DSH 把这个文件写到磁盘之后才会存在.
/// 不负责: 任何运行时行为, 任何资产, 任何场景或预制体接线.
/// 生命周期: 无. 静态, 从不实例化, 从不销毁.
/// 使用范式: 无. 它是标记, 不是服务.
/// 它服务的验证链路: DSH 写入本 .cs -> Unity 导入并生成带 GUID 的 .meta -> 程序集编译通过
/// -> git status 同时看得见 .cs 与 .meta -> 你将其一起提交.
/// TODO: 交接链路完整观测通过后删除本探针; 它没有生产价值.
/// </summary>
public static class DshWriteProbe
{
    public const string Marker = "DSH_WRITE_PROBE";

    public const string Expectation = "This line is readable only if DSH wrote this file and Unity imported it.";
}
