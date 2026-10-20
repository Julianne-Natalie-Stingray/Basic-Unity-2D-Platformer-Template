using UnityEngine;

/// <summary>
/// 把 GameProgress 包装成固定 key 的存档 section.
/// 归属: Game (Progress).
/// 不负责: 场景对象; progress 是全局贡献者, 不参与 Find 扫描.
/// </summary>
public sealed class GameProgressContributor : ISaveContributor
{
    private const string SectionKey = "progress";

    public static GameProgressContributor Instance { get; } = new();

    public string Key => SectionKey;

    private GameProgressContributor()
    {
    }

    public bool CanCapture()
        => true;

    public string Capture()
    {
        LevelProgressStore store = GameProgress.Store;

        return JsonUtility.ToJson(new GameProgressData
        {
            CompletedLevels = store.CompletedLevels,
            LastLevelId = store.LastLevelId
        });
    }

    public void Restore(string json)
    {
        GameProgressData data = JsonUtility.FromJson<GameProgressData>(json);

        if (data == null)
            return;

        GameProgress.Store.Restore(data.CompletedLevels, data.LastLevelId);
    }
}
