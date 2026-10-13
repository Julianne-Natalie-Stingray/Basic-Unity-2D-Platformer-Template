using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 每个 clip 的唯一标识.
/// 归属: Core (Audio).
/// </summary>
// TODO: 引入自定义 Identifier 包 —— 暂以朴素枚举实现替代.
public enum AudioId
{
    None = -1,

    DefaultSfx,
    DefaultOst,
    
    MouseClick
}
