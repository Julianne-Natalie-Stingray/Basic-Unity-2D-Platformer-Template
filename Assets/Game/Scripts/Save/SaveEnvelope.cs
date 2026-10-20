using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 存档信封: 版本号 + 一组互不解释的 section.
/// 归属: Game (Save).
/// 不负责: section 的顺序与内容; 新增 section 不应改变信封结构.
/// </summary>
[Serializable]
public sealed class SaveEnvelope
{
    /// <summary>当前信封版本; section 内部版本由贡献者自行负责.</summary>
    public const int CurrentVersion = 1;

    public int Version => version;

    public IReadOnlyList<SaveSection> Sections => sections;

    [SerializeField] private int version = CurrentVersion;
    [SerializeField] private List<SaveSection> sections = new();

    public SaveEnvelope()
    {
    }

    public SaveEnvelope(int version, List<SaveSection> sections)
    {
        this.version = version;
        this.sections = sections;
    }

    public bool TryGetSection(string key, out SaveSection section)
    {
        section = null;

        if (string.IsNullOrEmpty(key) || sections == null)
            return false;

        foreach (SaveSection candidate in sections)
        {
            if (candidate != null && candidate.Key == key)
            {
                section = candidate;
                return true;
            }
        }

        return false;
    }
}
