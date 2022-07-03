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

    private ECSPipeline Pipeline { get => (ECSPipeline)target; }

    static Pipeline_Inspector()
    {
        initSystemTypeNames = IntegrationHelper.GetTypeNames<ECSPipeline>(
            (t) => IsSystemType(t) && IntegrationHelper.HaveAttribute<InitSystemAttribute>(t));

        updateSystemTypeNames = IntegrationHelper.GetTypeNames<ECSPipeline>(
            //basically consider system without attribute as update system is added only for consistency
            (t) =>
            {
                if (!IsSystemType(t))
                    return false;

                if (IntegrationHelper.HaveAttribute<UpdateSystemAttribute>(t))
                    return true;

                var haveNoOtherAttributes = !IntegrationHelper.HaveAttribute<InitSystemAttribute>(t);
                haveNoOtherAttributes &= !IntegrationHelper.HaveAttribute<FixedUpdateSystemAttribute>(t);
                return haveNoOtherAttributes;
            });

        fixedUpdateSystemTypeNames = IntegrationHelper.GetTypeNames<ECSPipeline>(
            (t) => IsSystemType(t) && IntegrationHelper.HaveAttribute<FixedUpdateSystemAttribute>(t));
    }

    private static bool IsSystemType(Type type) => type != typeof(EcsSystem) && typeof(EcsSystem).IsAssignableFrom(type);

    public override void OnInspectorGUI()
    {
        var listText = _addListExpanded ? "Shrink systems list" : "Expand systems list";
        if (GUILayout.Button(new GUIContent(listText), GUILayout.ExpandWidth(false)))
            _addListExpanded = !_addListExpanded;
        if (_addListExpanded)
        {
            EditorGUILayout.BeginVertical();
                IntegrationHelper.DrawAddList(InitSystems, initSystemTypeNames,
                    (name) => OnAddSystem(name, ESystemCategory.Init));
                GUILayout.Space(10);
                IntegrationHelper.DrawAddList(UpdateSystems, updateSystemTypeNames,
                    (name) => OnAddSystem(name, ESystemCategory.Update));
                GUILayout.Space(10);
                IntegrationHelper.DrawAddList(FixedSystems, fixedUpdateSystemTypeNames,
                    (name) => OnAddSystem(name, ESystemCategory.FixedUpdate));
                GUILayout.Space(10);
            EditorGUILayout.EndVertical();
        }

        //===================================

        var pipeline = Pipeline;
    }

    private void OnAddSystem(string systemName, ESystemCategory systemCategory)
    {
        _addListExpanded = false;

        var pipeline = Pipeline;
        if (pipeline.AddSystem(systemName, systemCategory))
            EditorUtility.SetDirty(target);
    }
}
