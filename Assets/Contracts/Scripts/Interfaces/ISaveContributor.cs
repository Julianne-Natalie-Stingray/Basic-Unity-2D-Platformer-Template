/// <summary>
/// 存档中的一个独立 section: 贡献者决定自己能否拍快照, 并自行序列化/恢复.
/// 归属: Contracts.
/// 不负责: 文件读写, section 组装与原子替换; 那些属于 Game 的 SaveSystem.
/// </summary>
public interface ISaveContributor
{
    /// <summary>稳定且唯一的 section key; 重复 key 视为配置错误.</summary>
    string Key { get; }

    /// <summary>状态是否可表示;</summary>
    bool CanCapture();

    /// <summary>纯快照: 只读自身状态, 不修改游戏世界.</summary>
    string Capture();

    /// <summary>在对象完成自身初始化之后调用; 失败时保留 pending 以便重试.</summary>
    void Restore(string json);
}
