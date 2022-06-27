using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;


[CustomEditor(typeof(EntityView))]
public class EntityView_Inspector : Editor
{
    private static string[] componentTypeNames;
    private static string[] tagTypeNames;

    private bool _addExpanded;

    private void SetDirty() => EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

    static EntityView_Inspector()
    {
        var componentTypes = Assembly.GetAssembly(typeof(EntityView)).GetTypes()
            .Where((t) => t.Namespace == "Components").ToArray();
        componentTypeNames = Array.ConvertAll(componentTypes, (t) => t.Name);

        var tagTypes = Assembly.GetAssembly(typeof(EntityView)).GetTypes()
            .Where((t) => t.Namespace == "Tags").ToArray();
        tagTypeNames = Array.ConvertAll(tagTypes, (t) => t.Name);
    }

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        //return;

        serializedObject.Update();

        var view = (EntityView)target;

        if (GUILayout.Button(new GUIContent("+"), GUILayout.ExpandWidth(false)))
            _addExpanded = !_addExpanded;
        if (_addExpanded)
        {
            EditorGUILayout.BeginVertical();

            EditorGUILayout.LabelField("Components:");
            GUILayout.Space(10);
            foreach (var componentName in componentTypeNames)
                DrawAddButton(componentName);

            GUILayout.Space(10);

            EditorGUILayout.LabelField("Tags:");
            GUILayout.Space(10);
            foreach (var tagName in tagTypeNames)
                DrawAddButton(tagName);

            GUILayout.Space(10);

            EditorGUILayout.EndVertical();
        }

        for (int i = 0; i < view.MetasLength; i++)
        {
            EditorGUILayout.BeginHorizontal();

            DrawComponent(ref view.GetMeta(i));

            //TODO: delete button moves outside of the screen when foldout is expanded
            //component delete button
            if (GUILayout.Button(new GUIContent("-"), GUILayout.ExpandWidth(false)))
            {
                view.RemoveMetaAt(i);
                i--;
                SetDirty();
            }

            EditorGUILayout.EndHorizontal();
        }
    }

    private void DrawAddButton(string componentName)
    {
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField(componentName);
        bool shouldComponent = GUILayout.Button(new GUIContent("+"), GUILayout.ExpandWidth(false));
        if (shouldComponent)
        {
            _addExpanded = false;

        }

        EditorGUILayout.EndHorizontal();
    }

    private static void DrawComponent(ref ComponentMeta meta)
    {
        EditorGUILayout.BeginVertical();
        {
            meta.IsExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(meta.IsExpanded, meta.ComponentName);
            if (meta.IsExpanded)
            {
                for (int i = 0; i < meta.Fields.Length; i++)
                    DrawField(ref meta.Fields[i]);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
        EditorGUILayout.EndVertical();
    }

    private static void DrawField(ref ComponentFieldMeta fieldMeta)
    {
        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.LabelField(fieldMeta.Name);
            var valueObject = fieldMeta.GetValue();
            switch (fieldMeta.Type)
            {
                case EFieldType.Int:
                    EditorGUILayout.IntField((int)valueObject);
                    break;
                case EFieldType.Float:
                    EditorGUILayout.FloatField((float)valueObject);
                    break;
                case EFieldType.Vec3:
                    EditorGUILayout.Vector3Field("", (Vector3)valueObject);
                    break;
                case EFieldType.SceneGO:
                    break;
                case EFieldType.Prefab:
                    break;
            }
        }
        EditorGUILayout.EndHorizontal();
    }
}
