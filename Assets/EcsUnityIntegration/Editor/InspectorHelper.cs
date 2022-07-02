using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public struct AddListData
{
    public string Caption;
    public string[] Items;
}

//TODO: move inspector outside of Editor folder and use it for inspected objects to
//      for example move here typeof(Base).IsAssignableFrom(typeof(Derived)) or gathered types like EntityView.EcsComponentTypes
public static class InspectorHelper
{
    public static string GetTypeUIName(string fullName) => fullName.Substring(fullName.LastIndexOf('.') + 1);

    public static void DrawAddList(string label, string[] components, Action<string> onAdd)
    {
        EditorGUILayout.LabelField(label + ':');
        GUILayout.Space(10);
        foreach (var componentName in components)
        {
            EditorGUILayout.BeginHorizontal();

            //TODO: add lines between components for readability
            //      or remove "+" button and make buttons with component names on it
            EditorGUILayout.LabelField(InspectorHelper.GetTypeUIName(componentName));
            bool tryAdd = GUILayout.Button(new GUIContent("+"), GUILayout.ExpandWidth(false));
            if (tryAdd)
                onAdd(componentName);

            EditorGUILayout.EndHorizontal();
        }
    }



    public static void DrawAddLists(ref bool isExpanded,
                                    string expandedTxt,
                                    string shrinkedTxt,
                                    AddListData[] datas,
                                    Action<string> onAdd)
    {
        var listText = isExpanded ? expandedTxt : shrinkedTxt;
        if (GUILayout.Button(new GUIContent(listText), GUILayout.ExpandWidth(false)))
            isExpanded = !isExpanded;
        if (isExpanded)
        {
            EditorGUILayout.BeginVertical();
            foreach (var data in datas)
            {
                DrawAddList(data.Caption, data.Items, onAdd);
                GUILayout.Space(10);
            }
            EditorGUILayout.EndVertical();
        }
    }

    public static string[] GetTypeNames<SameAssemblyType>(Func<Type, bool> predicate)
    {
        var types = Assembly.GetAssembly(typeof(SameAssemblyType)).GetTypes().Where(predicate).ToArray();
        return Array.ConvertAll(types, (t) => t.FullName);
    }

    public static bool HaveAttribute<AttribType>(Type type) where AttribType : Attribute
        => type.GetCustomAttributes(typeof(AttribType), true).Any();
}
