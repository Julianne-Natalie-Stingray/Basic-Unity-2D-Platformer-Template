using GrignardReagent.LayeredId;

/// <summary>
/// 音频路径目录: 新增音效只在此加一行 const, 不改任何代码分支.
/// 归属: Core (Audio).
/// </summary>
[LayeredIdCatalog]
public sealed class AudioCatalog : LayeredIdCatalog
{
    public const string MouseClick = "Sfx/UI/MouseClick";
    public const string GenericSfx = "Sfx/Generic";
    public const string GenericOst = "Ost/Generic";
}
