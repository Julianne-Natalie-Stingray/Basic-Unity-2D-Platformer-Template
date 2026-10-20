using System;
using UnityEngine;

/// <summary>
/// 存档信封中的一个 section: key 定位贡献者, json 是该贡献者自己的载荷.
/// 归属: Game (Save).
/// 不负责: 解释 json; 内部结构由贡献者自行维护.
/// </summary>
[Serializable]
public sealed class SaveSection
{
    public string Key => key;

    public string Json => json;

    [SerializeField] private string key;
    [SerializeField] private string json;

    public SaveSection()
    {
    }

    public SaveSection(string key, string json)
    {
        this.key = key;
        this.json = json;
    }
}
