using Components;
using ECS;
using Tags;
using UnityEngine;

//choose system type here
[System(ESystemCategory.Init)]
public class GenerateLevelSystem : EcsSystem
{
    private readonly int _filterId;
    private readonly int _playerFilterId;

    public GenerateLevelSystem(EcsWorld world)
    {
        _filterId = world.RegisterFilter(new BitMask(Id<LevelSettingsComponent>()));
        _playerFilterId = world.RegisterFilter(new BitMask(Id<PlayerTag>(), Id<Transform>()));
    }

    public override void Tick(EcsWorld world)
    {
        DisposeOldMaze();

        int playerId = -1;
        foreach (var id in world.Enumerate(_playerFilterId))
        {
            playerId = id;
            break;
        }
        
        foreach (var id in world.Enumerate(_filterId))
        {
            var settings = world.GetComponent<LevelSettingsComponent>(id).Settings;
            
            var settingsDigits = settings.Digits;
            if (settingsDigits.Rows % 2 == 0 && settingsDigits.Cols % 2 == 0)
                Debug.LogError("Odd numbers work better for dungeon size.");

            DisposeOldMaze();

            var data = MazeDataGenerator.FromDimensions(settingsDigits);

            var settingsMaterials = settings.Materials;
            DisplayMaze(data, settingsDigits.Width, settingsDigits.Height, settingsMaterials.FloorMat, settingsMaterials.WallMat);
            var playerTransform = world.GetComponent<Transform>(playerId);
            playerTransform.gameObject.SetActive(true);
            playerTransform.position = FindStartPosition(data, settingsDigits.Width);
            
            break;
        }
    }
    
    private void DisposeOldMaze()
    {
        var objects = GameObject.FindGameObjectsWithTag("Generated");
        foreach (var go in objects) {
            GameObject.Destroy(go);
        }
    }
    
    private void DisplayMaze(int[,] data, float w, float h, Material mat1, Material mat2)
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
    
    private Vector3 FindStartPosition(int[,] data, float width)
    {
        int rMax = data.GetUpperBound(0);
        int cMax = data.GetUpperBound(1);

        for (int i = 0; i <= rMax; i++)
        {
            for (int j = 0; j <= cMax; j++)
            {
                if (data[i, j] == 0)
                    return new Vector3(j * width, .5f, i * width);
            }
        }

        Debug.LogError("can't find proper start position");
        return Vector3.zero;
    }
}