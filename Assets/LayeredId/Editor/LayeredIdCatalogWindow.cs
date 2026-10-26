using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GrignardReagent.LayeredId.Editor
{
    /// <summary>
    /// 目录校验窗口: 列出所有目录类的成员与问题; 目录是普通类, 无法用 Inspector 承载.
    /// 归属: LayeredId (Editor).
    /// </summary>
    public sealed class LayeredIdCatalogWindow : EditorWindow
    {
        private const string MenuPath = "Tools/GrignardReagent/Layered Id Catalogs";

        private readonly HashSet<Type> expanded = new();
        private Vector2 scroll;

        [MenuItem(MenuPath)]
        private static void Open()
            => GetWindow<LayeredIdCatalogWindow>("Layered Id Catalogs");

        private void OnGUI()
        {
            var tagged = new List<Type>();
            var untagged = new List<Type>();

            foreach (Type type in TypeCache.GetTypesDerivedFrom<LayeredIdCatalog>())
            {
                if (type.IsAbstract)
                    continue;

                bool hasAttribute = type.GetCustomAttributes(typeof(LayeredIdCatalogAttribute), false).Length > 0;
                (hasAttribute ? tagged : untagged).Add(type);
            }

            tagged.Sort((left, right) => string.CompareOrdinal(left.Name, right.Name));
            untagged.Sort((left, right) => string.CompareOrdinal(left.Name, right.Name));

            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(60f)))
                    Repaint();
            }

            scroll = EditorGUILayout.BeginScrollView(scroll);

            EditorGUILayout.LabelField($"Tagged ({tagged.Count})", EditorStyles.boldLabel);

            foreach (Type type in tagged)
                DrawCatalog(type, false);

            if (untagged.Count > 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField($"Untagged ({untagged.Count})", EditorStyles.boldLabel);

                foreach (Type type in untagged)
                    DrawCatalog(type, true);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawCatalog(Type catalogType, bool untagged)
        {
            bool isExpanded = expanded.Contains(catalogType);

            using (new EditorGUILayout.HorizontalScope())
            {
                bool next = EditorGUILayout.Foldout(isExpanded, $"{catalogType.Name} ({catalogType.Namespace})", true);

                if (next != isExpanded)
                {
                    if (next)
                        expanded.Add(catalogType);
                    else
                        expanded.Remove(catalogType);
                }

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Copy", EditorStyles.miniButton, GUILayout.Width(46f)))
                    EditorGUIUtility.systemCopyBuffer = catalogType.FullName;
            }

            if (untagged)
            {
                EditorGUILayout.HelpBox(
                    $"未标记 {nameof(LayeredIdCatalogAttribute)}; 生成器与全工程校验不会发现它. ",
                    MessageType.Warning);
            }

            if (!expanded.Contains(catalogType))
                return;

            IReadOnlyList<LayeredIdMember> members = LayeredIdCatalog.MembersOf(catalogType);

            using (new EditorGUI.IndentLevelScope())
            {
                foreach (LayeredIdMember member in members)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField(
                            string.IsNullOrWhiteSpace(member.Path) ? "<empty>" : member.Path,
                            GUILayout.MinWidth(180f));
                        EditorGUILayout.LabelField(member.Name, EditorStyles.miniLabel, GUILayout.MinWidth(120f));
                    }
                }

                foreach (LayeredIdIssue issue in LayeredIdCatalog.Validate(catalogType))
                {
                    string message = string.IsNullOrEmpty(issue.MemberName)
                        ? issue.Message
                        : $"{issue.MemberName}: {issue.Message}";

                    EditorGUILayout.HelpBox(message, MessageType.Error);
                }
            }

            EditorGUILayout.Space();
        }
    }
}
