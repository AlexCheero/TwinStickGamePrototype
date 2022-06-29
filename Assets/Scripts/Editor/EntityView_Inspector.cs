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

    private void SetSceneDirty() => EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    private EntityView View { get => (EntityView)target; }

    static EntityView_Inspector()
    {
        var componentTypes = Assembly.GetAssembly(typeof(EntityView)).GetTypes()
            .Where((t) => t.Namespace == EntityView.Components).ToArray();
        componentTypeNames = Array.ConvertAll(componentTypes, (t) => t.Name);

        var tagTypes = Assembly.GetAssembly(typeof(EntityView)).GetTypes()
            .Where((t) => t.Namespace == EntityView.Tags).ToArray();
        tagTypeNames = Array.ConvertAll(tagTypes, (t) => t.Name);
    }

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        //return;

        serializedObject.Update();

        var view = View;

        if (GUILayout.Button(new GUIContent(_addExpanded ? "Shrink components list" : "Expand components list"), GUILayout.ExpandWidth(false)))
            _addExpanded = !_addExpanded;
        if (_addExpanded)
        {
            EditorGUILayout.BeginVertical();
                DrawComponentsList(true);
                GUILayout.Space(10);
                
                DrawComponentsList(false);
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
                SetSceneDirty();
            }

            EditorGUILayout.EndHorizontal();
        }
    }

    private void DrawComponentsList(bool isDataComponents)
    {
        EditorGUILayout.LabelField((isDataComponents ? EntityView.Components : EntityView.Tags) + ':');
        GUILayout.Space(10);
        foreach (var componentName in componentTypeNames)
        {
            EditorGUILayout.BeginHorizontal();

            //TODO: add lines between components for readability
            EditorGUILayout.LabelField(componentName);
            bool shouldComponent = GUILayout.Button(new GUIContent("+"), GUILayout.ExpandWidth(false));
            if (shouldComponent)
            {
                _addExpanded = false;

                View.AddComponent(componentName);
            }

            EditorGUILayout.EndHorizontal();
        }
    }

    //TODO: implement drag'n'drop for components
    private void DrawComponent(ref ComponentMeta meta)
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

    private void DrawField(ref ComponentFieldMeta fieldMeta)
    {
        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.LabelField(fieldMeta.Name);
            var valueObject = fieldMeta.GetValue();

            bool setDirty = false;

            if (fieldMeta.TypeName == typeof(int).Name)
            {
                var intValue = valueObject != null ? (int)valueObject : default(int);
                setDirty = fieldMeta.SetValue(EditorGUILayout.IntField(intValue));
            }
            else if (fieldMeta.TypeName == typeof(float).Name)
            {
                var floatValue = valueObject != null ? (float)valueObject : default(float);
                setDirty = fieldMeta.SetValue(EditorGUILayout.FloatField(floatValue));
            }
            else if (fieldMeta.TypeName == typeof(Vector3).Name)
            {
                var vec3Value = valueObject != null ? (Vector3)valueObject : default(Vector3);
                setDirty = fieldMeta.SetValue(EditorGUILayout.Vector3Field("", vec3Value));
            }
            else
            {
                var type = typeof(Component).Assembly.GetType(fieldMeta.TypeName);
                var obj = valueObject != null ? (Component)valueObject : null;
                setDirty = fieldMeta.SetValue(EditorGUILayout.ObjectField("", obj, type, true));
            }

            if (setDirty)
                SetSceneDirty();
        }
        EditorGUILayout.EndHorizontal();
    }
}
