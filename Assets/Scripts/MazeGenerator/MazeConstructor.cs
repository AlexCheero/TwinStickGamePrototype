/*
 * written by Joseph Hocking 2017
 * released under MIT license
 * text of license https://opensource.org/licenses/MIT
 */

using UnityEngine;

public class MazeConstructor : MonoBehaviour
{
    public LevelSettings Settings;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
            GenerateNewMaze(Settings);
    }

    private static void GenerateNewMaze(LevelSettings settings)
    {
        var settingsDigits = settings.Digits;
        if (settingsDigits.Rows % 2 == 0 && settingsDigits.Cols % 2 == 0)
            Debug.LogError("Odd numbers work better for dungeon size.");

        DisposeOldMaze();

        var data = MazeDataGenerator.FromDimensions(settingsDigits);

        var settingsMaterials = settings.Materials;
        DisplayMaze(data, settingsDigits.Width, settingsDigits.Height, settingsMaterials.FloorMat, settingsMaterials.WallMat);
        var startPos = FindStartPosition(data);
        PlaceStartTrigger(startPos, settingsDigits.Width, settingsMaterials.StartMat);
        var endPos = FindGoalPosition(data);
        PlaceGoalTrigger(endPos, settingsDigits.Width, settingsMaterials.EndMat);
    }

    private static void DisplayMaze(int[,] data, float w, float h, Material mat1, Material mat2)
    {
        GameObject go = new GameObject();
        go.transform.position = Vector3.zero;
        go.name = "Procedural Maze";
        go.tag = "Generated";

        MeshFilter mf = go.AddComponent<MeshFilter>();
        mf.mesh = MazeMeshGenerator.FromData(data, w, h);

        MeshCollider mc = go.AddComponent<MeshCollider>();
        mc.sharedMesh = mf.mesh;

        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mr.materials = new Material[2] {mat1, mat2};
    }

    private static void DisposeOldMaze()
    {
        var objects = GameObject.FindGameObjectsWithTag("Generated");
        foreach (var go in objects) {
            Destroy(go);
        }
    }

    private static Vector2Int FindStartPosition(int[,] data)
    {
        int rMax = data.GetUpperBound(0);
        int cMax = data.GetUpperBound(1);

        for (int i = 0; i <= rMax; i++)
        {
            for (int j = 0; j <= cMax; j++)
            {
                if (data[i, j] == 0)
                    return new Vector2Int(i, j);
            }
        }

        Debug.LogError("can't find proper start position");
        return Vector2Int.zero;
    }

    private static Vector2Int FindGoalPosition(int[,] data)
    {
        int rMax = data.GetUpperBound(0);
        int cMax = data.GetUpperBound(1);

        // loop top to bottom, right to left
        for (int i = rMax; i >= 0; i--)
        {
            for (int j = cMax; j >= 0; j--)
            {
                if (data[i, j] == 0)
                    return new Vector2Int(i, j);
            }
        }
        
        Debug.LogError("can't find proper goal position");
        return Vector2Int.zero;
    }

    private static void PlaceStartTrigger(Vector2Int pos, float w, Material mat)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.transform.position = new Vector3(pos.y * w, .5f, pos.x * w);
        go.name = "Start Trigger";
        go.tag = "Generated";

        go.GetComponent<BoxCollider>().isTrigger = true;
        go.GetComponent<MeshRenderer>().sharedMaterial = mat;
    }

    private static void PlaceGoalTrigger(Vector2Int pos, float w, Material mat)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.transform.position = new Vector3(pos.y * w, .5f, pos.x * w);
        go.name = "Treasure";
        go.tag = "Generated";

        go.GetComponent<BoxCollider>().isTrigger = true;
        go.GetComponent<MeshRenderer>().sharedMaterial = mat;
    }
}
