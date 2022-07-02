using ECS;
using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ECSPipeline))]
public class Pipeline_Inspector : Editor
{
    //add more system types if needed
    private const string InitSystems = "Init Systems";
    private const string UpdateSystems = "Update Systems";
    private const string FixedSystems = "Fixed Update Systems";

    private static string[] initSystemTypeNames;
    private static string[] updateSystemTypeNames;
    private static string[] fixedUpdateSystemTypeNames;

    private bool _addListExpanded;

    static Pipeline_Inspector()
    {
        initSystemTypeNames = InspectorHelper.GetTypeNames<ECSPipeline>(
            (t) => IsSystemType(t) && InspectorHelper.HaveAttribute<InitSystemAttribute>(t));

        updateSystemTypeNames = InspectorHelper.GetTypeNames<ECSPipeline>(
            //basically consider system without attribute as update system is added only for consistency
            (t) =>
            {
                if (!IsSystemType(t))
                    return false;

                if (InspectorHelper.HaveAttribute<UpdateSystemAttribute>(t))
                    return true;

                var haveNoOtherAttributes = !InspectorHelper.HaveAttribute<InitSystemAttribute>(t);
                haveNoOtherAttributes &= !InspectorHelper.HaveAttribute<FixedUpdateSystemAttribute>(t);
                return haveNoOtherAttributes;
            });

        fixedUpdateSystemTypeNames = InspectorHelper.GetTypeNames<ECSPipeline>(
            (t) => IsSystemType(t) && InspectorHelper.HaveAttribute<FixedUpdateSystemAttribute>(t));
    }

    private static bool IsSystemType(Type type) => type != typeof(EcsSystem) && typeof(EcsSystem).IsAssignableFrom(type);

    //TODO: Draw current pipeline systems
    public override void OnInspectorGUI()
    {
        //var listDatas = new AddListData[]
        //{
        //    new AddListData { Caption = InitSystems, Items = initSystemTypeNames },
        //    new AddListData { Caption = UpdateSystems, Items = updateSystemTypeNames },
        //    new AddListData { Caption = FixedSystems, Items = fixedUpdateSystemTypeNames }
        //};
        //InspectorHelper.DrawAddLists(ref _addListExpanded, "Shrink systems list", "Expand systems list", listDatas, OnAddSystem);

        var listText = _addListExpanded ? "Shrink systems list" : "Expand systems list";
        if (GUILayout.Button(new GUIContent(listText), GUILayout.ExpandWidth(false)))
            _addListExpanded = !_addListExpanded;
        if (_addListExpanded)
        {
            EditorGUILayout.BeginVertical();
                InspectorHelper.DrawAddList(InitSystems, initSystemTypeNames, (name) => OnAddSystem(name, 0));
                GUILayout.Space(10);
                InspectorHelper.DrawAddList(UpdateSystems, updateSystemTypeNames, (name) => OnAddSystem(name, 1));
                GUILayout.Space(10);
                InspectorHelper.DrawAddList(FixedSystems, fixedUpdateSystemTypeNames, (name) => OnAddSystem(name, 2));
                GUILayout.Space(10);
            EditorGUILayout.EndVertical();
        }
    }

    private void OnAddSystem(string systemName, int systemCategory/*TODO: huge hack rewrite*/)
    {
        _addListExpanded = false;

        var pipeline = (ECSPipeline)target;
        if (pipeline.AddSystem(systemName, systemCategory))
            EditorUtility.SetDirty(target);
    }
}
