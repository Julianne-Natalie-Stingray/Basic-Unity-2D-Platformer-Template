using System;
using UnityEngine;

/// <summary>
/// 模版的设置数据: 工程新增设置项的地方, 并定义各设置项的默认值.
/// 归属: Setting.
/// 不负责: 何时落盘, 或值如何到达所描述的系统.
/// </summary>
[Serializable]
public class GameSettings : ISettingData
{
    public AudioSettings Audio => audio;

    [SerializeField] private AudioSettings audio;

    /// <summary>为每个成员赋一个新建的默认值, 因此不会保留其当时碰巧持有的值.</summary>
    public void ResetToDefault()
    {
        audio = AudioSettings.Default();
    }
}
