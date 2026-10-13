using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// SceneDefinition 的自定义 Inspector.
/// 归属: Basic2DPlatformer.Editor.
/// </summary>
[CustomEditor(typeof(SceneDefinition))]
public class SceneDefinitionEditor : Editor
{
    private SerializedProperty id;
    private SerializedProperty sceneName;
    private SerializedProperty displayName;

    private void OnEnable()
    {
        id = serializedObject.FindProperty("id");
        sceneName = serializedObject.FindProperty("sceneName");
        displayName = serializedObject.FindProperty("displayName");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(id);
        DrawScenePicker();
        EditorGUILayout.PropertyField(sceneName);
        EditorGUILayout.PropertyField(displayName);
        DrawBuildSettingsStatus();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawScenePicker()
    {
        SceneAsset current = null;
        string path = FindScenePath(sceneName.stringValue);
        if (!string.IsNullOrEmpty(path))
            current = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);

        EditorGUI.BeginChangeCheck();
        SceneAsset picked = (SceneAsset)EditorGUILayout.ObjectField("Scene Asset", current, typeof(SceneAsset), false);
        if (EditorGUI.EndChangeCheck() && picked != null)
        {
            sceneName.stringValue = picked.name;
            if (string.IsNullOrWhiteSpace(id.stringValue))
                id.stringValue = picked.name;
        }
    }

    private void DrawBuildSettingsStatus()
    {
        if (string.IsNullOrWhiteSpace(sceneName.stringValue))
        {
            EditorGUILayout.HelpBox("请先指定场景.", MessageType.Warning);
            return;
        }

        if (IsRegistered(sceneName.stringValue))
        {
            EditorGUILayout.HelpBox("已注册进 Build Settings.", MessageType.Info);
            return;
        }

        EditorGUILayout.HelpBox("尚未注册进 Build Settings.", MessageType.Warning);
        if (GUILayout.Button("注册进 Build Settings"))
            Register(sceneName.stringValue);
    }

    private static string FindScenePath(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        foreach (string guid in AssetDatabase.FindAssets("t:SceneAsset"))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (System.IO.Path.GetFileNameWithoutExtension(path) == name)
                return path;
        }

        return null;
    }

    private static bool IsRegistered(string name)
    {
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (scene != null && System.IO.Path.GetFileNameWithoutExtension(scene.path) == name)
                return true;
        }

        return false;
    }

    private static void Register(string name)
    {
        string path = FindScenePath(name);
        if (string.IsNullOrEmpty(path))
            return;

        List<EditorBuildSettingsScene> scenes = new(EditorBuildSettings.scenes);
        scenes.Add(new EditorBuildSettingsScene(path, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
