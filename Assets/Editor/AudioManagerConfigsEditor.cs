using System;
using System.Collections.Generic;
using GrignardReagent.LayeredId;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 音频注册表 Inspector: 按路径首段分组, 并标出空路径 / 目录外 / 重复的条目.
/// 归属: Editor.
/// </summary>
[CustomEditor(typeof(AudioManagerConfigs))]
public sealed class AudioManagerConfigsEditor : UnityEditor.Editor
{
    private SerializedProperty audios;
    private SerializedProperty collectionCheck;
    private SerializedProperty defaultCapacity;
    private SerializedProperty maxPoolSize;
    private SerializedProperty prewarmAmount;
    private SerializedProperty maxSoundInstance;

    private AudioClipData pendingAdd;

    private void OnEnable()
    {
        audios = serializedObject.FindProperty("audios");
        collectionCheck = serializedObject.FindProperty("collectionCheck");
        defaultCapacity = serializedObject.FindProperty("defaultCapacity");
        maxPoolSize = serializedObject.FindProperty("maxPoolSize");
        prewarmAmount = serializedObject.FindProperty("prewarmAmount");
        maxSoundInstance = serializedObject.FindProperty("maxSoundInstance");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Emitter Pooling", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(collectionCheck);
        EditorGUILayout.PropertyField(defaultCapacity);
        EditorGUILayout.PropertyField(maxPoolSize);
        EditorGUILayout.PropertyField(prewarmAmount);
        EditorGUILayout.PropertyField(maxSoundInstance);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField($"Audio Registry ({audios.arraySize})", EditorStyles.boldLabel);

        if (audios.arraySize == 0)
            EditorGUILayout.HelpBox("注册表为空. ", MessageType.Info);

        List<Entry> entries = Collect();
        Dictionary<string, int> counts = CountPaths(entries);
        entries.Sort((left, right) => string.CompareOrdinal(left.Path, right.Path));

        int removeIndex = -1;
        string group = null;

        foreach (Entry entry in entries)
        {
            string currentGroup = GroupOf(entry.Path);

            if (!string.Equals(group, currentGroup, StringComparison.Ordinal))
            {
                group = currentGroup;
                EditorGUILayout.LabelField(group, EditorStyles.miniBoldLabel);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(10f);

                SerializedProperty element = audios.GetArrayElementAtIndex(entry.Index);
                element.objectReferenceValue = EditorGUILayout.ObjectField(
                    element.objectReferenceValue,
                    typeof(AudioClipData),
                    false);

                EditorGUILayout.LabelField(
                    string.IsNullOrWhiteSpace(entry.Path) ? "<no id>" : entry.Path,
                    EditorStyles.miniLabel,
                    GUILayout.MinWidth(120f));

                if (GUILayout.Button("-", GUILayout.Width(22f)))
                    removeIndex = entry.Index;
            }

            DrawIssues(entry, counts);
        }

        if (removeIndex >= 0)
            audios.DeleteArrayElementAtIndex(removeIndex);

        EditorGUILayout.Space();

        using (new EditorGUILayout.HorizontalScope())
        {
            pendingAdd = (AudioClipData)EditorGUILayout.ObjectField(
                "Add Audio",
                pendingAdd,
                typeof(AudioClipData),
                false);

            using (new EditorGUI.DisabledScope(pendingAdd == null))
            {
                if (GUILayout.Button("+", GUILayout.Width(24f)))
                {
                    audios.InsertArrayElementAtIndex(audios.arraySize);
                    audios.GetArrayElementAtIndex(audios.arraySize - 1).objectReferenceValue = pendingAdd;
                    pendingAdd = null;
                }
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    private List<Entry> Collect()
    {
        var entries = new List<Entry>(audios.arraySize);

        for (int index = 0; index < audios.arraySize; index++)
        {
            var data = audios.GetArrayElementAtIndex(index).objectReferenceValue as AudioClipData;
            entries.Add(new Entry(index, data, data ? data.AudioId.Path : string.Empty));
        }

        return entries;
    }

    private static Dictionary<string, int> CountPaths(List<Entry> entries)
    {
        var counts = new Dictionary<string, int>(StringComparer.Ordinal);

        foreach (Entry entry in entries)
        {
            if (string.IsNullOrWhiteSpace(entry.Path))
                continue;

            counts.TryGetValue(entry.Path, out int count);
            counts[entry.Path] = count + 1;
        }

        return counts;
    }

    private void DrawIssues(Entry entry, Dictionary<string, int> counts)
    {
        if (!entry.Data)
        {
            EditorGUILayout.HelpBox("条目没有引用 AudioClipData. ", MessageType.Warning);
            return;
        }

        if (!entry.Data.AudioId.IsValid)
        {
            EditorGUILayout.HelpBox($"{entry.Data.name}: AudioId 为空. ", MessageType.Error);
            return;
        }

        if (!LayeredIdCatalog.Contains(typeof(AudioCatalog), entry.Path))
        {
            EditorGUILayout.HelpBox(
                $"{entry.Data.name}: {entry.Path} 不在 AudioCatalog 中. ",
                MessageType.Error);
        }

        if (counts.TryGetValue(entry.Path, out int count) && count > 1)
        {
            EditorGUILayout.HelpBox(
                $"{entry.Data.name}: {entry.Path} 出现 {count} 次; 运行时只保留第一条. ",
                MessageType.Error);
        }
    }

    private static string GroupOf(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return "Unassigned";

        int slash = path.IndexOf('/');
        return slash > 0 ? path.Substring(0, slash) : path;
    }

    private readonly struct Entry
    {
        public readonly int Index;
        public readonly AudioClipData Data;
        public readonly string Path;

        public Entry(int index, AudioClipData data, string path)
        {
            Index = index;
            Data = data;
            Path = path;
        }
    }
}
