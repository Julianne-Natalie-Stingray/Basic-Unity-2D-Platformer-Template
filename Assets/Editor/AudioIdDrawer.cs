using System;
using GrignardReagent.LayeredId.Editor;
using UnityEditor;

/// <summary>
/// AudioId 的 Inspector 绑定: 把通用分层抽屉接到 AudioCatalog.
/// 归属: Editor.
/// </summary>
[CustomPropertyDrawer(typeof(AudioId))]
public sealed class AudioIdDrawer : LayeredIdDrawer
{
    protected override Type CatalogType => typeof(AudioCatalog);
}
