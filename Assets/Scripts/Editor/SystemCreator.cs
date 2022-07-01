using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class SystemCreator : Editor
{
    [MenuItem("Assets/Create/Ecs/System", false, -1)]
    public static void CreateSystem()
    {
        var tempPath = $"{GetAssetPath()}/EcsRunSystem.cs";
        //ProjectWindowUtil.CreateScriptAssetFromTemplateFile(tempPath, "def.cs");
        //ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, action, fileName, icon, null);
    }

    private static string GetAssetPath()
    {
        var path = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (!string.IsNullOrEmpty(path) && AssetDatabase.Contains(Selection.activeObject))
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                path = Path.GetDirectoryName(path);
            }
        }
        else
        {
            path = "Assets";
        }
        return path;
    }
}
