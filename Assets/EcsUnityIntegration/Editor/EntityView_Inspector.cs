using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;


//TODO: hide some components fields that shouldn't be visible in inspector such as AttackComponent.previousAttackTime
//TODO: implement runtime fileds update
//TODO: implement search bar for components
[CustomEditor(typeof(EntityView))]
public class EntityView_Inspector : Editor
{
    private const string UnityComponents = "UnityComponents";

    private static string[] componentTypeNames;
    private static string[] tagTypeNames;

    private string[] _viewComponentTypeNames;
    private bool _addListExpanded;

    private EntityView View { get => (EntityView)target; }

    public override VisualElement CreateInspectorGUI()
    {
        var viewComponents = View.GetComponents<Component>();
        var length = viewComponents.Length - 1;
        _viewComponentTypeNames = new string[length];
        for (int i = 0, j = 0; i < viewComponents.Length && j < length; i++, j++)
            _viewComponentTypeNames[j] = viewComponents[i].GetType().FullName;

        return base.CreateInspectorGUI();
    }

    static EntityView_Inspector()
    {
        componentTypeNames = InspectorHelper.GetTypeNames<EntityView>((t) => t.Namespace == EntityView.Components);
        tagTypeNames = InspectorHelper.GetTypeNames<EntityView>((t) => t.Namespace == EntityView.Tags);
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var listDatas = new AddListData[]
        {
            new AddListData { Caption = EntityView.Components, Items = componentTypeNames },
            new AddListData { Caption = EntityView.Tags, Items = tagTypeNames },
            new AddListData { Caption = UnityComponents, Items = _viewComponentTypeNames }
        };
        InspectorHelper.DrawAddLists(ref _addListExpanded,
                                    "Shrink components list",
                                    "Expand components list",
                                    listDatas,
                                    OnAddComponent);

        var view = View;
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
                EditorUtility.SetDirty(target);
            }

            EditorGUILayout.EndHorizontal();
        }
    }

    private void OnAddComponent(string componentName)
    {
        _addListExpanded = false;
        var type = EntityView.GetUnityComponentTypeByName(componentName);
        if (EntityView.IsUnityComponent(type))
        {
            MethodInfo getComponentInfo = typeof(EntityView).GetMethod("GetComponent", new Type[] { }).MakeGenericMethod(type);
            var component = (Component)getComponentInfo.Invoke(View, null);
            if (View.AddUnityComponent(component))
                EditorUtility.SetDirty(target);
        }
        else
        {
            if (View.AddComponent(componentName))
                EditorUtility.SetDirty(target);
        }
    }

    //TODO: implement drag'n'drop for components
    private void DrawComponent(ref ComponentMeta meta)
    {
        EditorGUILayout.BeginVertical();
        {
            //TODO: draw tags without arrow
            meta.IsExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(meta.IsExpanded, InspectorHelper.GetTypeUIName(meta.ComponentName));
            if (meta.IsExpanded && meta.Fields != null)
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

            bool setDirty;
            if (fieldMeta.TypeName == typeof(int).FullName)
            {
                var intValue = valueObject != null ? (int)valueObject : default(int);
                setDirty = fieldMeta.SetValue(EditorGUILayout.IntField(intValue));
            }
            else if (fieldMeta.TypeName == typeof(float).FullName)
            {
                var floatValue = valueObject != null ? (float)valueObject : default(float);
                setDirty = fieldMeta.SetValue(EditorGUILayout.FloatField(floatValue));
            }
            else if (fieldMeta.TypeName == typeof(Vector3).FullName)
            {
                var vec3Value = valueObject != null ? (Vector3)valueObject : default(Vector3);
                setDirty = fieldMeta.SetValue(EditorGUILayout.Vector3Field("", vec3Value));
            }
            else
            {
                var type = EntityView.GetUnityComponentTypeByName(fieldMeta.TypeName);
                var obj = valueObject != null ? (Component)valueObject : null;
                setDirty = fieldMeta.SetValue(EditorGUILayout.ObjectField("", obj, type, true));
            }

            if (setDirty)
                EditorUtility.SetDirty(target);
        }
        EditorGUILayout.EndHorizontal();
    }
}
