using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// 模板的项目工具窗口(场景/输入/标签层/SO).
/// 归属: Basic2DPlatformer.Editor.
/// 不负责: 修改 InputManager 的转发事件(属 Core 代码, 需手写).
/// </summary>
public class ProjectToolWindow : EditorWindow
{
    private enum Tab
    {
        Scenes,
        Input,
        TagsLayers,
        ScriptableObjects
    }

    private const string InputPath = "Assets/Infra/InputSystem/PlayerControls.inputactions";
    private const string TagManagerPath = "ProjectSettings/TagManager.asset";

    private Tab tab = Tab.Scenes;
    private Vector2 scroll;

    private string newActionName = "NewAction";
    private string newActionBinding = "<Keyboard>/";
    private readonly Dictionary<string, string> bindingInputs = new();

    private string newTag = "";
    private string newLayer = "";

    private string soClassName = "NewConfigs";
    private string soFolder = "Assets/Game/Scripts/Data";
    private string soMenuPath = "Basic2DPlatformer/New Configs";

    [MenuItem("Tools/Basic2DPlatformer/Project")]
    private static void Open()
    {
        GetWindow<ProjectToolWindow>("Basic2DPlatformer");
    }

    private void OnGUI()
    {
        tab = (Tab)GUILayout.Toolbar((int)tab, new[] { "Scenes", "Input", "Tags & Layers", "SO" });
        EditorGUILayout.Space();

        scroll = EditorGUILayout.BeginScrollView(scroll);
        switch (tab)
        {
            case Tab.Scenes:
                DrawScenesTab();
                break;
            case Tab.Input:
                DrawInputTab();
                break;
            case Tab.TagsLayers:
                DrawTagsLayersTab();
                break;
            case Tab.ScriptableObjects:
                DrawScriptableObjectTab();
                break;
        }
        EditorGUILayout.EndScrollView();
    }

    private void DrawScenesTab()
    {
        EditorGUILayout.LabelField("Scenes", EditorStyles.boldLabel);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Create SceneDefinition for Active Scene"))
                CreateForActiveScene();
            if (GUILayout.Button("Register All in Build Settings"))
                RegisterAll();
            if (GUILayout.Button("Validate"))
                Validate();
        }

        EditorGUILayout.Space();

        foreach (string path in FindDefinitionPaths())
        {
            SceneDefinition definition = AssetDatabase.LoadAssetAtPath<SceneDefinition>(path);
            if (definition != null)
                DrawDefinition(path, definition);
        }
    }

    private static void DrawDefinition(string path, SceneDefinition definition)
    {
        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            EditorGUILayout.ObjectField("Definition", definition, typeof(SceneDefinition), false);
            EditorGUILayout.LabelField("Id", definition.Id);
            EditorGUILayout.LabelField("Scene", string.IsNullOrEmpty(definition.SceneName) ? "<missing>" : definition.SceneName);
            EditorGUILayout.LabelField("Display", definition.DisplayName);

            bool hasScene = !string.IsNullOrEmpty(FindScenePath(definition.SceneName));
            if (!hasScene)
                EditorGUILayout.HelpBox("场景资产缺失.", MessageType.Error);
            else if (!IsRegistered(definition.SceneName))
                EditorGUILayout.HelpBox("未注册进 Build Settings.", MessageType.Warning);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Select"))
                    Selection.activeObject = definition;
                using (new EditorGUI.DisabledScope(!hasScene))
                {
                    if (GUILayout.Button("Open Scene"))
                        OpenScene(definition.SceneName);
                    if (GUILayout.Button("Register in Build Settings"))
                        Register(definition.SceneName);
                }
            }
        }
    }

    private static void CreateForActiveScene()
    {
        Scene scene = SceneManager.GetActiveScene();
        string scenePath = scene.path;
        if (string.IsNullOrEmpty(scenePath))
        {
            Debug.LogWarning("[Basic2DPlatformer] 当前场景尚未保存, 无法创建 SceneDefinition.");
            return;
        }

        string folder = Path.GetDirectoryName(scenePath).Replace('\\', '/');
        string assetPath = $"{folder}/{scene.name}SceneDefinition.asset";
        if (AssetDatabase.LoadAssetAtPath<SceneDefinition>(assetPath) != null)
        {
            Debug.LogWarning($"[Basic2DPlatformer] {assetPath} 已存在.");
            return;
        }

        SceneDefinition definition = CreateInstance<SceneDefinition>();
        SerializedObject serialized = new(definition);
        serialized.FindProperty("id").stringValue = scene.name;
        serialized.FindProperty("sceneName").stringValue = scene.name;
        serialized.FindProperty("displayName").stringValue = scene.name;
        serialized.ApplyModifiedPropertiesWithoutUndo();

        AssetDatabase.CreateAsset(definition, assetPath);
        AssetDatabase.SaveAssets();
        Selection.activeObject = definition;
        EditorGUIUtility.PingObject(definition);
    }

    private static void RegisterAll()
    {
        List<EditorBuildSettingsScene> scenes = new(EditorBuildSettings.scenes);
        int added = 0;
        foreach (string path in FindDefinitionPaths())
        {
            SceneDefinition definition = AssetDatabase.LoadAssetAtPath<SceneDefinition>(path);
            if (definition == null || string.IsNullOrEmpty(definition.SceneName) || IsRegistered(definition.SceneName))
                continue;

            string scenePath = FindScenePath(definition.SceneName);
            if (string.IsNullOrEmpty(scenePath))
                continue;

            scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            added++;
        }

        EditorBuildSettings.scenes = scenes.ToArray();
        Debug.Log($"[Basic2DPlatformer] 已注册 {added} 个场景进 Build Settings.");
    }

    private static void Validate()
    {
        Dictionary<string, int> idCounts = new();
        Dictionary<string, int> nameCounts = new();
        int problems = 0;

        foreach (string path in FindDefinitionPaths())
        {
            SceneDefinition definition = AssetDatabase.LoadAssetAtPath<SceneDefinition>(path);
            if (definition == null)
                continue;

            if (string.IsNullOrWhiteSpace(definition.SceneName))
            {
                Debug.LogWarning($"[Basic2DPlatformer] {path}: 场景名为空.", definition);
                problems++;
                continue;
            }

            if (string.IsNullOrEmpty(FindScenePath(definition.SceneName)))
            {
                Debug.LogWarning($"[Basic2DPlatformer] {path}: 场景资产缺失 ({definition.SceneName}).", definition);
                problems++;
            }

            if (!IsRegistered(definition.SceneName))
            {
                Debug.LogWarning($"[Basic2DPlatformer] {path}: 未注册进 Build Settings ({definition.SceneName}).", definition);
                problems++;
            }

            idCounts[definition.Id] = idCounts.TryGetValue(definition.Id, out int idCount) ? idCount + 1 : 1;
            nameCounts[definition.SceneName] = nameCounts.TryGetValue(definition.SceneName, out int nameCount) ? nameCount + 1 : 1;
        }

        foreach (KeyValuePair<string, int> entry in idCounts)
        {
            if (entry.Value > 1)
            {
                Debug.LogWarning($"[Basic2DPlatformer] Id 重复: {entry.Key} ({entry.Value} 次).");
                problems++;
            }
        }

        foreach (KeyValuePair<string, int> entry in nameCounts)
        {
            if (entry.Value > 1)
            {
                Debug.LogWarning($"[Basic2DPlatformer] 场景名重复: {entry.Key} ({entry.Value} 次).");
                problems++;
            }
        }

        Debug.Log(problems == 0 ? "[Basic2DPlatformer] 校验通过, 无问题." : $"[Basic2DPlatformer] 校验发现 {problems} 个问题.");
    }

    private static List<string> FindDefinitionPaths()
    {
        List<string> paths = new();
        foreach (string guid in AssetDatabase.FindAssets("t:SceneDefinition"))
            paths.Add(AssetDatabase.GUIDToAssetPath(guid));
        return paths;
    }

    private static string FindScenePath(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        foreach (string guid in AssetDatabase.FindAssets("t:SceneAsset"))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (Path.GetFileNameWithoutExtension(path) == name)
                return path;
        }

        return null;
    }

    private static bool IsRegistered(string name)
    {
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (scene != null && Path.GetFileNameWithoutExtension(scene.path) == name)
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

    private static void OpenScene(string name)
    {
        string path = FindScenePath(name);
        if (!string.IsNullOrEmpty(path))
            EditorSceneManager.OpenScene(path);
    }

    private void DrawInputTab()
    {
        EditorGUILayout.LabelField("Input Actions", EditorStyles.boldLabel);

        InputActionAsset asset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputPath);
        if (asset == null)
        {
            EditorGUILayout.HelpBox($"找不到输入资产: {InputPath}", MessageType.Error);
            return;
        }

        EditorGUILayout.HelpBox("新增/删除动作与绑定后会自动保存并重生成 PlayerControls.cs。\nInputManager 上的转发事件仍需手写。", MessageType.Info);

        foreach (InputActionMap map in asset.actionMaps)
        {
            EditorGUILayout.LabelField($"Map: {map.name}", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                foreach (InputAction action in map.actions)
                    DrawAction(asset, action);
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Add Action (to first map)", EditorStyles.boldLabel);
        newActionName = EditorGUILayout.TextField("Name", newActionName);
        newActionBinding = EditorGUILayout.TextField("Binding", newActionBinding);
        string hint = string.IsNullOrWhiteSpace(newActionBinding) || newActionBinding.EndsWith("/")
            ? "(留空表示不绑定;例如 <Keyboard>/j)"
            : "";
        if (!string.IsNullOrEmpty(hint))
            EditorGUILayout.LabelField(hint, EditorStyles.miniLabel);

        using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(newActionName) || asset.actionMaps.Count == 0))
        {
            if (GUILayout.Button("Add Action"))
                AddAction(asset, newActionName, newActionBinding);
        }
    }

    private void DrawAction(InputActionAsset asset, InputAction action)
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField(action.name + $"  [{action.type}]", GUILayout.Width(200));
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Remove Action", GUILayout.Width(110)))
            {
                action.RemoveAction();
                ApplyInputChanges(asset);
                GUIUtility.ExitGUI();
            }
        }

        for (int i = 0; i < action.bindings.Count; i++)
        {
            InputBinding binding = action.bindings[i];
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField($"    {i}: {binding.path}", EditorStyles.miniLabel);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Remove", GUILayout.Width(70)))
                {
                    action.ChangeBinding(i).Erase();
                    ApplyInputChanges(asset);
                    GUIUtility.ExitGUI();
                }
            }
        }

        if (!bindingInputs.TryGetValue(action.name, out string path))
        {
            path = "<Keyboard>/";
            bindingInputs[action.name] = path;
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            path = EditorGUILayout.TextField(path);
            bindingInputs[action.name] = path;
            if (GUILayout.Button("Add Binding", GUILayout.Width(100)))
            {
                action.AddBinding(path);
                ApplyInputChanges(asset);
                GUIUtility.ExitGUI();
            }
        }
    }

    private void AddAction(InputActionAsset asset, string name, string binding)
    {
        InputActionMap map = asset.actionMaps[0];
        InputAction action = string.IsNullOrWhiteSpace(binding)
            ? map.AddAction(name)
            : map.AddAction(name, binding: binding);
        ApplyInputChanges(asset);
        newActionName = "NewAction";
        newActionBinding = "<Keyboard>/";
    }

    private static void ApplyInputChanges(InputActionAsset asset)
    {
        if (asset == null)
            return;

        // Input System 以 JSON 序列化该资产; 仅 SaveAssets 不会把程序化改动写盘.
        File.WriteAllText(InputPath, asset.ToJson());
        AssetDatabase.ImportAsset(InputPath, ImportAssetOptions.ForceUpdate);
    }

    private void DrawTagsLayersTab()
    {
        EditorGUILayout.LabelField("Tags & Layers", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("只提供查看与新增; 不提供删除/重命名, 以免静默破坏既有引用。", MessageType.Info);

        SerializedObject tagManager = new(AssetDatabase.LoadAllAssetsAtPath(TagManagerPath)[0]);
        SerializedProperty tags = tagManager.FindProperty("tags");
        SerializedProperty layers = tagManager.FindProperty("layers");

        EditorGUILayout.LabelField("Tags", EditorStyles.boldLabel);
        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            for (int i = 0; i < tags.arraySize; i++)
                EditorGUILayout.LabelField(tags.GetArrayElementAtIndex(i).stringValue);
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            newTag = EditorGUILayout.TextField(newTag);
            using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(newTag)))
            {
                if (GUILayout.Button("Add Tag", GUILayout.Width(90)))
                {
                    tags.InsertArrayElementAtIndex(tags.arraySize);
                    tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = newTag;
                    tagManager.ApplyModifiedProperties();
                    newTag = "";
                    GUIUtility.ExitGUI();
                }
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Layers (slots 0-7 are built-in)", EditorStyles.boldLabel);
        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            for (int i = 0; i < layers.arraySize; i++)
            {
                string value = layers.GetArrayElementAtIndex(i).stringValue;
                if (!string.IsNullOrEmpty(value))
                    EditorGUILayout.LabelField($"{i}: {value}");
            }
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            newLayer = EditorGUILayout.TextField(newLayer);
            using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(newLayer)))
            {
                if (GUILayout.Button("Add Layer", GUILayout.Width(90)))
                {
                    int slot = -1;
                    for (int i = 8; i < layers.arraySize; i++)
                    {
                        if (string.IsNullOrEmpty(layers.GetArrayElementAtIndex(i).stringValue))
                        {
                            slot = i;
                            break;
                        }
                    }

                    if (slot < 0)
                    {
                        Debug.LogWarning("[Basic2DPlatformer] 没有空闲的用户层槽位.");
                    }
                    else
                    {
                        layers.GetArrayElementAtIndex(slot).stringValue = newLayer;
                        tagManager.ApplyModifiedProperties();
                        newLayer = "";
                    }
                    GUIUtility.ExitGUI();
                }
            }
        }
    }

    private void DrawScriptableObjectTab()
    {
        EditorGUILayout.LabelField("ScriptableObject Generator", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("先生成脚本; 编译完成后再点 Create Default Asset。", MessageType.Info);

        soClassName = EditorGUILayout.TextField("Class Name", soClassName);
        soFolder = EditorGUILayout.TextField("Script Folder", soFolder);
        soMenuPath = EditorGUILayout.TextField("Menu Path", soMenuPath);

        using (new EditorGUILayout.HorizontalScope())
        {
            using (new EditorGUI.DisabledScope(!IsValidIdentifier(soClassName)))
            {
                if (GUILayout.Button("Generate Script"))
                    GenerateScript();
                using (new EditorGUI.DisabledScope(FindType(soClassName) == null))
                {
                    if (GUILayout.Button("Create Default Asset"))
                        CreateDefaultAsset();
                }
            }
        }

        if (!IsValidIdentifier(soClassName))
            EditorGUILayout.HelpBox("类名必须是合法标识符。", MessageType.Warning);
        else if (FindType(soClassName) == null)
            EditorGUILayout.HelpBox("类型尚未编译, 生成脚本后等待编译完成再创建资产。", MessageType.None);
    }

    private void GenerateScript()
    {
        string folder = soFolder.TrimEnd('/');
        if (!EnsureFolder(folder))
            return;

        string path = $"{folder}/{soClassName}.cs";
        File.WriteAllText(path, BuildScriptSource());
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        Debug.Log($"[Basic2DPlatformer] 已生成脚本 {path}. 编译完成后可创建默认资产.");
    }

    private void CreateDefaultAsset()
    {
        System.Type type = FindType(soClassName);
        if (type == null || !typeof(ScriptableObject).IsAssignableFrom(type))
        {
            Debug.LogWarning($"[Basic2DPlatformer] 找不到可用的 ScriptableObject 类型: {soClassName}.");
            return;
        }

        EnsureFolder("Assets/SO");
        string assetPath = $"Assets/SO/{soClassName}.asset";
        if (AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath) != null)
        {
            Debug.LogWarning($"[Basic2DPlatformer] {assetPath} 已存在.");
            return;
        }

        ScriptableObject instance = CreateInstance(type);
        AssetDatabase.CreateAsset(instance, assetPath);
        AssetDatabase.SaveAssets();
        Selection.activeObject = instance;
        EditorGUIUtility.PingObject(instance);
    }

    private string BuildScriptSource()
    {
        return "using UnityEngine;\n\n"
            + "/// <summary>\n"
            + $"/// {soClassName} —— TODO: 说明职责.\n"
            + "/// 归属: TODO.\n"
            + "/// </summary>\n"
            + $"[CreateAssetMenu(fileName = \"{soClassName}\", menuName = \"{soMenuPath}\")]\n"
            + $"public class {soClassName} : ScriptableObject\n"
            + "{\n"
            + "    // TODO: 在此添加序列化数据字段.\n\n"
            + "#if UNITY_EDITOR\n"
            + "    /// <summary>编辑器校验入口: 调用一次 ClampValues().</summary>\n"
            + "    private void OnValidate()\n"
            + "    {\n"
            + "        ClampValues();\n"
            + "    }\n\n"
            + "    /// <summary>把序列化数据约束到合法范围.</summary>\n"
            + "    private void ClampValues()\n"
            + "    {\n"
            + "        // TODO: 预留.\n"
            + "    }\n"
            + "#endif\n"
            + "}\n";
    }

    private static bool EnsureFolder(string folder)
    {
        if (string.IsNullOrWhiteSpace(folder) || folder == "Assets")
            return folder == "Assets";
        if (AssetDatabase.IsValidFolder(folder))
            return true;

        string parent = Path.GetDirectoryName(folder).Replace('\\', '/');
        string leaf = Path.GetFileName(folder);
        if (!EnsureFolder(parent))
            return false;

        string guid = AssetDatabase.CreateFolder(parent, leaf);
        return !string.IsNullOrEmpty(guid);
    }

    private static bool IsValidIdentifier(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;
        if (!char.IsLetter(name[0]) && name[0] != '_')
            return false;
        foreach (char c in name)
        {
            if (!char.IsLetterOrDigit(c) && c != '_')
                return false;
        }
        return true;
    }

    private static System.Type FindType(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        foreach (System.Reflection.Assembly assembly in System.AppDomain.CurrentDomain.GetAssemblies())
        {
            System.Type type = assembly.GetType(name);
            if (type != null)
                return type;
        }

        return null;
    }
}
