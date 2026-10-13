using UnityEngine;

/// <summary>
/// 某一组持久化设置的泛型存储: 加载, 持有, 保存并重置一个 TData 实例.
/// 归属: Setting.
/// 不负责: 决定何时保存, 或把加载到的值应用到系统(Configure 抽象).
/// </summary>
public abstract class SettingStore<TData>
    where TData : class, ISettingData, new()
{
    /// <summary>加载在自举期完成, 早于任何场景对象, 因此随时可读.</summary>
    public TData Data => data;

    public string FilePath => filePath;

    protected TData data;
    protected string filePath;

    private const string FileExtension = ".json";

    /// <summary>先从设计默认值铺底再由文件覆盖: 文件缺失的成员回退到设计值而不是零值.</summary>
    public void Load()
    {
        data = new TData();
        data.ResetToDefault();
        filePath = BuildFilePath();

        if (!System.IO.File.Exists(filePath))
        {
            GameLog.Info()
                .Subsystem("Setting")
                .Name(LogName.Class)
                .Issue(LogIssue.Specify($"No saved settings at {filePath}; using defaults. "))
                .Write();

            Configure(data);
            return;
        }

        LoadInto(data);

        Configure(data);
    }

    /// <summary>失败一律带日志回落到默认值且从不抛异常; 子类须改用返回值报告失败.</summary>
    protected void LoadInto(TData target)
    {
        string json;

        try
        {
            json = System.IO.File.ReadAllText(filePath);
        }
        catch (System.Exception exception)
        {
            ReportLoadFailure($"could not read {filePath}: {exception.Message}");

            return;
        }

        if (string.IsNullOrWhiteSpace(json))
        {
            ReportLoadFailure($"{filePath} is empty");

            return;
        }

        if (TryDeserialize(json, target))
            return;

        ReportLoadFailure($"could not parse {filePath}");
    }

    /// <summary>立即写入并以返回值报告成功与否, 从不抛异常.</summary>
    public bool Save()
    {
        string json;

        try
        {
            json = Serialize(data);
        }
        catch (System.Exception exception)
        {
            return ReportSaveFailure($"serialization failed: {exception.Message}");
        }

        try
        {
            System.IO.File.WriteAllText(filePath, json);
        }
        catch (System.Exception exception)
        {
            return ReportSaveFailure($"could not write {filePath}: {exception.Message}");
        }

        GameLog.Info()
            .Subsystem("Setting")
            .Name(LogName.Class)
            .Issue(LogIssue.Specify($"Settings saved to {filePath}. "))
            .Write();

        return true;
    }

    /// <summary>仅在内存中重置; 需随后调用 Save 才持久化.</summary>
    public void ResetToDefault()
    {
        data.ResetToDefault();
        Configure(data);
    }

    /// <summary>JsonUtility 无法序列化泛型参数, 故具体存储须自行实现; 不得抛异常.</summary>
    protected abstract string Serialize(TData target);

    /// <summary>返回 false 表示回落到默认值; 必须覆盖而非新建, 否则缺失成员会取零值.</summary>
    protected abstract bool TryDeserialize(string json, TData target);

    /// <summary>每次加载与重置后调用; 拥有者自行拉取值时空实现合法.</summary>
    protected abstract void Configure(TData target);

    private string BuildFilePath()
        => System.IO.Path.Combine(
            Application.persistentDataPath,
            typeof(TData).Name + FileExtension);

    private void ReportLoadFailure(string reason)
    {
        GameLog.Warning()
            .Subsystem("Setting")
            .Name(LogName.Class)
            .Issue(LogIssue.Specify($"{reason}; design defaults were used instead. "))
            .Action(LogAction.UseFallbackValue("design defaults"))
            .Write();
    }

    private bool ReportSaveFailure(string reason)
    {
        GameLog.Error()
            .Subsystem("Setting")
            .Name(LogName.Class)
            .Issue(LogIssue.Specify($"Settings were not saved: {reason} "))
            .Action(LogAction.Ignore)
            .Write();

        return false;
    }
}
