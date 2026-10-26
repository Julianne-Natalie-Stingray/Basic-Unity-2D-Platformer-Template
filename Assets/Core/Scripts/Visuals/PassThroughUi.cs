using UnityEngine;

/// <summary>
/// 全屏点击层的标记: 命中它不算"指针被 UI 挡住".
/// 归属: Core (Visuals).
/// 不负责: 点击行为; 音效等消费者单独挂载.
/// </summary>
[DisallowMultipleComponent]
public sealed class PassThroughUi : MonoBehaviour
{
}
