using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GrignardReagent.LayeredId.Editor
{
    /// <summary>
    /// 目录 Inspector: 按路径分层展示成员, 并列出空路径 / 重复路径等问题.
    /// 归属: LayeredId (Editor).
    /// </summary>
    [CustomEditor(typeof(LayeredIdCatalog), true)]
    public sealed class LayeredIdCatalogEditor : UnityEditor.Editor
    {
        private const float IndentWidth = 12f;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            Type catalogType = target.GetType();

            if (catalogType.GetCustomAttributes(typeof(LayeredIdCatalogAttribute), false).Length == 0)
            {
                EditorGUILayout.HelpBox(
                    $"目录未标记 {nameof(LayeredIdCatalogAttribute)}; 生成器与全工程校验不会发现它. ",
                    MessageType.Warning);
            }

            IReadOnlyList<LayeredIdMember> members = LayeredIdCatalog.MembersOf(catalogType);
            IReadOnlyList<LayeredIdIssue> issues = LayeredIdCatalog.Validate(catalogType);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Paths ({members.Count})", EditorStyles.boldLabel);

            if (members.Count == 0)
                EditorGUILayout.HelpBox("目录内没有任何 public const string 成员. ", MessageType.Info);

            foreach (LayeredIdMember member in SortedByPath(members))
            {
                string[] segments = member.Path.Split('/');
                int depth = segments.Length - 1;

                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Space(depth * IndentWidth);
                    EditorGUILayout.LabelField(
                        string.IsNullOrEmpty(segments[depth]) ? "<empty>" : segments[depth],
                        GUILayout.Width(160f));
                    EditorGUILayout.LabelField(member.Name, EditorStyles.miniLabel);
                }
            }

            if (issues.Count == 0)
                return;

            EditorGUILayout.Space();

            foreach (LayeredIdIssue issue in issues)
            {
                string message = string.IsNullOrEmpty(issue.MemberName)
                    ? issue.Message
                    : $"{issue.MemberName}: {issue.Message}";

                EditorGUILayout.HelpBox(message, MessageType.Error);
            }
        }

        private static List<LayeredIdMember> SortedByPath(IReadOnlyList<LayeredIdMember> members)
        {
            var sorted = new List<LayeredIdMember>(members);
            sorted.Sort((left, right) => string.CompareOrdinal(left.Path, right.Path));
            return sorted;
        }
    }
}
