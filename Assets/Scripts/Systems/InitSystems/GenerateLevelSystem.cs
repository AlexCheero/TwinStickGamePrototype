using System;
using System.Collections.Generic;
using Components;
using ECS;
using Tags;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

//choose system type here
[System(ESystemCategory.Init)]
public class GenerateLevelSystem : EcsSystem
{
    private readonly int _filterId;
    private readonly int _playerFilterId;
    private readonly int _exitFilterId;
    private readonly int _enemyFilterId;

    public GenerateLevelSystem(EcsWorld world)
    {
        _filterId = world.RegisterFilter(new BitMask(Id<LevelSettingsComponent>()));
        _playerFilterId = world.RegisterFilter(new BitMask(Id<PlayerTag>(), Id<Transform>()));
        _exitFilterId = world.RegisterFilter(new BitMask(Id<LevelExit>(), Id<Transform>()), new BitMask(Id<DeadTag>()));
        _enemyFilterId = world.RegisterFilter(new BitMask(Id<EnemyTag>()));
    }

    public override void Tick(EcsWorld world)
    {
        int settingsId = -1;
        foreach (var id in world.Enumerate(_filterId))
        {
            settingsId = id;
            break;
        }
        if (settingsId < 0)
            return;
        
        foreach (var id in world.Enumerate(_exitFilterId))
        {
            world.GetComponent<Transform>(id).gameObject.SetActive(false);
            world.Add<DeadTag>(id);
        }
        
        DisposeOldMaze();

        int playerId = -1;
        foreach (var id in world.Enumerate(_playerFilterId))
        {
            playerId = id;
            break;
        }
        
        var settings = world.GetComponent<LevelSettingsComponent>(settingsId).Settings;
            
        var settingsDigits = settings.Digits;
        if (settingsDigits.Rows % 2 == 0 && settingsDigits.Cols % 2 == 0)
            Debug.LogError("Odd numbers work better for dungeon size.");

        DisposeOldMaze();

        var data = FromDimensions(settingsDigits);

        var settingsMaterials = settings.Materials;
        DisplayMaze(data, settingsDigits.Width, settingsDigits.Height, settingsMaterials.FloorMat,
            settingsMaterials.WallMat, settingsMaterials.TransparentMat);
        var playerTransform = world.GetComponent<Transform>(playerId);
        playerTransform.gameObject.SetActive(true);
        playerTransform.position = FindStartPosition(data, settingsDigits.Width);
            
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        var goalView = go.AddComponent<EntityView>();
        goalView.transform.position = FindGoalPosition(data, settingsDigits.Width);
        go.GetComponent<BoxCollider>().isTrigger = true;
        go.GetComponent<MeshRenderer>().sharedMaterial = settingsMaterials.EndMat;
        goalView.InitAsEntity(world);
        world.Add<LevelExit>(goalView.Id);

        var nms = GameObject.FindObjectOfType<NavMeshSurface>();
        nms.BuildNavMesh();

        foreach (var lootPrefab in settings.Loot)
            PlaceLevelObjects(lootPrefab, world, data, settingsDigits.Width);
        foreach (var enemyPrefab in settings.Enemies)
            PlaceLevelObjects(enemyPrefab, world, data, settingsDigits.Width);

        var playerView = playerTransform.GetComponent<EntityView>();
        foreach (var id in world.Enumerate(_enemyFilterId))
            world.Add(id, new TargetEntityComponent { target = playerView });
    }

    private void PlaceLevelObjects(EntityView levelObjectPrefab, EcsWorld world, int[,] data, float width)
    {
        levelObjectPrefab.InitAsEntity(world);
        var count = 1;
        if (world.Have<RandomCountRange>(levelObjectPrefab.Id))
        {
            var countRange = world.GetComponent<RandomCountRange>(levelObjectPrefab.Id);
            count = Random.Range(countRange.min, countRange.max + 1);
        }
        world.Delete(levelObjectPrefab.Id);
        for (int i = 0; i < count; i++)
        {
            var lootPosition = Vector3.zero;
            if (!GetRandomFreePoint(data, width, ref lootPosition))
                return;
            var loot = GameObject.Instantiate(levelObjectPrefab, lootPosition, Quaternion.identity);
            loot.InitAsEntity(world);
            world.Add(loot.Id, new Prototype { prefab = levelObjectPrefab });
        }
    }
    
    private void DisposeOldMaze()
    {
        var objects = GameObject.FindGameObjectsWithTag("Generated");
        foreach (var go in objects) {
            GameObject.Destroy(go);
        }
    }
    
    private void DisplayMaze(int[,] data, float w, float h, Material mat1, Material mat2, Material mat3)
    {
        GameObject go = new GameObject();
        go.transform.position = Vector3.zero;
        go.name = "Procedural Maze";
        go.tag = "Generated";

        MeshFilter mf = go.AddComponent<MeshFilter>();
        mf.mesh = FromData(data, w, h);

        MeshCollider mc = go.AddComponent<MeshCollider>();
        mc.sharedMesh = mf.mesh;

        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mr.materials = new Material[3] {mat1, mat2, mat3};
    }

    private bool GetRandomFreePoint(int[,] data, float width, ref Vector3 point)
    {
        int rMax = data.GetUpperBound(0);
        int cMax = data.GetUpperBound(1);

        var rowOffset = Random.Range(0, rMax + 1);
        var row = -1;
        for (int i = 0; i <= rMax; i++)
        {
            row = (rowOffset + i) % (rMax + 1);
            bool haveFreePointsInRow = false;
            for (int j = 0; j <= cMax; j++)
            {
                if (data[row, j] != 0)
                    continue;
                haveFreePointsInRow = true;
                break;
            }
            
            if (haveFreePointsInRow)
                break;
        }

        if (row < 0)
        {
            Debug.LogError("can't find free row");
            return false;
        }

        var colOffset = Random.Range(0, cMax + 1);
        var col = -1;
        for (int j = 0; j <= cMax; j++)
        {
            col = (colOffset + j) % (cMax + 1);
            if (data[row, col] == 0)
                break;
        }
        
#if DEBUG
        if (row < 0)
            throw new Exception("can't find free column");
#endif

        data[row, col] = 2;//TODO: magick number for placed loot. use enum or something like this instead
        
        point = GetPositionByMazeCoordinates(col, row, width);
        return true;
    }
    
    private Vector3 FindGoalPosition(int[,] data, float width)
    {
        int rMax = data.GetUpperBound(0);
        int cMax = data.GetUpperBound(1);

        for (int i = 0; i <= rMax; i++)
        {
            for (int j = 0; j <= cMax; j++)
            {
                if (data[i, j] == 0)
                    return GetPositionByMazeCoordinates(j, i, width);
            }
        }

        Debug.LogError("can't find proper start position");
        return Vector3.zero;
    }
    
    private Vector3 FindStartPosition(int[,] data, float width)
    {
        int rMax = data.GetUpperBound(0);
        int cMax = data.GetUpperBound(1);

        // loop top to bottom, right to left
        for (int i = rMax; i >= 0; i--)
        {
            for (int j = cMax; j >= 0; j--)
            {
                if (data[i, j] == 0)
                    return GetPositionByMazeCoordinates(j, i, width);
            }
        }
        
        Debug.LogError("can't find proper goal position");
        return Vector3.zero;
    }

    private Vector3 GetPositionByMazeCoordinates(int col, int row, float width) =>
        new Vector3(col * width, .5f, row * width);
    
    private int[,] FromDimensions(LevelSettingsDigits levelDigits)
    {
        int[,] maze = new int[levelDigits.Rows, levelDigits.Cols];

        int rMax = maze.GetUpperBound(0);
        int cMax = maze.GetUpperBound(1);

        for (int i = 0; i <= rMax; i++)
        {
            for (int j = 0; j <= cMax; j++)
            {
                // outside wall
                if (i == 0 || j == 0 || i == rMax || j == cMax)
                {
                    maze[i, j] = 1;
                } 
                // every other inside space
                else if (i % levelDigits.GapX == 0 && j % levelDigits.GapY == 0 && Random.value > levelDigits.PlacementThreshold)
                {
                    maze[i, j] = 1;

                    // in addition to this spot, randomly place adjacent
                    int a = Random.value < .5 ? 0 : (Random.value < .5 ? -1 : 1);
                    int b = a != 0 ? 0 : (Random.value < .5 ? -1 : 1);
                    maze[i+a, j+b] = 1;
                }
            }
        }

        return maze;
    }
    
    private Mesh FromData(int[,] data, float width, float height)
    {
        Mesh maze = new Mesh();

        List<Vector3> newVertices = new List<Vector3>();
        List<Vector2> newUVs = new List<Vector2>();

        // multiple materials for floors and walls
        maze.subMeshCount = 3;
        List<int> floorTriangles = new List<int>();
        List<int> wallTriangles = new List<int>();
        List<int> wallUpTriangles = new List<int>();

        int rMax = data.GetUpperBound(0);
        int cMax = data.GetUpperBound(1);
        float halfH = height * .5f;

        for (int i = 0; i <= rMax; i++)
        {
            for (int j = 0; j <= cMax; j++)
            {
                if (data[i, j] == 1)
                {
                    // wall up surface
                    AddQuad(Matrix4x4.TRS(
                        new Vector3(j * width, height, i * width),
                        Quaternion.LookRotation(Vector3.up),
                        new Vector3(width, width, 1)
                    ), newVertices, newUVs, wallUpTriangles);
                    continue;
                }
                // floor
                AddQuad(Matrix4x4.TRS(
                    new Vector3(j * width, 0, i * width),
                    Quaternion.LookRotation(Vector3.up),
                    new Vector3(width, width, 1)
                ), newVertices, newUVs, floorTriangles);

                // ceiling
                // AddQuad(Matrix4x4.TRS(
                //     new Vector3(j * width, height, i * width),
                //     Quaternion.LookRotation(Vector3.down),
                //     new Vector3(width, width, 1)
                // ), newVertices, newUVs, floorTriangles);


                // walls on sides next to blocked grid cells

                if (i - 1 < 0 || data[i-1, j] == 1)
                {
                    AddQuad(Matrix4x4.TRS(
                        new Vector3(j * width, halfH, (i-.5f) * width),
                        Quaternion.LookRotation(Vector3.forward),
                        new Vector3(width, height, 1)
                    ), newVertices, newUVs, wallTriangles);
                }

                if (j + 1 > cMax || data[i, j+1] == 1)
                {
                    AddQuad(Matrix4x4.TRS(
                        new Vector3((j+.5f) * width, halfH, i * width),
                        Quaternion.LookRotation(Vector3.left),
                        new Vector3(width, height, 1)
                    ), newVertices, newUVs, wallTriangles);
                }

                if (j - 1 < 0 || data[i, j-1] == 1)
                {
                    AddQuad(Matrix4x4.TRS(
                        new Vector3((j-.5f) * width, halfH, i * width),
                        Quaternion.LookRotation(Vector3.right),
                        new Vector3(width, height, 1)
                    ), newVertices, newUVs, wallTriangles);
                }

                if (i + 1 > rMax || data[i+1, j] == 1)
                {
                    AddQuad(Matrix4x4.TRS(
                        new Vector3(j * width, halfH, (i+.5f) * width),
                        Quaternion.LookRotation(Vector3.back),
                        new Vector3(width, height, 1)
                    ), newVertices, newUVs, wallTriangles);
                }
            }
        }

        maze.vertices = newVertices.ToArray();
        maze.uv = newUVs.ToArray();
        
        maze.SetTriangles(floorTriangles.ToArray(), 0);
        maze.SetTriangles(wallTriangles.ToArray(), 1);
        maze.SetTriangles(wallUpTriangles.ToArray(), 2);

        maze.RecalculateNormals();

        return maze;
    }

    private void AddQuad(Matrix4x4 matrix, List<Vector3> newVertices,
        List<Vector2> newUVs, List<int> newTriangles)
    {
        int index = newVertices.Count;

        // corners before transforming
        Vector3 vert1 = new Vector3(-.5f, -.5f, 0);
        Vector3 vert2 = new Vector3(-.5f, .5f, 0);
        Vector3 vert3 = new Vector3(.5f, .5f, 0);
        Vector3 vert4 = new Vector3(.5f, -.5f, 0);

        newVertices.Add(matrix.MultiplyPoint3x4(vert1));
        newVertices.Add(matrix.MultiplyPoint3x4(vert2));
        newVertices.Add(matrix.MultiplyPoint3x4(vert3));
        newVertices.Add(matrix.MultiplyPoint3x4(vert4));

        newUVs.Add(new Vector2(1, 0));
        newUVs.Add(new Vector2(1, 1));
        newUVs.Add(new Vector2(0, 1));
        newUVs.Add(new Vector2(0, 0));

        newTriangles.Add(index+2);
        newTriangles.Add(index+1);
        newTriangles.Add(index);

        newTriangles.Add(index+3);
        newTriangles.Add(index+2);
        newTriangles.Add(index);
    }
}