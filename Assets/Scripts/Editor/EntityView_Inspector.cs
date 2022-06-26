using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;


[CustomEditor(typeof(EntityView))]
public class EntityView_Inspector : Editor
{
    private static string[] componentTypeNames;

    static EntityView_Inspector()
    {
        var componentTypes = Assembly.GetAssembly(typeof(EntityView)).GetTypes()
            .Where((t) => t.Namespace == "Components").ToArray();
        componentTypeNames = Array.ConvertAll(componentTypes, (t) => t.Name);
    }

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        //return;

        serializedObject.Update();

        var view = (EntityView)target;

        bool shouldAddComponent = GUILayout.Button(new GUIContent("+"), GUILayout.ExpandWidth(false));
        if (shouldAddComponent)
        {

        }

        for (int i = 0; i < view.MetasLength; i++)
            DrawComponent(ref view.GetMeta(i));
    }

    private static void DrawComponent(ref ComponentMeta meta)
    {
        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.BeginVertical();
            {
                meta.IsExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(meta.IsExpanded, meta.ComponentName);
                if (meta.IsExpanded)
                {
                    for (int i = 0; i <meta.Fields.Length; i++)
                        DrawField(ref meta.Fields[i]);
                }
                EditorGUILayout.EndFoldoutHeaderGroup();
            }
            EditorGUILayout.EndVertical();

            //TODO: delete button moves outside of the screen when foldout is expanded
            //component delete button
            bool shouldRemoveComponent = GUILayout.Button(new GUIContent("-"), GUILayout.ExpandWidth(false));

        }
        EditorGUILayout.EndHorizontal();
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
