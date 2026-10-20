using System;
using System.IO;
using UnityEngine;

/// <summary>
/// 存档文件的唯一 I/O 入口: 读取, 原子写入与删除.
/// 归属: Game (Save).
/// 不负责: 收集贡献者或解释 section; 也不保证跨进程的文件锁.
/// </summary>
public sealed class GameSaveStore
{
    private const string DefaultFileName = "GameSave.json";
    private const string TempSuffix = ".tmp";

    public string FilePath { get; }

    public bool Exists => File.Exists(FilePath);

    public GameSaveStore(string filePath = null)
    {
        FilePath = string.IsNullOrWhiteSpace(filePath)
            ? Path.Combine(Application.persistentDataPath, DefaultFileName)
            : filePath;
    }

    /// <summary>文件缺失/损坏/版本不匹配时返回 false, 且不抛异常.</summary>
    public bool TryLoad(out SaveEnvelope envelope)
    {
        envelope = null;

        if (!Exists)
            return false;

        string json;

        try
        {
            json = File.ReadAllText(FilePath);
        }
        catch (Exception exception)
        {
            ReportLoadFailure($"could not read {FilePath}: {exception.Message}");
            return false;
        }

        if (string.IsNullOrWhiteSpace(json))
        {
            ReportLoadFailure($"{FilePath} is empty");
            return false;
        }

        try
        {
            envelope = JsonUtility.FromJson<SaveEnvelope>(json);
        }
        catch (Exception exception)
        {
            envelope = null;
            ReportLoadFailure($"could not parse {FilePath}: {exception.Message}");
            return false;
        }

        if (envelope == null || envelope.Sections == null)
        {
            ReportLoadFailure($"could not parse {FilePath}");
            return false;
        }

        if (envelope.Version != SaveEnvelope.CurrentVersion)
        {
            ReportLoadFailure($"{FilePath} has unsupported version {envelope.Version}");
            return false;
        }

        return true;
    }

    /// <summary>先写临时文件再替换正式文件; 失败时保留旧文件.</summary>
    public bool TrySave(SaveEnvelope envelope)
    {
        if (envelope == null)
        {
            ReportSaveFailure("envelope is null");
            return false;
        }

        string json;

        try
        {
            json = JsonUtility.ToJson(envelope, true);
        }
        catch (Exception exception)
        {
            ReportSaveFailure($"serialization failed: {exception.Message}");
            return false;
        }

        string tempPath = FilePath + TempSuffix;

        try
        {
            string directory = Path.GetDirectoryName(FilePath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllText(tempPath, json);

            if (File.Exists(FilePath))
                File.Replace(tempPath, FilePath, null);
            else
                File.Move(tempPath, FilePath);

            return true;
        }
        catch (Exception exception)
        {
            TryDelete(tempPath);
            ReportSaveFailure($"could not write {FilePath}: {exception.Message}");
            return false;
        }
    }

    /// <summary>删除正式文件; 不存在时为空操作.</summary>
    public void Delete()
    {
        TryDelete(FilePath);
        TryDelete(FilePath + TempSuffix);
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch (Exception exception)
        {
            GameLog.Warning()
                .Subsystem("Save")
                .Name(LogName.Class)
                .Issue(LogIssue.Specify($"could not delete {path}: {exception.Message} "))
                .Action(LogAction.Ignore)
                .Write();
        }
    }

    private static void ReportLoadFailure(string reason)
    {
        GameLog.Warning()
            .Subsystem("Save")
            .Name(LogName.Class)
            .Issue(LogIssue.Specify($"{reason}; no save was restored. "))
            .Action(LogAction.UseFallbackValue("empty progress"))
            .Write();
    }

    private static void ReportSaveFailure(string reason)
    {
        GameLog.Error()
            .Subsystem("Save")
            .Name(LogName.Class)
            .Issue(LogIssue.Specify($"Save was not written: {reason} "))
            .Action(LogAction.Ignore)
            .Write();
    }
}
