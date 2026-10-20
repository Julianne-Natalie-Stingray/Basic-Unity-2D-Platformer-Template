using System;
using System.Collections.Generic;

/// <summary>
/// 存档流程的唯一编排者: 扫描, 两阶段拍快照, 组装信封, 原子写入与延迟恢复.
/// 归属: Game (Save).
/// 不负责: 场景对象发现(由 Core 的 SaveService 提供), 文件格式细节(由 GameSaveStore 负责).
/// </summary>
public sealed class SaveSystem
{
    private readonly GameSaveStore store;
    private readonly List<ISaveContributor> contributors = new();
    private readonly Dictionary<string, string> pending = new();

    public SaveSystem(GameSaveStore store, IEnumerable<ISaveContributor> contributors = null)
    {
        this.store = store ?? throw new ArgumentNullException(nameof(store));

        if (contributors == null)
            return;

        foreach (ISaveContributor contributor in contributors)
            AddContributor(contributor);
    }

    public bool HasSave => store.Exists;

    public int PendingCount => pending.Count;

    /// <summary>重复的 key 直接拒绝; null 忽略.</summary>
    public bool AddContributor(ISaveContributor contributor)
    {
        if (contributor == null || string.IsNullOrWhiteSpace(contributor.Key))
            return false;

        foreach (ISaveContributor existing in contributors)
        {
            if (existing.Key == contributor.Key)
                return false;
        }

        contributors.Add(contributor);
        return true;
    }

    /// <summary>
    /// 扫描贡献者 → 全部 CanCapture → 全部 Capture → 写盘.
    /// 任一阶段失败都不触碰正式文件; 已拍好的快照被丢弃.
    /// </summary>
    public SaveResult Save(Func<IEnumerable<ISaveContributor>> sceneContributors = null)
    {
        List<ISaveContributor> all;

        try
        {
            all = CollectContributors(sceneContributors);
        }
        catch (Exception exception)
        {
            return SaveResult.Failure($"Could not collect contributors: {exception.Message}");
        }

        HashSet<string> keys = new();

        foreach (ISaveContributor contributor in all)
        {
            if (string.IsNullOrWhiteSpace(contributor.Key))
                return SaveResult.Failure("A contributor has an empty key.");

            if (!keys.Add(contributor.Key))
                return SaveResult.Failure($"Duplicate save key: {contributor.Key}.");
        }

        foreach (ISaveContributor contributor in all)
        {
            bool canCapture;

            try
            {
                canCapture = contributor.CanCapture();
            }
            catch (Exception exception)
            {
                return SaveResult.Failure($"{contributor.Key} failed CanCapture: {exception.Message}");
            }

            if (!canCapture)
                return SaveResult.Failure($"{contributor.Key} is not ready to be captured.");
        }

        List<SaveSection> sections = new();

        foreach (ISaveContributor contributor in all)
        {
            string json;

            try
            {
                json = contributor.Capture();
            }
            catch (Exception exception)
            {
                return SaveResult.Failure($"{contributor.Key} failed Capture: {exception.Message}");
            }

            if (json == null)
                return SaveResult.Failure($"{contributor.Key} returned a null snapshot.");

            sections.Add(new SaveSection(contributor.Key, json));
        }

        sections.Sort((left, right) => string.CompareOrdinal(left.Key, right.Key));

        SaveEnvelope envelope = new(SaveEnvelope.CurrentVersion, sections);

        return store.TrySave(envelope)
            ? SaveResult.Success()
            : SaveResult.Failure($"Could not write {store.FilePath}.");
    }

    /// <summary>读取信封并把 section 放入 pending; 不在此处恢复场景对象.</summary>
    public bool TryLoad()
    {
        if (!store.TryLoad(out SaveEnvelope envelope) || envelope.Sections == null)
            return false;

        Dictionary<string, string> loaded = new();

        foreach (SaveSection section in envelope.Sections)
        {
            if (section == null || string.IsNullOrWhiteSpace(section.Key))
                return false;

            if (!loaded.TryAdd(section.Key, section.Json))
                return false;
        }

        pending.Clear();
        foreach (KeyValuePair<string, string> pair in loaded)
            pending.Add(pair.Key, pair.Value);

        return true;
    }

    /// <summary>按 key 取走 pending section; 失败时保留以便重试.</summary>
    public bool TryRestore(ISaveContributor contributor)
    {
        if (contributor == null || !pending.TryGetValue(contributor.Key, out string json))
            return false;

        try
        {
            contributor.Restore(json);
        }
        catch (Exception)
        {
            return false;
        }

        pending.Remove(contributor.Key);
        return true;
    }

    /// <summary>对当前可见的贡献者逐个尝试; 未匹配的 pending 留待下次场景加载.</summary>
    public void RestoreAll(IEnumerable<ISaveContributor> sceneContributors)
    {
        if (sceneContributors == null)
            return;

        foreach (ISaveContributor contributor in sceneContributors)
            TryRestore(contributor);
    }

    /// <summary>删除存档并丢弃 pending; 内存进度由调用方一并重置.</summary>
    public void NewGame()
    {
        store.Delete();
        pending.Clear();
    }

    private List<ISaveContributor> CollectContributors(Func<IEnumerable<ISaveContributor>> sceneContributors)
    {
        List<ISaveContributor> all = new(contributors);

        if (sceneContributors == null)
            return all;

        IEnumerable<ISaveContributor> scene = sceneContributors();
        if (scene == null)
            return all;

        foreach (ISaveContributor contributor in scene)
        {
            if (contributor != null)
                all.Add(contributor);
        }

        return all;
    }
}
