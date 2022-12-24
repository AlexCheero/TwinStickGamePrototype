using System;
using UnityEngine;

[Serializable]
public struct LevelSettingsDigits
{
    public int GapX;
    public int GapY;
    public float Width;
    public float Height;
    public int Rows;
    public int Cols;
    public float PlacementThreshold;
}

[Serializable]
public struct LevelSettingsMaterials
{
    public Material FloorMat;
    public Material WallMat;
    public Material TransparentMat;
    public Material StartMat;
    public Material EndMat;
}

[CreateAssetMenu(fileName = "LevelSettings", menuName = "Level Generator/New Level Settings", order = -1)]
public class LevelSettings : ScriptableObject
{
    public LevelSettingsDigits Digits = new LevelSettingsDigits
    {
        GapX = 2,
        GapY = 2,
        Width = 3.75f,
        Height = 3.75f,
        Rows = 17,
        Cols = 17,
        PlacementThreshold = 0.5f
    };

    public LevelSettingsMaterials Materials;
}