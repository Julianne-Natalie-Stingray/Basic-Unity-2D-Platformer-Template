using UnityEngine;

/// <summary>
/// 模版的具体 Setting 存储: 一个 JSON 文件持有 GameSettings 的实例.
/// 归属: Setting.
/// 不负责: 文件 I/O, 路径推导与加载/保存序列, 那些属于 SettingStore.
/// </summary>
public sealed class FileSettingStore : SettingStore<GameSettings>
{
    /// <summary>委托 JsonUtility, 这也是 GameSettings 必须保持普通可序列化类的原因.</summary>
    protected override string Serialize(GameSettings target)
        => JsonUtility.ToJson(target, true);

    /// <summary>FromJsonOverwrite 保留顶层缺失成员的设计默认值(嵌套成员无此保证); 解析异常转为 false.</summary>
    protected override bool TryDeserialize(string json, GameSettings target)
    {
        try
        {
            JsonUtility.FromJsonOverwrite(json, target);
            return true;
        }
        catch (System.Exception)
        {
            return false;
        }
    }

    /// <summary>刻意留空: 设置集中存储, 由所描述的系统自行读取, 存储不引用任何系统.</summary>
    protected override void Configure(GameSettings target)
    {
    }
}
