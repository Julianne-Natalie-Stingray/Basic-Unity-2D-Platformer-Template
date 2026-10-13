/// <summary>
/// 某个 Setting 存储的持久化数据契约: 只含基元, 字符串, 数组或嵌套可序列化字段的数据类.
/// 归属: Setting.
/// </summary>
public interface ISettingData
{
    /// <summary>把每个字段恢复为默认值; 存储据此定义首次运行的基线.</summary>
    void ResetToDefault();
}
