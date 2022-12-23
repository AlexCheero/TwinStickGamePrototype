using System.Collections.Generic;
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

            var data = FromDimensions(settingsDigits);

            var settingsMaterials = settings.Materials;
            DisplayMaze(data, settingsDigits.Width, settingsDigits.Height, settingsMaterials.FloorMat, settingsMaterials.WallMat);
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
        mf.mesh = FromData(data, w, h);

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
    
    private static Vector3 FindGoalPosition(int[,] data, float width)
    {
        int rMax = data.GetUpperBound(0);
        int cMax = data.GetUpperBound(1);

        // loop top to bottom, right to left
        for (int i = rMax; i >= 0; i--)
        {
            for (int j = cMax; j >= 0; j--)
            {
                if (data[i, j] == 0)
                    return new Vector3(j * width, .5f, i * width);
            }
        }
        
        Debug.LogError("can't find proper goal position");
        return Vector3.zero;
    }
    
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
        maze.subMeshCount = 2;
        List<int> floorTriangles = new List<int>();
        List<int> wallTriangles = new List<int>();

        int rMax = data.GetUpperBound(0);
        int cMax = data.GetUpperBound(1);
        float halfH = height * .5f;

        for (int i = 0; i <= rMax; i++)
        {
            for (int j = 0; j <= cMax; j++)
            {
                if (data[i, j] == 1)
                    continue;
                // floor
                AddQuad(Matrix4x4.TRS(
                    new Vector3(j * width, 0, i * width),
                    Quaternion.LookRotation(Vector3.up),
                    new Vector3(width, width, 1)
                ), newVertices, newUVs, floorTriangles);

                // ceiling
                AddQuad(Matrix4x4.TRS(
                    new Vector3(j * width, height, i * width),
                    Quaternion.LookRotation(Vector3.down),
                    new Vector3(width, width, 1)
                ), newVertices, newUVs, floorTriangles);


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