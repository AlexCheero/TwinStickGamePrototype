using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;


[CustomEditor(typeof(EntityView))]
public class EntityView_Inspector : Editor
{
    private static string[] componentTypeNames;
    private static string[] tagTypeNames;

    private string[] _viewComponentTypeNames;

    public override VisualElement CreateInspectorGUI()
    {
        var viewComponents = View.GetComponents<Component>();
        var length = viewComponents.Length - 1;
        _viewComponentTypeNames = new string[length];
        for (int i = 0, j = 0; i < viewComponents.Length && j < length; i++, j++)
        {
            var typeName = viewComponents[i].GetType().Name;
            if (typeName == "EntityView")//skip EntityView
            {
                i++;
                continue;
            }
            _viewComponentTypeNames[j] = typeName;
        }

        return base.CreateInspectorGUI();
    }

    private bool _addListExpanded;

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

        var ecsListText = _addListExpanded ? "Shrink components list" : "Expand components list";
        if (GUILayout.Button(new GUIContent(ecsListText), GUILayout.ExpandWidth(false)))
            _addListExpanded = !_addListExpanded;
        if (_addListExpanded)
        {
            EditorGUILayout.BeginVertical();
                DrawComponentsList(EntityView.Components, componentTypeNames);
                GUILayout.Space(10);
                DrawComponentsList(EntityView.Tags, tagTypeNames);
                GUILayout.Space(10);
                DrawComponentsList("ViewComponents", _viewComponentTypeNames);
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

    private void DrawComponentsList(string label, string[] components)
    {
        EditorGUILayout.LabelField(label + ':');
        GUILayout.Space(10);
        foreach (var componentName in components)
        {
            EditorGUILayout.BeginHorizontal();

            //TODO: add lines between components for readability
            //      or remove "+" button and make buttons with component names on it
            EditorGUILayout.LabelField(componentName);
            bool tryAdd = GUILayout.Button(new GUIContent("+"), GUILayout.ExpandWidth(false));
            if (tryAdd)
            {
                _addListExpanded = false;
                if (View.AddComponent(componentName))
                    SetSceneDirty();
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
